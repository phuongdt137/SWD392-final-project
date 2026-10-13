using HSMS.Model.Exceptions;

namespace HSMS.Model.Model;

public class CustomerRole
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    public long RoleId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime AssignedAt { get; private set; }

    public virtual Customer Customer { get; private set; } = null!;
    public virtual Role Role { get; private set; } = null!;

    protected CustomerRole() { }

    public CustomerRole(long customerId, long roleId, bool isActive = true)
    {
        if (customerId <= 0)
            throw new BusinessException("Id người dùng không hợp lệ.", "INVALID_CUSTOMER_ID");

        if (roleId <= 0)
            throw new BusinessException("Id vai trò không hợp lệ.", "INVALID_ROLE_ID");

        CustomerId = customerId;
        RoleId = roleId;
        IsActive = isActive;
        AssignedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
