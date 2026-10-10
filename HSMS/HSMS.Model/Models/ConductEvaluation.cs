namespace HSMS.Model.Model;

public class ConductEvaluation
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long SemesterId { get; set; }
    public long HomeroomEmployeeId { get; set; }
    public string ConductGrade { get; set; } = null!;
    public string? TeacherComment { get; set; }
    public DateTime EvaluatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual Semester Semester { get; set; } = null!;
    public virtual Employee HomeroomEmployee { get; set; } = null!;
}
