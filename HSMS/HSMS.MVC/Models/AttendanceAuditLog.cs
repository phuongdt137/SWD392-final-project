using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

/// <summary>Immutable history of an attendance result, including the first recording.</summary>
public class AttendanceAuditLog
{
    public long Id { get; private set; }
    public long AttendanceRecordId { get; private set; }
    // OldStatus=null ở lần ghi đầu; cặp Old/New lưu ảnh chụp trước và sau thay đổi.
    public string? OldStatus { get; private set; }
    public string NewStatus { get; private set; } = null!;
    public string? OldComment { get; private set; }
    public string? NewComment { get; private set; }
    public long? OldLeaveRequestId { get; private set; }
    public long? NewLeaveRequestId { get; private set; }
    public string? ChangeReason { get; private set; }
    // Lưu Employee ID của người thao tác; đây không phải User ID hay Customer ID.
    public long ChangedByEmployeeId { get; private set; }
    public DateTime ChangedAt { get; private set; }
    public virtual AttendanceRecord AttendanceRecord { get; private set; } = null!;
    public virtual Employee ChangedByEmployee { get; private set; } = null!;
    protected AttendanceAuditLog() { }
    // Chụp giá trị mới từ record sau RecordResult; giá trị cũ được Service giữ trước khi sửa.
    public AttendanceAuditLog(AttendanceRecord record, string? oldStatus, string? oldComment,
        long? oldLeaveRequestId, string? reason, long employeeId, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (employeeId <= 0)
            throw new AppError("Id người thực hiện không hợp lệ.", "INVALID_EMPLOYEE_ID");

        if (oldStatus != null)
        {
            oldStatus = AttendanceStatuses.Validate(oldStatus);
            if (string.IsNullOrWhiteSpace(reason))
                throw new AppError("Chỉnh sửa điểm danh phải có lý do.", "CHANGE_REASON_REQUIRED");
        }

        if (reason?.Length > 1000)
            throw new AppError("Lý do chỉnh sửa tối đa 1.000 ký tự.", "CHANGE_REASON_TOO_LONG");

        if (oldComment?.Length > 1000)
            throw new AppError("Ghi chú cũ tối đa 1.000 ký tự.", "COMMENT_TOO_LONG");

        AttendanceRecord = record;
        AttendanceRecordId = record.Id;
        OldStatus = oldStatus;
        NewStatus = record.Status;
        OldComment = oldComment;
        NewComment = record.Comment;
        OldLeaveRequestId = oldLeaveRequestId;
        NewLeaveRequestId = record.LeaveRequestId;
        ChangeReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        ChangedByEmployeeId = employeeId;
        ChangedAt = utcNow;
    }
}
