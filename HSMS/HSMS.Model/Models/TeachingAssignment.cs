namespace HSMS.Model.Model;

public class TeachingAssignment
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public long SubjectId { get; set; }
    public long ClassId { get; set; }
    public long SemesterId { get; set; }
    public string Status { get; set; } = "ACTIVE";

    public virtual Employee Employee { get; set; } = null!;
    public virtual Subject Subject { get; set; } = null!;
    public virtual Class Class { get; set; } = null!;
    public virtual Semester Semester { get; set; } = null!;
    public virtual ICollection<TimetableEntry> TimetableEntries { get; set; } = new List<TimetableEntry>();
    public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
    public virtual ICollection<TeacherFeedback> TeacherFeedbacks { get; set; } = new List<TeacherFeedback>();
}
