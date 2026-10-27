using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

public class Role
{
    public long Id { get; private set; }
    public string RoleCode { get; private set; } = null!;
    public string RoleName { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }

    public virtual ICollection<CustomerRole> CustomerRoles { get; private set; } = new List<CustomerRole>();
    public virtual ICollection<EmployeeRole> EmployeeRoles { get; private set; } = new List<EmployeeRole>();

    protected Role() { }

    public Role(string roleCode, string roleName, string? description = null, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(roleCode))
            throw new AppError("Mã vai trò không được để trống.", "ROLE_CODE_REQUIRED");

        if (string.IsNullOrWhiteSpace(roleName))
            throw new AppError("Tên vai trò không được để trống.", "ROLE_NAME_REQUIRED");

        RoleCode = roleCode.Trim().ToUpperInvariant();
        RoleName = roleName.Trim();
        Description = description;
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    public Role(long id, string roleCode, string roleName, string? description, bool isActive, DateTime createdAt)
    {
        Id = id;
        RoleCode = roleCode;
        RoleName = roleName;
        Description = description;
        IsActive = isActive;
        CreatedAt = createdAt;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void UpdateDescription(string? description) => Description = description;
}
