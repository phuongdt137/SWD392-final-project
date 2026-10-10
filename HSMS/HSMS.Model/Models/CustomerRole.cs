namespace HSMS.Model.Model;

public class CustomerRole
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long RoleId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime AssignedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
