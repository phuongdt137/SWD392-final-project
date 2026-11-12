using System.ComponentModel.DataAnnotations;
using HSMS.MVC.Models;

namespace HSMS.MVC.DTOs.Attendance;

public class AttendanceScheduleFilter
{
    public DateTime? Date { get; set; }
    public long? ClassId { get; set; }
    public int Page { get; set; } = 1;
}

public class AttendanceHistoryFilter
{
    public long? SemesterId { get; set; }
    public long? SubjectId { get; set; }
    public DateTime? Date { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public long? ClassId { get; set; }
    public long? StudentId { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
}

public class SaveAttendanceDto
{
    [Range(1, long.MaxValue)]
    public long TimetableEntryId { get; set; }
    public DateTime Date { get; set; }
    [StringLength(1000)]
    public string? ChangeReason { get; set; }
    public List<AttendanceInputDto> Rows { get; set; } = [];
}

public class AttendanceInputDto
{
    [Range(1, long.MaxValue)]
    public long CustomerId { get; set; }
    [Required(ErrorMessage = "Còn học sinh chưa được điểm danh. Vui lòng chọn đủ trạng thái trước khi lưu.")]
    public string Status { get; set; } = string.Empty;
    [StringLength(1000)]
    public string? Comment { get; set; }
    public long? LeaveRequestId { get; set; }
}

public record AttendanceChoice(long Id, string Name);
public record AttendanceSemesterChoice(long Id, string Name, DateTime StartDate, DateTime EndDate);
public record AttendanceLessonDto(TimetableEntry Entry, int Students, int Recorded);
public record AttendanceSaveResult(int Changed, int MissingParents, int MissingEmails);
