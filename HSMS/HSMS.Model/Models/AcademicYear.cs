namespace HSMS.Model.Model;

public class AcademicYear
{
    public long Id { get; set; }
    public string YearCode { get; set; } = null!;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
