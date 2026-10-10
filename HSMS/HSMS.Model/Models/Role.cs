namespace HSMS.Model.Model;

public class Role
{
    public long Id { get; set; }
    public string RoleCode { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CustomerRole> CustomerRoles { get; set; } = new List<CustomerRole>();
    public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
}
