using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

public class StudentScore
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    public long AssessmentId { get; private set; }
    public decimal? Score { get; private set; }
    public string? Comment { get; private set; }
    public bool IsLocked { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public virtual Customer Customer { get; private set; } = null!;
    public virtual Assessment Assessment { get; private set; } = null!;
    public virtual ICollection<ScoreAuditLog> ScoreAuditLogs { get; private set; } = new List<ScoreAuditLog>();

    protected StudentScore() { }

    public StudentScore(long customerId, long assessmentId, decimal? score = null, string? comment = null)
    {
        if (customerId <= 0)
            throw new AppError("Id học sinh không hợp lệ.", "INVALID_CUSTOMER_ID");

        if (assessmentId <= 0)
            throw new AppError("Id bài kiểm tra không hợp lệ.", "INVALID_ASSESSMENT_ID");

        if (score.HasValue)
            ValidateScore(score.Value);

        CustomerId = customerId;
        AssessmentId = assessmentId;
        Score = score;
        Comment = comment;
        IsLocked = false;
        CreatedAt = DateTime.UtcNow;
    }

    public void EnterScore(decimal score, string? comment = null)
    {
        if (IsLocked)
            throw new AppError("Bảng điểm đã bị khóa, không thể nhập điểm.", "SCORE_LOCKED");

        ValidateScore(score);

        Score = score;
        Comment = comment;
        UpdatedAt = DateTime.UtcNow;
    }

    public ScoreAuditLog UpdateScore(decimal newScore, string reason, Employee employee, string? ipAddress = null)
    {
        ArgumentNullException.ThrowIfNull(employee);
        if (IsLocked)
            throw new AppError("Bảng điểm đã bị khóa, không thể chỉnh sửa điểm.", "SCORE_LOCKED");

        ValidateScore(newScore);

        if (string.IsNullOrWhiteSpace(reason))
            throw new AppError("Lý do sửa điểm không được để trống.", "REASON_REQUIRED");

        var auditLog = new ScoreAuditLog(
            Id,
            Score,
            newScore,
            reason.Trim(),
            employee.Id,
            DateTime.UtcNow,
            ipAddress,
            this,
            employee
        );

        ScoreAuditLogs.Add(auditLog);
        Score = newScore;
        UpdatedAt = DateTime.UtcNow;
        return auditLog;
    }

    public void Lock()
    {
        IsLocked = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public ICollection<ScoreAuditLog> GetAuditLogs() => ScoreAuditLogs;

    private static void ValidateScore(decimal score)
    {
        if (score < 0 || score > 10)
            throw new AppError("Điểm số phải nằm trong khoảng từ 0 đến 10.", "INVALID_SCORE");
    }
}
