using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.Models;

namespace HSMS.MVC.ViewModels.Attendance;

public sealed record AttendanceLesson(TimetableEntry Entry, int Students, int Recorded, bool Locked, bool Started)
{
    public bool CanWrite { get; init; }
    public bool Completed => Students > 0 && Recorded >= Students;
    public string Status => Locked ? "LOCKED" : !Started ? "UPCOMING" : Completed ? "COMPLETED" : "PENDING";
    public string StatusLabel => Status switch { "LOCKED" => "Đã khóa", "UPCOMING" => "Chưa đến giờ", "COMPLETED" => "Đã điểm danh", _ => "Chưa điểm danh" };
}
public sealed record AttendancePagination(int Page, int Pages, Func<int, string> PageUrl);
public sealed record AttendanceIndex(DateTime Date, IReadOnlyList<AttendanceLesson> Lessons)
{
    public AttendanceScheduleFilter Filter { get; init; } = new();
    public AttendanceScheduleChoices Choices { get; init; } = new([], [], [], []);
    public int Total { get; init; }
    public int Classes { get; init; }
    public int Completed { get; init; }
    public int Pending { get; init; }
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / (double)AttendanceScheduleFilter.PageSize));
}
public sealed record AttendanceStudent(Customer Student, AttendanceRecord? Record, IReadOnlyList<LeaveRequest> Leaves);
public sealed class AttendanceSheet
{
    public TimetableEntry Entry { get; init; } = null!;
    public DateTime Date { get; init; }
    public string RosterToken { get; init; } = "";
    public IReadOnlyList<AttendanceStudent> Students { get; init; } = [];
    public IReadOnlyList<AttendanceRecord> HistoricalRecords { get; init; } = [];
    public bool CanWrite { get; init; }
    public bool CanEditAfterDeadline { get; init; }
    public string? ReadOnlyReason { get; init; }
    public SaveAttendanceDto? Attempt { get; set; }
}
public sealed record AttendanceHistory(AttendanceFilter Filter, IReadOnlyList<AttendanceRecord> Records,
    int Total, AttendanceSummary Summary, AttendanceChoices Choices);
public sealed record AttendanceAudit(AttendanceRecord Record, IReadOnlyList<AttendanceAuditLog> Logs);
public sealed record StudentAttendanceSession(DateTime Date, TimetableEntry Entry, AttendanceRecord? Record);
public sealed class StudentAttendanceHistory
{
    public bool IsParent { get; init; }
    public IReadOnlyList<StudentChoice> Students { get; init; } = [];
    public long? StudentId { get; init; }
    public string? StudentName { get; init; }
    public DateTime LocalNow { get; init; }
    public IReadOnlyList<Semester> Semesters { get; init; } = [];
    public IReadOnlyList<TeachingAssignment> Assignments { get; init; } = [];
    public long? SemesterId { get; init; }
    public TeachingAssignment? Assignment { get; init; }
    public IReadOnlyList<StudentAttendanceSession> Sessions { get; init; } = [];
    public int Recorded { get; init; }
    public int Absent { get; init; }
    public int Unmarked { get; init; }
    public int Upcoming { get; init; }
    public decimal? AbsenceRate => Recorded == 0 ? null : Math.Round(100m * Absent / Recorded, 1);
    public int Total { get; init; }
    public int Page { get; init; } = 1;
    public const int PageSize = 30;
}
