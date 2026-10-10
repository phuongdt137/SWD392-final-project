namespace HSMS.Model.Model;

public class StudentClass
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long ClassId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public string Status { get; set; } = "ACTIVE";

    public virtual Customer Customer { get; set; } = null!;
    public virtual Class Class { get; set; } = null!;
}
