using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Academic;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using HSMS.MVC.Service.Interfaces;
using static HSMS.MVC.DTOs.Academic.ClassDtos;

namespace HSMS.MVC.Service.Implementations
{
    public class StudentClassService : IStudentClassService
    {
        private readonly IStudentClassRepository _repo;
        private readonly ICurrentUserService _currentUser;

        public StudentClassService(IStudentClassRepository repo, ICurrentUserService currentUser)
        {
            _repo = repo;
            _currentUser = currentUser;
        }

        public async Task<ServiceResult<List<SelectOptionDto>>> GetClassOptionsAsync()
        {
            var classes = await _repo.GetAllClassesAsync();

            var list = classes.Select(c => new SelectOptionDto
            {
                Id = c.Id,
                Text = c.ClassName + " (" + c.AcademicYear.YearCode + ")"
            }).ToList();

            return ServiceResult<List<SelectOptionDto>>.Success(list);
        }

        public async Task<ServiceResult<ClassStudentsDto>> GetClassStudentsAsync(long classId)
        {
            var schoolClass = await _repo.GetClassWithYearAsync(classId);
            if (schoolClass == null)
                return ServiceResult<ClassStudentsDto>.NotFound("Không tìm thấy lớp học.");

            var rows = await _repo.GetActiveStudentsOfClassAsync(classId);

            var dto = new ClassStudentsDto
            {
                ClassId = schoolClass.Id,
                ClassName = schoolClass.ClassName,
                AcademicYearCode = schoolClass.AcademicYear.YearCode,
                ClassStatus = schoolClass.Status,
                Students = rows.Select(sc => new StudentInClassDto
                {
                    StudentClassId = sc.Id,
                    StudentCode = sc.Customer.StudentCode ?? "—",
                    FullName = sc.Customer.FullName,
                    JoinedAt = sc.JoinedAt
                }).ToList()
            };
            return ServiceResult<ClassStudentsDto>.Success(dto);
        }

        public async Task<ServiceResult<AssignFormDto>> GetAssignFormAsync(long classId)
        {
            var schoolClass = await _repo.GetClassWithYearAsync(classId);
            if (schoolClass == null)
                return ServiceResult<AssignFormDto>.NotFound("Không tìm thấy lớp học.");

            if (schoolClass.Status != "ACTIVE")
                return ServiceResult<AssignFormDto>.BadRequest("Lớp đã ngừng hoạt động nên không xếp thêm học sinh.");

            var students = await _repo.GetAssignableStudentsAsync(schoolClass.AcademicYearId);

            var dto = new AssignFormDto
            {
                ClassId = schoolClass.Id,
                ClassName = schoolClass.ClassName,
                AcademicYearCode = schoolClass.AcademicYear.YearCode,
                Students = students.Select(s => new SelectOptionDto
                {
                    Id = s.Id,
                    Text = (s.StudentCode ?? "—") + " - " + s.FullName
                }).ToList()
            };
            return ServiceResult<AssignFormDto>.Success(dto);
        }

        public async Task<ServiceResult> AssignAsync(AssignStudentDto dto)
        {
            try
            {
                var schoolClass = await _repo.GetClassWithYearAsync(dto.ClassId);
                if (schoolClass == null)
                    return ServiceResult.NotFound("Không tìm thấy lớp học.");

                if (schoolClass.Status != "ACTIVE")
                    return ServiceResult.BadRequest("Lớp đã ngừng hoạt động nên không xếp thêm học sinh.");

                var customerId = dto.CustomerId!.Value;
                var joinedAt = dto.JoinedAt!.Value.Date;

                // Phải là học sinh đang hoạt động
                if (!await _repo.IsStudentAsync(customerId))
                    return ServiceResult.BadRequest("Hồ sơ này không phải học sinh đang hoạt động.");

                // Ngày vào lớp phải nằm trong năm học của lớp
                var year = schoolClass.AcademicYear;
                if (joinedAt < year.StartDate.Date || joinedAt > year.EndDate.Date)
                    return ServiceResult.BadRequest("Ngày vào lớp phải nằm trong năm học " + year.YearCode + ".");

                // Quy tắc chính: mỗi học sinh chỉ có 1 lớp ACTIVE trong 1 năm học
                if (await _repo.HasActiveInYearAsync(customerId, schoolClass.AcademicYearId))
                    return ServiceResult.Failed("Học sinh đã có lớp trong năm học này. Hãy dùng chức năng chuyển lớp.",
                        "STUDENT_ALREADY_IN_CLASS", 409);

                var studentClass = new StudentClass(customerId, schoolClass.Id, joinedAt);
                await _repo.AddAsync(studentClass);
                await _repo.SaveChangesAsync();

                return ServiceResult.Success("Đã xếp học sinh vào lớp.", 201);
            }
            catch (AppError ex)
            {
                return ServiceResult.Failed(ex);
            }
        }

