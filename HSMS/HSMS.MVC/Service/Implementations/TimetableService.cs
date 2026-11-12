using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.DTOs.Timetable;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using HSMS.MVC.Service.Interfaces;
using HSMS.MVC.ViewModels.Timetable;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HSMS.MVC.Service.Implementations;

/// <summary>
/// Thực thi Service cho phân hệ Thời khóa biểu (TimetableService).
/// Tuân thủ quy trình xử lý của Sequence Diagram.
/// </summary>
public class TimetableService : ITimetableService
{
    private readonly ITimetableRepository _repository;

    public TimetableService(ITimetableRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<ServiceResult<CreateAssignmentResult>> CreateAssignmentAndTimetableAsync(CreateTeachingAssignmentRequest request)
    {
        // -------------------------------------------------------------
        // Step 4 & 5: validate(request) -> Repository kiểm tra dữ liệu
        // -------------------------------------------------------------
        var validation = await _repository.ValidateRequestAsync(request);
        if (!validation.IsValid)
        {
            var errorMessage = string.Join("; ", validation.Errors);
            return ServiceResult<CreateAssignmentResult>.BadRequest(
                errorMessage,
                errorCode: "VALIDATION_FAILED",
                errors: validation.Errors);
        }

        // -------------------------------------------------------------
        // Step 7 & 8: checkScheduleConflict -> Repository tìm xung đột
        // -------------------------------------------------------------
        var hasConflict = await _repository.FindScheduleConflictAsync(
            teacherId: request.TeacherId,
            classId: request.ClassId,
            roomId: request.RoomId,
            dayOfWeek: request.DayOfWeek,
            timeSlotId: request.TimeSlotId,
            effectiveFrom: request.EffectiveFrom,
            effectiveTo: request.EffectiveTo);

        if (hasConflict)
        {
            // Xung đột lịch dạy (giáo viên, lớp học hoặc phòng học đã có lịch)
            return ServiceResult<CreateAssignmentResult>.Failed(
                new AppError(
                    "Xung đột lịch dạy: Giáo viên, Lớp học hoặc Phòng học đã có lịch tại Thứ và Tiết học này trong khoảng thời gian hiệu lực.",
                    "SCHEDULE_CONFLICT",
                    ErrorType.Conflict),
                statusCode: 409);
        }

        // -------------------------------------------------------------
        // Step 11 & 12: save(assignment) -> Repository lưu TeachingAssignment
        // -------------------------------------------------------------
        var assignment = new TeachingAssignment(
            employeeId: request.TeacherId,
            subjectId: request.SubjectId,
            classId: request.ClassId,
            semesterId: request.SemesterId,
            status: "ACTIVE");

        var assignmentId = await _repository.SaveAssignmentAsync(assignment);

        // -------------------------------------------------------------
        // Step 15 & 16: save(timetableEntry) -> Repository lưu TimetableEntry
        // -------------------------------------------------------------
        var timetableEntry = new TimetableEntry(
            teachingAssignmentId: assignmentId,
            roomId: request.RoomId,
            timeSlotId: request.TimeSlotId,
            dayOfWeek: request.DayOfWeek,
            effectiveFrom: request.EffectiveFrom,
            effectiveTo: request.EffectiveTo,
            status: "ACTIVE");

        var timetableId = await _repository.SaveTimetableEntryAsync(timetableEntry);

        // -------------------------------------------------------------
        // Step 19: Return createAssignmentResult(timetableId, assignmentId)
        // -------------------------------------------------------------
        var result = new CreateAssignmentResult(
            assignmentId: assignmentId,
            timetableId: timetableId,
            isSuccess: true,
            message: "Xếp lịch dạy và phân công giảng dạy thành công!");

        return ServiceResult<CreateAssignmentResult>.Success(result, statusCode: 201);
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TimetableIndexViewModel>> GetTimetableIndexViewModelAsync(
        TimetableFilterDto filter,
        LoginResponseDto currentUser)
    {
        filter ??= new TimetableFilterDto();

        var viewModel = new TimetableIndexViewModel
        {
            SelectedSemesterId = filter.SemesterId,
            SelectedClassId = filter.ClassId,
            SelectedTeacherId = filter.TeacherId,
            SelectedRoomId = filter.RoomId,
            SelectedStudentCustomerId = filter.StudentCustomerId
        };

        // Xác định vai trò chính và quyền hạn
        var isStaffOrAdmin = currentUser.Has("STAFF") || currentUser.Has("PRINCIPAL") || currentUser.Has("ADMIN");
        var isTeacher = currentUser.Has("TEACHER");
        var isStudent = currentUser.Has("STUDENT");
        var isParent = currentUser.Has("PARENT");

        viewModel.CanCreate = isStaffOrAdmin;

        // Tải các cấu trúc chung: Tiết học và Học kỳ
        var allTimeSlots = await _repository.GetTimeSlotsAsync();
        viewModel.TimeSlots = allTimeSlots.OrderBy(ts => ts.PeriodNo).ToList();

        var allSemesters = await _repository.GetSemestersAsync();
        var currentSemester = await _repository.GetCurrentSemesterAsync();

        if (!viewModel.SelectedSemesterId.HasValue && currentSemester != null)
        {
            viewModel.SelectedSemesterId = currentSemester.Id;
            filter.SemesterId = currentSemester.Id;
        }

        viewModel.Semesters = allSemesters.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = $"{s.SemesterName} ({s.SemesterCode})",
            Selected = s.Id == viewModel.SelectedSemesterId
        }).ToList();

        // Xử lý dữ liệu hiển thị theo từng phân hệ người dùng
        if (isTeacher && !isStaffOrAdmin)
        {
            // GIÁO VIÊN: Chỉ xem lịch dạy của chính mình
            viewModel.UserRole = "TEACHER";
            viewModel.DisplayTitle = $"Lịch dạy giáo viên: {currentUser.FullName}";
            viewModel.Subtitle = "Xem lịch giảng dạy các lớp được phân công trong tuần (Chế độ chỉ xem)";

            if (currentUser.EmployeeId.HasValue)
            {
                filter.TeacherId = currentUser.EmployeeId.Value;
                viewModel.SelectedTeacherId = currentUser.EmployeeId.Value;
                viewModel.TimetableItems = (await _repository.GetByTeacherAsync(currentUser.EmployeeId.Value, filter.SemesterId)).ToList();
            }
            else
            {
                viewModel.TimetableItems = new List<TimetableItemDto>();
            }
        }
        else if (isStudent && !isStaffOrAdmin)
        {
            // HỌC SINH: Chỉ xem thời khóa biểu lớp của mình
            viewModel.UserRole = "STUDENT";
            if (currentUser.CustomerId.HasValue)
            {
                var studentClass = await _repository.GetCurrentClassOfStudentAsync(currentUser.CustomerId.Value);
                if (studentClass != null)
                {
                    viewModel.DisplayTitle = $"Thời khóa biểu lớp {studentClass.ClassName}";
                    viewModel.Subtitle = $"Lịch học của học sinh {currentUser.FullName} (Chế độ chỉ xem)";
                    filter.ClassId = studentClass.Id;
                    viewModel.SelectedClassId = studentClass.Id;
                    viewModel.TimetableItems = (await _repository.GetByClassAsync(studentClass.Id, filter.SemesterId)).ToList();
                }
                else
                {
                    viewModel.DisplayTitle = "Thời khóa biểu học sinh";
                    viewModel.Subtitle = "Chưa được gán vào lớp học nào.";
                    viewModel.TimetableItems = new List<TimetableItemDto>();
                }
            }
        }
        else if (isParent && !isStaffOrAdmin)
        {
            // PHỤ HUYNH: Chỉ xem thời khóa biểu của con
            viewModel.UserRole = "PARENT";
            if (currentUser.CustomerId.HasValue)
            {
                var children = await _repository.GetChildrenByParentCustomerIdAsync(currentUser.CustomerId.Value);
                viewModel.Children = children.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = $"{c.FullName} {(string.IsNullOrWhiteSpace(c.StudentCode) ? "" : $"({c.StudentCode})")}",
                    Selected = c.Id == viewModel.SelectedStudentCustomerId
                }).ToList();

                var targetChild = children.FirstOrDefault(c => c.Id == viewModel.SelectedStudentCustomerId)
                               ?? children.FirstOrDefault();

                if (targetChild != null)
                {
                    viewModel.SelectedStudentCustomerId = targetChild.Id;
                    var childClass = await _repository.GetCurrentClassOfStudentAsync(targetChild.Id);
                    if (childClass != null)
                    {
                        viewModel.DisplayTitle = $"Thời khóa biểu của học sinh: {targetChild.FullName} ({childClass.ClassName})";
                        viewModel.Subtitle = "Thời khóa biểu theo tuần của con (Chế độ chỉ xem)";
                        filter.ClassId = childClass.Id;
                        viewModel.SelectedClassId = childClass.Id;
                        viewModel.TimetableItems = (await _repository.GetByClassAsync(childClass.Id, filter.SemesterId)).ToList();
                    }
                    else
                    {
                        viewModel.DisplayTitle = $"Thời khóa biểu của: {targetChild.FullName}";
                        viewModel.Subtitle = "Học sinh hiện chưa được xếp vào lớp học nào.";
                        viewModel.TimetableItems = new List<TimetableItemDto>();
                    }
                }
            }
        }
        else
        {
            // CÁN BỘ HỌC VỤ (STAFF / PRINCIPAL / ADMIN): Toàn quyền xem và đặt lịch
            viewModel.UserRole = "STAFF";
            viewModel.DisplayTitle = "Quản lý Thời khóa biểu & Lịch dạy";
            viewModel.Subtitle = "Tra cứu và xếp lịch học, phân công giảng dạy cho giáo viên và lớp học.";

            // Tải danh mục dropdown cho bộ lọc
            var classes = await _repository.GetActiveClassesAsync();
            viewModel.Classes = classes.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.ClassName,
                Selected = c.Id == viewModel.SelectedClassId
            }).ToList();

            var teachers = await _repository.GetActiveTeachersAsync();
            viewModel.Teachers = teachers.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"{t.FullName} ({t.EmployeeCode})",
                Selected = t.Id == viewModel.SelectedTeacherId
            }).ToList();

            var rooms = await _repository.GetActiveRoomsAsync();
            viewModel.Rooms = rooms.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = $"{r.RoomCode} {(string.IsNullOrWhiteSpace(r.RoomName) ? "" : $"- {r.RoomName}")}",
                Selected = r.Id == viewModel.SelectedRoomId
            }).ToList();

            // Nếu Staff chưa chọn bộ lọc nào thì mặc định lấy lớp đầu tiên để bảng không trống
            if (!filter.ClassId.HasValue && !filter.TeacherId.HasValue && !filter.RoomId.HasValue && classes.Count > 0)
            {
                filter.ClassId = classes.First().Id;
                viewModel.SelectedClassId = filter.ClassId;
            }

            viewModel.TimetableItems = (await _repository.GetTimetableItemsAsync(filter)).ToList();
        }

        return ServiceResult<TimetableIndexViewModel>.Success(viewModel);
    }

    /// <inheritdoc />
    public async Task<CreateTimetableViewModel> GetCreateTimetableViewModelAsync(CreateTeachingAssignmentRequest? initialRequest = null)
    {
        var vm = new CreateTimetableViewModel
        {
            Request = initialRequest ?? new CreateTeachingAssignmentRequest
            {
                DayOfWeek = 1
            }
        };

        var teachers = await _repository.GetActiveTeachersAsync();
        vm.Teachers = teachers.Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            Text = $"{t.FullName} ({t.EmployeeCode})"
        }).ToList();

        var subjects = await _repository.GetSubjectsAsync();
        vm.Subjects = subjects.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = $"{s.SubjectName} ({s.SubjectCode})"
        }).ToList();

        var classes = await _repository.GetActiveClassesAsync();
        vm.Classes = classes.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.ClassName
        }).ToList();

        var semesters = await _repository.GetSemestersAsync();
        var currentSemester = await _repository.GetCurrentSemesterAsync();
        vm.Semesters = semesters.Select(s => new SelectListItem
        {
            Value = s.Id.ToString(),
            Text = $"{s.SemesterName} ({s.SemesterCode})",
            Selected = currentSemester != null && s.Id == currentSemester.Id
        }).ToList();

        if (vm.Request.SemesterId == 0 && currentSemester != null)
        {
            vm.Request.SemesterId = currentSemester.Id;
        }

        var rooms = await _repository.GetActiveRoomsAsync();
        vm.Rooms = rooms.Select(r => new SelectListItem
        {
            Value = r.Id.ToString(),
            Text = $"{r.RoomCode} {(string.IsNullOrWhiteSpace(r.RoomName) ? "" : $"- {r.RoomName}")}"
        }).ToList();

        var timeSlots = await _repository.GetTimeSlotsAsync();
        vm.TimeSlots = timeSlots.Select(ts => new SelectListItem
        {
            Value = ts.Id.ToString(),
            Text = $"Tiết {ts.PeriodNo} ({ts.StartTime:hh\\:mm} - {ts.EndTime:hh\\:mm}) [{ts.SlotCode}]"
        }).ToList();

        return vm;
    }

    /// <inheritdoc />
    public async Task<TeachingAssignmentResponseDto> MapToResponseDtoAsync(CreateAssignmentResult result)
    {
        var entry = await _repository.GetTimetableEntryByIdAsync(result.TimetableId);
        if (entry == null)
        {
            return new TeachingAssignmentResponseDto
            {
                AssignmentId = result.AssignmentId,
                TimetableId = result.TimetableId,
                Message = result.Message
            };
        }

        var dayName = entry.DayOfWeek switch
        {
            1 => "Thứ Hai",
            2 => "Thứ Ba",
            3 => "Thứ Tư",
            4 => "Thứ Năm",
            5 => "Thứ Sáu",
            6 => "Thứ Bảy",
            7 => "Chủ Nhật",
            _ => $"Thứ {entry.DayOfWeek}"
        };

        return new TeachingAssignmentResponseDto
        {
            AssignmentId = result.AssignmentId,
            TimetableId = result.TimetableId,
            TeacherName = entry.TeachingAssignment?.Employee?.FullName ?? string.Empty,
            SubjectName = entry.TeachingAssignment?.Subject?.SubjectName ?? string.Empty,
            ClassName = entry.TeachingAssignment?.Class?.ClassName ?? string.Empty,
            RoomCode = entry.Room?.RoomCode ?? string.Empty,
            SlotCode = entry.TimeSlot?.SlotCode ?? string.Empty,
            DayOfWeek = entry.DayOfWeek,
            DayOfWeekName = dayName,
            StartTime = entry.TimeSlot?.StartTime ?? TimeSpan.Zero,
            EndTime = entry.TimeSlot?.EndTime ?? TimeSpan.Zero,
            Status = entry.Status,
            Message = result.Message
        };
    }
}

