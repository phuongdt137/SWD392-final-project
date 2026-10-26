using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

public class AttendanceRecord
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    public long RecordedByEmployeeId { get; private set; }
    public long? LeaveRequestId { get; private set; }
    public long TimetableEntryId { get; private set; }
    public DateTime AttendanceDate { get; private set; }
    public string Status { get; private set; } = null!;
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public long? UpdatedByEmployeeId { get; private set; }
    public Guid Version { get; private set; } = Guid.NewGuid();
    public virtual Employee? UpdatedByEmployee { get; private set; }

    public virtual Customer Customer { get; private set; } = null!;
    public virtual Employee RecordedByEmployee { get; private set; } = null!;
    public virtual LeaveRequest? LeaveRequest { get; private set; }
    public virtual TimetableEntry TimetableEntry { get; private set; } = null!;

    protected AttendanceRecord() { }

    public AttendanceRecord(long customerId, long timetableEntryId, long recordedByEmployeeId, DateTime attendanceDate, string status, string? comment = null, long? leaveRequestId = null, DateTime? createdAtUtc = null)
    {
        if (customerId <= 0)
            throw new AppError("Id học sinh không hợp lệ.", "INVALID_CUSTOMER_ID");

        if (timetableEntryId <= 0)
            throw new AppError("Id thời khóa biểu không hợp lệ.", "INVALID_TIMETABLE_ENTRY_ID");

        if (recordedByEmployeeId <= 0)
            throw new AppError("Id cán bộ điểm danh không hợp lệ.", "INVALID_EMPLOYEE_ID");

        if (string.IsNullOrWhiteSpace(status))
            throw new AppError("Trạng thái điểm danh không được để trống.", "INVALID_STATUS");

        CustomerId = customerId;
        TimetableEntryId = timetableEntryId;
        RecordedByEmployeeId = recordedByEmployeeId;
        AttendanceDate = attendanceDate.Date;
        Status = AttendanceStatuses.Validate(status);
        if (Status == AttendanceStatuses.Excused && !leaveRequestId.HasValue)
            throw new AppError("Vắng có phép cần đơn nghỉ được duyệt.", "LEAVE_REQUIRED");
        if (comment?.Length > 1000) throw new AppError("Ghi chú tối đa 1.000 ký tự.");
        Comment = comment?.Trim();
        LeaveRequestId = Status == AttendanceStatuses.Excused ? leaveRequestId : null;
        CreatedAt = createdAtUtc ?? DateTime.UtcNow;
    }

    public void Mark(string status, string? comment = null)
    {
        status = AttendanceStatuses.Validate(status);
        if (status == AttendanceStatuses.Excused)
            throw new AppError("Dùng đơn nghỉ đã duyệt để ghi nhận vắng có phép.", "LEAVE_REQUIRED");
        if (comment?.Length > 1000) throw new AppError("Ghi chú tối đa 1.000 ký tự.");
        Status = status; Comment = comment?.Trim(); LeaveRequestId = null; LeaveRequest = null;
        Version = Guid.NewGuid();
    }

    public void ApplyApprovedLeave(LeaveRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.StudentCustomerId != CustomerId)
            throw new AppError("Đơn nghỉ không thuộc học sinh này.", "LEAVE_STUDENT_MISMATCH");
        if (request.Status != "APPROVED")
            throw new AppError("Đơn xin nghỉ phép chưa được duyệt.", "LEAVE_NOT_APPROVED");

        if (AttendanceDate < request.StartDate || AttendanceDate > request.EndDate)
            throw new AppError("Ngày điểm danh không nằm trong khoảng thời gian nghỉ phép của đơn.", "DATE_OUT_OF_LEAVE_RANGE");

        LeaveRequestId = request.Id;
        LeaveRequest = request;
        Status = "EXCUSED";
        Comment = $"Nghỉ có phép theo đơn #{request.Id}";
        Version = Guid.NewGuid();
    }

    public void RecordResult(string status, string? comment, LeaveRequest? leave, long employeeId, DateTime utcNow)
    {
        status = AttendanceStatuses.Validate(status);
        if (comment?.Length > 1000) throw new AppError("Ghi chú tối đa 1.000 ký tự.");
        if (status == AttendanceStatuses.Excused)
            ApplyApprovedLeave(leave ?? throw new AppError("Vắng có phép cần đơn nghỉ được duyệt.", "LEAVE_REQUIRED"));
        else Mark(status, comment);
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        UpdatedAt = utcNow; UpdatedByEmployeeId = employeeId; Version = Guid.NewGuid();
    }
}
