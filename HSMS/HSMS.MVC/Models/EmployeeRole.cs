using HSMS.MVC.Exceptions;

namespace HSMS.MVC.Models;

public class EmployeeRole
{
    public long Id { get; private set; }
    public long EmployeeId { get; private set; }
    public long RoleId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime AssignedAt { get; private set; }

    public virtual Employee Employee { get; private set; } = null!;
    public virtual Role Role { get; private set; } = null!;

    protected EmployeeRole() { }

    public EmployeeRole(long employeeId, long roleId, bool isActive = true)
    {
        if (employeeId <= 0)
            throw new BusinessException("Id cán bộ không hợp lệ.", "INVALID_EMPLOYEE_ID");

        if (roleId <= 0)
            throw new BusinessException("Id vai trò không hợp lệ.", "INVALID_ROLE_ID");

        EmployeeId = employeeId;
        RoleId = roleId;
        IsActive = isActive;
        AssignedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
