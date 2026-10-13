using HSMS.Model.Exceptions;

namespace HSMS.Model.Model;

public class LeaveRequest
{
    public long Id { get; private set; }
    public long StudentCustomerId { get; private set; }
    public long RequestedByCustomerId { get; private set; }
    public string Reason { get; private set; } = null!;
    public string? Attachment { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Status { get; private set; } = "PENDING";
    public DateTime RequestedAt { get; private set; }
    public string? ReviewNote { get; private set; }
    public long? ReviewedByEmployeeId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    public virtual Customer StudentCustomer { get; private set; } = null!;
    public virtual Customer RequestedByCustomer { get; private set; } = null!;
    public virtual Employee? ReviewedByEmployee { get; private set; }
    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; private set; } = new List<AttendanceRecord>();

    protected LeaveRequest() { }

    public LeaveRequest(long studentCustomerId, long requestedByCustomerId, string reason, DateTime startDate, DateTime endDate, string? attachment = null)
    {
        if (studentCustomerId <= 0)
            throw new BusinessException("Id học sinh không hợp lệ.", "INVALID_STUDENT_ID");

        if (requestedByCustomerId <= 0)
            throw new BusinessException("Id người nộp đơn không hợp lệ.", "INVALID_REQUESTER_ID");

        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessException("Lý do xin nghỉ không được để trống.", "REASON_REQUIRED");

        if (endDate < startDate)
            throw new BusinessException("Ngày kết thúc nghỉ phép không được nhỏ hơn ngày bắt đầu.", "INVALID_DATE_RANGE");

        StudentCustomerId = studentCustomerId;
        RequestedByCustomerId = requestedByCustomerId;
        Reason = reason.Trim();
        StartDate = startDate.Date;
        EndDate = endDate.Date;
        Attachment = attachment;
        Status = "PENDING";
        RequestedAt = DateTime.UtcNow;
    }

    public void Submit()
    {
        if (EndDate < StartDate)
            throw new BusinessException("Ngày kết thúc nghỉ phép không được nhỏ hơn ngày bắt đầu.", "INVALID_DATE_RANGE");

        Status = "PENDING";
        RequestedAt = DateTime.UtcNow;
    }

    public void Approve(Employee reviewer, string? note = null)
    {
        ArgumentNullException.ThrowIfNull(reviewer);
        if (Status != "PENDING")
            throw new BusinessException("Chỉ đơn xin nghỉ ở trạng thái PENDING mới có thể phê duyệt.", "INVALID_STATUS");

        Status = "APPROVED";
        ReviewedByEmployeeId = reviewer.Id;
        ReviewedByEmployee = reviewer;
        ReviewNote = note;
        ReviewedAt = DateTime.UtcNow;
    }

    public void Reject(Employee reviewer, string reason)
    {
        ArgumentNullException.ThrowIfNull(reviewer);
        if (string.IsNullOrWhiteSpace(reason))
            throw new BusinessException("Lý do từ chối không được để trống.", "REASON_REQUIRED");

        if (Status != "PENDING")
            throw new BusinessException("Chỉ đơn xin nghỉ ở trạng thái PENDING mới có thể từ chối.", "INVALID_STATUS");

        Status = "REJECTED";
        ReviewedByEmployeeId = reviewer.Id;
        ReviewedByEmployee = reviewer;
        ReviewNote = reason.Trim();
        ReviewedAt = DateTime.UtcNow;
    }
}
