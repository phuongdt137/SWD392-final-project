namespace HSMS.Model.Model;

public class SemesterSubjectResult
{
    public long Id { get; set; }
    public long SemesterReportId { get; set; }
    public long SubjectId { get; set; }
    public decimal? AverageScore { get; set; }
    public string? EvaluationResult { get; set; }

    public virtual SemesterReport SemesterReport { get; set; } = null!;
    public virtual Subject Subject { get; set; } = null!;
}
