using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.Models;

namespace HSMS.MVC.ViewModels.Attendance;

public class AttendanceIndexViewModel
{
    public bool CanFilter { get; init; }
    public bool CanViewHistory { get; init; }
    public AttendanceScheduleFilter Filter { get; init; } = new();
    public IReadOnlyList<AttendanceLessonDto> Lessons { get; init; } = [];
    public IReadOnlyList<AttendanceChoice> Classes { get; init; } = [];
    public int Total { get; init; }
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / 20m));
}

public record AttendanceStudentRow(Customer Student, AttendanceRecord? Record,
    IReadOnlyList<LeaveRequest> Leaves, bool InRoster);

public class AttendanceSheetViewModel
{
    public bool CanViewAudit { get; init; }
    public TimetableEntry Entry { get; init; } = null!;
    public DateTime Date { get; init; }
    public IReadOnlyList<AttendanceStudentRow> Students { get; init; } = [];
    public bool CanWrite { get; init; }
    public bool StaffMode { get; init; }
    public string? ReadOnlyReason { get; init; }
    public DateTime DeadlineUtc { get; init; }
    public SaveAttendanceDto? Attempt { get; set; }
}

public class AttendanceHistoryViewModel
{
    public bool IsLearner { get; init; }
    public bool IsParent { get; init; }
    public IReadOnlyList<AttendanceSemesterChoice> Semesters { get; init; } = [];
    public IReadOnlyList<AttendanceChoice> Subjects { get; init; } = [];
    public AttendanceHistoryFilter Filter { get; init; } = new();
    public IReadOnlyList<AttendanceRecord> Records { get; init; } = [];
    public IReadOnlyList<AttendanceChoice> Classes { get; init; } = [];
    public IReadOnlyList<AttendanceChoice> Students { get; init; } = [];
    public int Total { get; init; }
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / 30m));
}

public record AttendanceAuditViewModel(AttendanceRecord Record, IReadOnlyList<AttendanceAuditLog> Logs);
