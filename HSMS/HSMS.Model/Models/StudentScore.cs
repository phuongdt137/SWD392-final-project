namespace HSMS.Model.Model;

public class StudentScore
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long AssessmentId { get; set; }
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
    public bool IsLocked { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual Assessment Assessment { get; set; } = null!;
    public virtual ICollection<ScoreAuditLog> ScoreAuditLogs { get; set; } = new List<ScoreAuditLog>();
}
