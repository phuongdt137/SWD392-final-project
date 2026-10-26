using System.ComponentModel.DataAnnotations;
using HSMS.MVC.Models;

namespace HSMS.MVC.DTOs.Attendance;

public sealed class SaveAttendanceDto
{
    [Range(1, long.MaxValue)] public long TimetableEntryId { get; set; }
    public DateTime Date { get; set; }
    [Required] public string RosterToken { get; set; } = "";
    [StringLength(1000)] public string? ChangeReason { get; set; }
    public List<AttendanceInput> Rows { get; set; } = [];
}
public sealed class AttendanceInput
{
    public long CustomerId { get; set; }
    [Required] public string Status { get; set; } = "";
    [StringLength(1000)] public string? Comment { get; set; }
    public long? LeaveRequestId { get; set; }
    public Guid? Version { get; set; }
}
public sealed class AttendanceFilter
{
    public long? SemesterId { get; set; }
    public long? TeachingAssignmentId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public long? StudentId { get; set; }
    public long? ClassId { get; set; }
    public long? SubjectId { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
}
public sealed record SaveAttendanceResult(int Changed, int MissingParents);
public sealed record StudentChoice(long Id, string Name);
public sealed record AttendanceChoices(IReadOnlyList<StudentChoice> Students, IReadOnlyList<StudentChoice> Classes, IReadOnlyList<StudentChoice> Subjects);
public sealed class AttendanceScheduleFilter
{
    public DateTime? Date { get; set; }
    public long? GradeLevelId { get; set; }
    public long? ClassId { get; set; }
    public long? SubjectId { get; set; }
    public long? TeacherId { get; set; }
    public string? Status { get; set; }
    [StringLength(100)] public string? Query { get; set; }
    public int Page { get; set; } = 1;
    public const int PageSize = 20;
}
public sealed record AttendanceScheduleChoices(IReadOnlyList<StudentChoice> Grades, IReadOnlyList<StudentChoice> Classes,
    IReadOnlyList<StudentChoice> Subjects, IReadOnlyList<StudentChoice> Teachers);
public sealed record AttendanceScheduleRow(TimetableEntry Entry, int Students, int Recorded);
public sealed record AttendanceScheduleResult(IReadOnlyList<AttendanceScheduleRow> Rows, int Total, int Classes,
    int Completed, int Pending, int Page, AttendanceScheduleChoices Choices);
public sealed record AttendanceSummary(int Present, int Absent, int Late, int Excused, int Pending = 0)
{
    public int Recorded => Present + Absent + Late + Excused;
    public decimal? Rate => Recorded == 0 ? null : Math.Round(100m * (Present + Late) / Recorded, 1);
}
