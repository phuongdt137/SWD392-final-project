namespace HSMS.MVC.Models;

/// <summary>Immutable history of an attendance result, including the first recording.</summary>
public class AttendanceAuditLog
{
    public long Id { get; private set; }
    public long AttendanceRecordId { get; private set; }
    public string? OldStatus { get; private set; }
    public string NewStatus { get; private set; } = null!;
    public string? OldComment { get; private set; }
    public string? NewComment { get; private set; }
    public long? OldLeaveRequestId { get; private set; }
    public long? NewLeaveRequestId { get; private set; }
    public string? ChangeReason { get; private set; }
    public long ChangedByEmployeeId { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public virtual AttendanceRecord AttendanceRecord { get; private set; } = null!;
    public virtual Employee ChangedByEmployee { get; private set; } = null!;
    protected AttendanceAuditLog() { }
    public AttendanceAuditLog(AttendanceRecord record, string? oldStatus, string? oldComment,
        long? oldLeaveRequestId, string? reason, long employeeId, DateTime utcNow)
    {
        AttendanceRecord = record; AttendanceRecordId = record.Id;
        OldStatus = oldStatus; NewStatus = record.Status; OldComment = oldComment; NewComment = record.Comment;
        OldLeaveRequestId = oldLeaveRequestId; NewLeaveRequestId = record.LeaveRequestId;
        ChangeReason = reason; ChangedByEmployeeId = employeeId; ChangedAt = utcNow;
    }
}
