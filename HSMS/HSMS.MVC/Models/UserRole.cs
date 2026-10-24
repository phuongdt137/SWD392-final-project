using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

/// <summary>
/// Thực thể gán quyền cho người dùng (User - Role).
/// </summary>
public class UserRole
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public long RoleId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime AssignedAt { get; private set; }

    public virtual User User { get; private set; } = null!;
    public virtual Role Role { get; private set; } = null!;

    protected UserRole() { }

    public UserRole(long userId, long roleId, bool isActive = true)
    {
        if (userId <= 0)
            throw new AppError("Id người dùng không hợp lệ.", "INVALID_USER_ID");

        if (roleId <= 0)
            throw new AppError("Id vai trò không hợp lệ.", "INVALID_ROLE_ID");

        UserId = userId;
        RoleId = roleId;
        IsActive = isActive;
        AssignedAt = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}
