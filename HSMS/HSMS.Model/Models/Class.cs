namespace HSMS.Model.Model;

public class Class
{
    public long Id { get; set; }
    public string ClassName { get; set; } = null!;
    public long? HomeroomEmployeeId { get; set; }
    public long AcademicYearId { get; set; }
    public long GradeLevelId { get; set; }
    public string Status { get; set; } = "ACTIVE";

    public virtual Employee? HomeroomEmployee { get; set; }
    public virtual AcademicYear AcademicYear { get; set; } = null!;
    public virtual GradeLevel GradeLevel { get; set; } = null!;
    public virtual ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
    public virtual ICollection<TeachingAssignment> TeachingAssignments { get; set; } = new List<TeachingAssignment>();
}
