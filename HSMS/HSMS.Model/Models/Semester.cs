namespace HSMS.Model.Model;

public class Semester
{
    public long Id { get; set; }
    public string SemesterCode { get; set; } = null!;
    public string SemesterName { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public long AcademicYearId { get; set; }

    public virtual AcademicYear AcademicYear { get; set; } = null!;
    public virtual ICollection<TeachingAssignment> TeachingAssignments { get; set; } = new List<TeachingAssignment>();
    public virtual ICollection<ConductEvaluation> ConductEvaluations { get; set; } = new List<ConductEvaluation>();
    public virtual ICollection<SemesterReport> SemesterReports { get; set; } = new List<SemesterReport>();
}
