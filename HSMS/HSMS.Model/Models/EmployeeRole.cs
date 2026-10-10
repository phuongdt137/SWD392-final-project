namespace HSMS.Model.Model;

public class EmployeeRole
{
    public long Id { get; set; }
    public long EmployeeId { get; set; }
    public long RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime AssignedAt { get; set; }

    public virtual Employee Employee { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
