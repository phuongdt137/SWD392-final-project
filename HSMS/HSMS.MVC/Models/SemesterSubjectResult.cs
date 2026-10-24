using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

public class SemesterSubjectResult
{
    public long Id { get; private set; }
    public long SemesterReportId { get; private set; }
    public long SubjectId { get; private set; }
    public decimal? AverageScore { get; private set; }
    public string? EvaluationResult { get; private set; }

    public virtual SemesterReport SemesterReport { get; private set; } = null!;
    public virtual Subject Subject { get; private set; } = null!;

    protected SemesterSubjectResult() { }

    public SemesterSubjectResult(long semesterReportId, long subjectId, decimal? averageScore = null, string? evaluationResult = null)
    {
        if (semesterReportId <= 0)
            throw new AppError("Id báo cáo học kỳ không hợp lệ.", "INVALID_REPORT_ID");

        if (subjectId <= 0)
            throw new AppError("Id môn học không hợp lệ.", "INVALID_SUBJECT_ID");

        if (averageScore.HasValue && (averageScore.Value < 0 || averageScore.Value > 10))
            throw new AppError("Điểm trung bình môn phải nằm trong khoảng từ 0 đến 10.", "INVALID_SCORE");

        SemesterReportId = semesterReportId;
        SubjectId = subjectId;
        AverageScore = averageScore;
        EvaluationResult = evaluationResult?.Trim();
    }

    public decimal CalculateSubjectAverage(IEnumerable<decimal> scores)
    {
        ArgumentNullException.ThrowIfNull(scores);
        var list = scores.ToList();
        if (list.Count == 0)
        {
            AverageScore = 0m;
            return 0m;
        }

        AverageScore = Math.Round(list.Average(), 2);
        return AverageScore.Value;
    }

    public bool UpdateSubjectResult(decimal average, string result)
    {
        if (average < 0 || average > 10)
            throw new AppError("Điểm trung bình môn phải nằm trong khoảng từ 0 đến 10.", "INVALID_SCORE");

        if (string.IsNullOrWhiteSpace(result))
            throw new AppError("Kết quả đánh giá môn không được để trống.", "RESULT_REQUIRED");

        AverageScore = average;
        EvaluationResult = result.Trim();
        return true;
    }
}
