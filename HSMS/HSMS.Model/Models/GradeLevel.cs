namespace HSMS.Model.Model;

public class GradeLevel
{
    public long Id { get; set; }
    public string LevelCode { get; set; } = null!;
    public string LevelName { get; set; } = null!;

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
