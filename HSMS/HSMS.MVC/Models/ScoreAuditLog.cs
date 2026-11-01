using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

public class ScoreAuditLog
{
    public long Id { get; private set; }
    public long StudentScoreId { get; private set; }
    public decimal? OldScore { get; private set; }
    public decimal? NewScore { get; private set; }
    public string? ChangeReason { get; private set; }
    public long ChangedByEmployeeId { get; private set; }
    public string? IpAddress { get; private set; }
    public DateTime ChangedAt { get; private set; }

    public virtual StudentScore StudentScore { get; private set; } = null!;
    public virtual Employee ChangedByEmployee { get; private set; } = null!;

    protected ScoreAuditLog() { }

    public ScoreAuditLog(long studentScoreId, decimal? oldScore, decimal? newScore, string? changeReason, long changedByEmployeeId, DateTime changedAt, string? ipAddress = null, StudentScore? studentScore = null, Employee? changedByEmployee = null)
    {
        if (studentScoreId <= 0)
            throw new AppError("Id điểm học sinh không hợp lệ.", "INVALID_STUDENT_SCORE_ID");

        if (changedByEmployeeId <= 0)
            throw new AppError("Id cán bộ chỉnh sửa không hợp lệ.", "INVALID_EMPLOYEE_ID");

        if (string.IsNullOrWhiteSpace(changeReason))
            throw new AppError("Lý do sửa điểm không được để trống.", "REASON_REQUIRED");
        StudentScoreId = studentScoreId;
        OldScore = oldScore;
        NewScore = newScore;
        ChangeReason = changeReason;
        ChangedByEmployeeId = changedByEmployeeId;
        ChangedAt = changedAt;
        IpAddress = ipAddress;
        if (studentScore != null) StudentScore = studentScore;
        if (changedByEmployee != null) ChangedByEmployee = changedByEmployee;
    }

    public decimal GetScoreDifference() => (NewScore ?? 0m) - (OldScore ?? 0m);
}
