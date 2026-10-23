using HSMS.MVC.Exceptions;

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

    public virtual Customer Customer { get; private set; } = null!;
    public virtual Employee RecordedByEmployee { get; private set; } = null!;
    public virtual LeaveRequest? LeaveRequest { get; private set; }
    public virtual TimetableEntry TimetableEntry { get; private set; } = null!;

    protected AttendanceRecord() { }

    public AttendanceRecord(long customerId, long timetableEntryId, long recordedByEmployeeId, DateTime attendanceDate, string status, string? comment = null, long? leaveRequestId = null)
    {
        if (customerId <= 0)
            throw new BusinessException("Id học sinh không hợp lệ.", "INVALID_CUSTOMER_ID");

        if (timetableEntryId <= 0)
            throw new BusinessException("Id thời khóa biểu không hợp lệ.", "INVALID_TIMETABLE_ENTRY_ID");

        if (recordedByEmployeeId <= 0)
            throw new BusinessException("Id cán bộ điểm danh không hợp lệ.", "INVALID_EMPLOYEE_ID");

        if (string.IsNullOrWhiteSpace(status))
            throw new BusinessException("Trạng thái điểm danh không được để trống.", "INVALID_STATUS");

        CustomerId = customerId;
        TimetableEntryId = timetableEntryId;
        RecordedByEmployeeId = recordedByEmployeeId;
        AttendanceDate = attendanceDate.Date;
        Status = status.Trim().ToUpperInvariant();
        Comment = comment;
        LeaveRequestId = leaveRequestId;
        CreatedAt = DateTime.UtcNow;
    }

    public void Mark(string status, string? comment = null)
    {
        if (string.IsNullOrWhiteSpace(status))
            throw new BusinessException("Trạng thái điểm danh không được để trống.", "INVALID_STATUS");

        Status = status.Trim().ToUpperInvariant();
        Comment = comment;
    }

    public void ApplyApprovedLeave(LeaveRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Status != "APPROVED")
            throw new BusinessException("Đơn xin nghỉ phép chưa được duyệt.", "LEAVE_NOT_APPROVED");

        if (AttendanceDate < request.StartDate || AttendanceDate > request.EndDate)
            throw new BusinessException("Ngày điểm danh không nằm trong khoảng thời gian nghỉ phép của đơn.", "DATE_OUT_OF_LEAVE_RANGE");

        LeaveRequestId = request.Id;
        LeaveRequest = request;
        Status = "EXCUSED";
        Comment = $"Nghỉ có phép theo đơn #{request.Id}";
    }
}
