namespace HSMS.Model.Model;

public class Subject
{
    public long Id { get; set; }
    public string SubjectCode { get; set; } = null!;
    public string SubjectName { get; set; } = null!;
    public string? EvaluationType { get; set; }

    public virtual ICollection<TeachingAssignment> TeachingAssignments { get; set; } = new List<TeachingAssignment>();
    public virtual ICollection<SemesterSubjectResult> SemesterSubjectResults { get; set; } = new List<SemesterSubjectResult>();
}
