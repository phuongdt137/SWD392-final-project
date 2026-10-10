namespace HSMS.Model.Model;

public class AttendanceRecord
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long RecordedByEmployeeId { get; set; }
    public long? LeaveRequestId { get; set; }
    public long TimetableEntryId { get; set; }
    public DateTime AttendanceDate { get; set; }
    public string Status { get; set; } = null!;
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual Employee RecordedByEmployee { get; set; } = null!;
    public virtual LeaveRequest? LeaveRequest { get; set; }
    public virtual TimetableEntry TimetableEntry { get; set; } = null!;
}
