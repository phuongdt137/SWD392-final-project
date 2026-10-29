using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Academic;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using HSMS.MVC.Service.Interfaces;

namespace HSMS.MVC.Service.Implementations
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly IAcademicYearRepository _repo;

        public AcademicYearService(IAcademicYearRepository repo)
        {
            _repo = repo;
        }

        public async Task<ServiceResult<List<AcademicYearListItemDto>>> GetAllAsync()
        {
            var years = await _repo.GetAllWithDetailsAsync();

            var list = years.Select(y => new AcademicYearListItemDto
            {
                Id = y.Id,
                YearCode = y.YearCode,
                StartDate = y.StartDate,
                EndDate = y.EndDate,
                SemesterCount = y.Semesters.Count,
                ClassCount = y.Classes.Count,
                IsCurrent = y.IsCurrent()
            }).ToList();

            return ServiceResult<List<AcademicYearListItemDto>>.Success(list);
        }

        public async Task<ServiceResult<AcademicYearDetailDto>> GetByIdAsync(long id)
        {
            var year = await _repo.GetWithSemestersAsync(id);
            if (year == null)
                return ServiceResult<AcademicYearDetailDto>.NotFound("Không tìm thấy năm học.");

            var dto = new AcademicYearDetailDto
            {
                Id = year.Id,
                YearCode = year.YearCode,
                StartDate = year.StartDate,
                EndDate = year.EndDate,
                Semesters = year.Semesters
                    .OrderBy(s => s.StartDate)
                    .Select(s => new SemesterDto
                    {
                        Id = s.Id,
                        SemesterCode = s.SemesterCode,
                        SemesterName = s.SemesterName,
                        StartDate = s.StartDate,
                        EndDate = s.EndDate
                    }).ToList()
            };
            return ServiceResult<AcademicYearDetailDto>.Success(dto);
        }

        public async Task<ServiceResult<long>> CreateAsync(SaveAcademicYearDto dto)
        {
            try
            {
                var code = dto.YearCode.Trim();
                var start = dto.StartDate!.Value.Date;
                var end = dto.EndDate!.Value.Date;

                // Constructor của AcademicYear tự kiểm tra mã rỗng / ngày sai và ném AppError
                var year = new AcademicYear(code, start, end);

                if (await _repo.CodeExistsAsync(code))
                    return ServiceResult<long>.Failed("Mã năm học đã tồn tại.", "YEAR_CODE_DUPLICATE", 409);

                if (await _repo.OverlapsAsync(start, end))
                    return ServiceResult<long>.Failed("Khoảng thời gian bị chồng lên một năm học khác.", "YEAR_OVERLAP", 409);

                await _repo.AddAsync(year);
                await _repo.SaveChangesAsync();

                return ServiceResult<long>.Success(year.Id, "Tạo năm học thành công.", 201);
            }
            catch (AppError ex)
            {
                return ServiceResult<long>.Failed(ex);
            }
        }

        public async Task<ServiceResult> UpdateAsync(SaveAcademicYearDto dto)
        {
            var year = await _repo.GetWithSemestersAsync(dto.Id);
            if (year == null)
                return ServiceResult.NotFound("Không tìm thấy năm học.");

            var code = dto.YearCode.Trim();
            var start = dto.StartDate!.Value.Date;
            var end = dto.EndDate!.Value.Date;

            if (code == "")
                return ServiceResult.BadRequest("Mã năm học không được để trống.");

            if (end < start)
                return ServiceResult.BadRequest("Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");

            if (await _repo.CodeExistsAsync(code, year.Id))
                return ServiceResult.Failed("Mã năm học đã tồn tại.", "YEAR_CODE_DUPLICATE", 409);

            if (await _repo.OverlapsAsync(start, end, year.Id))
                return ServiceResult.Failed("Khoảng thời gian bị chồng lên một năm học khác.", "YEAR_OVERLAP", 409);

            // Không để học kỳ nào nằm ngoài khoảng ngày mới
            if (year.Semesters.Any(s => s.StartDate < start || s.EndDate > end))
                return ServiceResult.BadRequest("Có học kỳ nằm ngoài khoảng ngày mới của năm học.");

            _repo.UpdateInfo(year, code, start, end);
            await _repo.SaveChangesAsync();

            return ServiceResult.Success("Cập nhật năm học thành công.");
        }

        public async Task<ServiceResult<long>> CreateSemesterAsync(SaveSemesterDto dto)
        {
            try
            {
                var year = await _repo.GetWithSemestersAsync(dto.AcademicYearId);
                if (year == null)
                    return ServiceResult<long>.NotFound("Không tìm thấy năm học.");

                var start = dto.StartDate!.Value.Date;
                var end = dto.EndDate!.Value.Date;

                // Constructor của Semester tự kiểm tra dữ liệu cơ bản và ném AppError
                var semester = new Semester(dto.SemesterCode, dto.SemesterName, start, end, year.Id);

                if (start < year.StartDate || end > year.EndDate)
                    return ServiceResult<long>.BadRequest("Học kỳ phải nằm trong khoảng ngày của năm học.");

                if (year.Semesters.Any(s => s.StartDate <= end && s.EndDate >= start))
                    return ServiceResult<long>.Failed("Học kỳ bị chồng lên một học kỳ khác.", "SEMESTER_OVERLAP", 409);

                if (await _repo.SemesterCodeExistsAsync(semester.SemesterCode))
                    return ServiceResult<long>.Failed("Mã học kỳ đã tồn tại.", "SEMESTER_CODE_DUPLICATE", 409);

                await _repo.AddSemesterAsync(semester);
                await _repo.SaveChangesAsync();

                return ServiceResult<long>.Success(semester.Id, "Thêm học kỳ thành công.", 201);
            }
            catch (AppError ex)
            {
                return ServiceResult<long>.Failed(ex);
            }
        }
    }
}
