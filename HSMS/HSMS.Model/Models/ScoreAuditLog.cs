namespace HSMS.Model.Model;

public class ScoreAuditLog
{
    public long Id { get; set; }
    public long StudentScoreId { get; set; }
    public decimal? OldScore { get; set; }
    public decimal? NewScore { get; set; }
    public string? ChangeReason { get; set; }
    public long ChangedByEmployeeId { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ChangedAt { get; set; }

    public virtual StudentScore StudentScore { get; set; } = null!;
    public virtual Employee ChangedByEmployee { get; set; } = null!;
}
