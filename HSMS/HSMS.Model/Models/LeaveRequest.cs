namespace HSMS.Model.Model;

public class LeaveRequest
{
    public long Id { get; set; }
    public long StudentCustomerId { get; set; }
    public long RequestedByCustomerId { get; set; }
    public string Reason { get; set; } = null!;
    public string? Attachment { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "PENDING";
    public DateTime RequestedAt { get; set; }
    public string? ReviewNote { get; set; }
    public long? ReviewedByEmployeeId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public virtual Customer StudentCustomer { get; set; } = null!;
    public virtual Customer RequestedByCustomer { get; set; } = null!;
    public virtual Employee? ReviewedByEmployee { get; set; }
    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}
