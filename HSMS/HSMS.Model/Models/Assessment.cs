namespace HSMS.Model.Model;

public class Assessment
{
    public long Id { get; set; }
    public long TeachingAssignmentId { get; set; }
    public long AssessmentTypeId { get; set; }
    public string AssessmentName { get; set; } = null!;
    public DateTime? ConductedAt { get; set; }

    public virtual TeachingAssignment TeachingAssignment { get; set; } = null!;
    public virtual AssessmentType AssessmentType { get; set; } = null!;
    public virtual ICollection<StudentScore> StudentScores { get; set; } = new List<StudentScore>();
}
