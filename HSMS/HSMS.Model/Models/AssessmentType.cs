namespace HSMS.Model.Model;

public class AssessmentType
{
    public long Id { get; set; }
    public string TypeCode { get; set; } = null!;
    public string TypeName { get; set; } = null!;
    public decimal Weight { get; set; }
    public int MinEntries { get; set; } = 1;
    public int? MaxEntries { get; set; }

    public virtual ICollection<Assessment> Assessments { get; set; } = new List<Assessment>();
}
