using HSMS.MVC.DTOs;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;

namespace HSMS.MVC.Service;

public class TimetableService : ITimetableService
{
    private readonly ITimetableRepository _repo;

    public TimetableService(ITimetableRepository repo)
    {
        _repo = repo;
    }

    // =====================================================================
    // BƯỚC 3: createAssignmentAndTimetable(request)
    // =====================================================================
    public AssignmentResponseDto CreateAssignmentAndTimetable(CreateAssignmentRequest request)
    {
        // Bước 4-6: validate dữ liệu + tham chiếu
        using var transaction = _repo.BeginTransaction();
        var errors = _repo.Validate(request);
        if (errors.Count > 0)
            return Fail(string.Join(" ", errors), isConflict: false);

        // Bước 7-10: kiểm tra trùng lịch
        var conflict = _repo.FindScheduleConflict(request);
        if (conflict != null)
            return Fail(BuildConflictMessage(request, conflict), isConflict: true);

        // Dùng transaction: lưu 2 bảng, nếu bước nào lỗi thì rollback cả hai
        // Bước 11-14: save(assignment) -> assignmentId
        var assignment = new TeachingAssignment(request.EmployeeId, request.SubjectId, request.ClassId, request.SemesterId);
        long assignmentId = _repo.SaveAssignment(assignment);

        // Bước 15-18: save(timetableEntry) -> timetableId
        var entry = new TimetableEntry(assignmentId, request.RoomId, request.TimeSlotId, (byte)request.DayOfWeek, request.EffectiveFrom, request.EffectiveTo);
        long timetableId = _repo.SaveTimetableEntry(entry);

        transaction.Commit();

        // Bước 19: trả createAssignmentResult(timetableId, assignmentId) rồi mapToResponseDto
        var result = new CreateAssignmentResult { AssignmentId = assignmentId, TimetableId = timetableId };
        return MapToResponseDto(result);
    }

    // Bước 19: mapToResponseDto(result)
    private static AssignmentResponseDto MapToResponseDto(CreateAssignmentResult result)
    {
        return new AssignmentResponseDto
        {
            Success = true,
            IsConflict = false,
            Message = "Tạo phân công thành công!",
            AssignmentId = result.AssignmentId,
            TimetableId = result.TimetableId
        };
    }

    private static AssignmentResponseDto Fail(string message, bool isConflict)
    {
        return new AssignmentResponseDto { Success = false, IsConflict = isConflict, Message = message };
    }

    // Xác định trùng giáo viên, lớp hay phòng để báo lỗi cho dễ hiểu
    private static string BuildConflictMessage(CreateAssignmentRequest r, TimetableEntry c)
    {
        if (c.TeachingAssignment.EmployeeId == r.EmployeeId)
            return "Giáo viên đã có lịch dạy khác trong khung giờ này.";
        if (c.TeachingAssignment.ClassId == r.ClassId)
            return "Lớp đã có tiết học khác trong khung giờ này.";
        return "Phòng học đã được sử dụng trong khung giờ này.";
    }

    // =====================================================================
    // DỮ LIỆU CHO FORM
    // =====================================================================
    public FormOptionsDto GetFormOptions()
    {
        return new FormOptionsDto
        {
            Employees = _repo.GetEmployees().Select(x => new OptionDto { Id = x.Id, Text = $"{x.FullName} ({x.EmployeeCode})" }).ToList(),
            Subjects = _repo.GetSubjects().Select(x => new OptionDto { Id = x.Id, Text = x.SubjectName }).ToList(),
            Classes = _repo.GetClasses().Select(x => new OptionDto { Id = x.Id, Text = x.ClassName }).ToList(),
            Semesters = _repo.GetSemesters().Select(x => new OptionDto { Id = x.Id, Text = x.SemesterName }).ToList(),
            Rooms = _repo.GetRooms().Select(x => new OptionDto { Id = x.Id, Text = x.RoomName ?? x.RoomCode }).ToList(),
            TimeSlots = _repo.GetTimeSlots().Select(x => new OptionDto { Id = x.Id, Text = $"Tiết {x.PeriodNo} ({x.StartTime:hh\\:mm} - {x.EndTime:hh\\:mm})" }).ToList()
        };
    }

    // =====================================================================
    // XEM LỊCH
    // =====================================================================

    // Staff: xem tất cả, có thể lọc theo lớp / giáo viên
    public WeeklyTimetableDto GetStaffTimetable(long? classId, long? employeeId, DateTime week)
    {
        string title = "Thời khóa biểu toàn trường";
        if (classId.HasValue) title = "Thời khóa biểu lớp " + _repo.GetClassName(classId.Value);
        else if (employeeId.HasValue) title = "Thời khóa biểu giáo viên " + _repo.GetEmployeeName(employeeId.Value);

        return BuildWeekly(title, employeeId, classId, week);
    }

    // Giáo viên: chỉ xem lịch dạy của chính mình
    public WeeklyTimetableDto GetTeacherTimetable(long employeeId, DateTime week)
    {
        string name = _repo.GetEmployeeName(employeeId) ?? "";
        return BuildWeekly("Lịch dạy của giáo viên " + name, employeeId, null, week);
    }

    // Học sinh: tìm lớp của học sinh rồi xem lịch của lớp đó
    public WeeklyTimetableDto GetStudentTimetable(long customerId, DateTime week)
    {
        return BuildWeekly("Thời khóa biểu của " + _repo.GetStudentName(customerId), null, null, week, customerId);
    }
    // Hàm dùng chung: lấy dữ liệu từ repo và đổi sang DTO để View vẽ bảng
    private WeeklyTimetableDto BuildWeekly(string title, long? employeeId, long? classId, DateTime week, long? customerId = null)
    {
        var monday = week.Date.AddDays(1 - HSMS.MVC.Common.SchoolTime.Weekday(week));
        var items = _repo.GetEntries(employeeId, classId, monday, customerId)
            .Select(e => new TimetableItemDto
            {
                DayOfWeek = e.DayOfWeek,
                TimeSlotId = e.TimeSlotId,
                SubjectName = e.TeachingAssignment.Subject.SubjectName,
                ClassName = e.TeachingAssignment.Class.ClassName,
                TeacherName = e.TeachingAssignment.Employee.FullName,
                RoomName = e.Room.RoomName ?? e.Room.RoomCode
            }).ToList();

        return new WeeklyTimetableDto { WeekStart = monday, Title = title, Slots = MapSlots(), Items = items };
    }

    private List<SlotDto> MapSlots()
    {
        return _repo.GetTimeSlots().Select(s => new SlotDto
        {
            Id = s.Id,
            PeriodNo = s.PeriodNo,
            TimeText = $"{s.StartTime:hh\\:mm} - {s.EndTime:hh\\:mm}"
        }).ToList();
    }
}