        public async Task<ServiceResult<TransferFormDto>> GetTransferFormAsync(long studentClassId)
        {
            var current = await _repo.GetWithDetailsAsync(studentClassId);
            if (current == null)
                return ServiceResult<TransferFormDto>.NotFound("Không tìm thấy học sinh trong lớp.");

            if (current.Status != "ACTIVE")
                return ServiceResult<TransferFormDto>.BadRequest("Học sinh không còn học ở lớp này nên không chuyển được.");

            var targets = await _repo.GetOtherActiveClassesInYearAsync(current.Class.AcademicYearId, current.ClassId);

            var dto = new TransferFormDto
            {
                StudentClassId = current.Id,
                StudentCode = current.Customer.StudentCode ?? "—",
                StudentName = current.Customer.FullName,
                FromClassName = current.Class.ClassName,
                AcademicYearCode = current.Class.AcademicYear.YearCode,
                Classes = targets.Select(c => new SelectOptionDto { Id = c.Id, Text = c.ClassName }).ToList()
            };
            return ServiceResult<TransferFormDto>.Success(dto);
        }

        // Chuyển lớp = 3 việc trong CÙNG MỘT lần lưu (EF tự bọc trong 1 transaction):
        //   1. Đóng bản ghi lớp cũ (TRANSFERRED + LeftAt)
        //   2. Tạo bản ghi lớp mới (ACTIVE)
        //   3. Ghi lịch sử StudentTransfer (loại CLASS, ai thực hiện, lý do)
        // Dữ liệu cũ (điểm danh, điểm số) vẫn gắn với lớp cũ, không bị đụng tới.
        public async Task<ServiceResult<long>> TransferAsync(TransferStudentDto dto)
        {
            try
            {
                // Người thực hiện: lấy từ session phía server, không tin dữ liệu từ form
                var me = await _currentUser.GetAsync();
                if (me == null)
                    return ServiceResult<long>.Unauthorized();

                if (!me.EmployeeId.HasValue)
                    return ServiceResult<long>.BadRequest("Tài khoản của bạn chưa có hồ sơ nhân viên nên không ghi nhận được người chuyển lớp.");

                var current = await _repo.GetWithDetailsAsync(dto.StudentClassId);
                if (current == null)
                    return ServiceResult<long>.NotFound("Không tìm thấy học sinh trong lớp.");

                if (current.Status != "ACTIVE")
                    return ServiceResult<long>.BadRequest("Học sinh không còn học ở lớp này nên không chuyển được.");

                var toClassId = dto.ToClassId!.Value;
                if (toClassId == current.ClassId)
                    return ServiceResult<long>.BadRequest("Lớp mới phải khác lớp hiện tại.");

                var newClass = await _repo.GetClassWithYearAsync(toClassId);
                if (newClass == null)
                    return ServiceResult<long>.NotFound("Không tìm thấy lớp mới.");

                if (newClass.Status != "ACTIVE")
                    return ServiceResult<long>.BadRequest("Lớp mới đã ngừng hoạt động.");

                if (newClass.AcademicYearId != current.Class.AcademicYearId)
                    return ServiceResult<long>.BadRequest("Chỉ được chuyển lớp trong cùng một năm học.");

                var today = DateTime.Today;
                if (current.JoinedAt.Date > today)
                    return ServiceResult<long>.BadRequest("Học sinh chưa tới ngày vào lớp hiện tại nên chưa chuyển được.");

                var reason = dto.Reason.Trim();
                if (reason == "")
                    return ServiceResult<long>.BadRequest("Lý do chuyển lớp không được để trống.");

                var fromClassId = current.ClassId;

                current.Transfer(newClass);
                var created = new StudentClass(current.CustomerId, newClass.Id, today);
                var log = new StudentTransfer(current.CustomerId, "CLASS", today, reason,
                    me.EmployeeId.Value, fromClassId, newClass.Id);

                await _repo.AddAsync(created);
                await _repo.AddTransferAsync(log);
                await _repo.SaveChangesAsync();   // một lần lưu duy nhất

                return ServiceResult<long>.Success(newClass.Id, "Đã chuyển lớp.");
            }
            catch (AppError ex)
            {
                return ServiceResult<long>.Failed(ex);
            }
        }
    }
}
