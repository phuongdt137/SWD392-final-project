namespace HSMS.Model.Model;

public class SemesterReport
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long SemesterId { get; set; }
    public string Status { get; set; } = "DRAFT";
    public long? FinalizedByEmployeeId { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public decimal? GPA { get; set; }
    public string? AcademicRank { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual Semester Semester { get; set; } = null!;
    public virtual Employee? FinalizedByEmployee { get; set; }
    public virtual ICollection<SemesterSubjectResult> SemesterSubjectResults { get; set; } = new List<SemesterSubjectResult>();
}
