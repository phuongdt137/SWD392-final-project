using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

/// <summary>
/// Thực thể Người dùng (User) - Quản lý danh tính và thông tin xác thực tập trung toàn hệ thống.
/// </summary>
public class User
{
    public long Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public string? AvatarUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }

    // Navigations
    public virtual ICollection<UserRole> UserRoles { get; private set; } = new List<UserRole>();
    public virtual Employee? EmployeeProfile { get; private set; }
    public virtual Customer? CustomerProfile { get; private set; }
    public virtual ICollection<Notification> SentNotifications { get; private set; } = new List<Notification>();
    public virtual ICollection<Notification> ReceivedNotifications { get; private set; } = new List<Notification>();
    public virtual ICollection<MessageLog> ReceivedMessageLogs { get; private set; } = new List<MessageLog>();
    public virtual ICollection<MessageLog> CreatedMessageLogs { get; private set; } = new List<MessageLog>();

    protected User() { }

    public User(
        string username,
        string email,
        string passwordHash,
        string fullName,
        string? phoneNumber = null,
        string? avatarUrl = null,
        bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new AppError("Tên đăng nhập không được để trống.", "USERNAME_REQUIRED");

        if (string.IsNullOrWhiteSpace(email))
            throw new AppError("Email không được để trống.", "EMAIL_REQUIRED");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new AppError("Mật khẩu băm không được để trống.", "PASSWORD_REQUIRED");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new AppError("Họ và tên không được để trống.", "FULLNAME_REQUIRED");

        Username = username.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber?.Trim();
        AvatarUrl = avatarUrl?.Trim();
        IsActive = isActive;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string fullName, string? phoneNumber, string? avatarUrl)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new AppError("Họ và tên không được để trống.", "FULLNAME_REQUIRED");

        FullName = fullName.Trim();
        PhoneNumber = phoneNumber?.Trim();
        AvatarUrl = avatarUrl?.Trim();
    }

    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new AppError("Mật khẩu mới không được để trống.", "PASSWORD_REQUIRED");

        PasswordHash = newPasswordHash;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public IReadOnlyList<string> GetActiveRoleCodes()
    {
        return UserRoles
            .Where(ur => ur.IsActive && ur.Role != null && ur.Role.IsActive)
            .Select(ur => ur.Role!.RoleCode)
            .Distinct()
            .ToList()
            .AsReadOnly();
    }

    public void SetPasswordResetToken(string token, DateTime expiry)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new AppError("Mã xác nhận đặt lại mật khẩu không hợp lệ.", "RESET_TOKEN_REQUIRED");

        PasswordResetToken = token;
        PasswordResetTokenExpiry = expiry;
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
    }

    public bool IsPasswordResetTokenValid(string token)
    {
        return !string.IsNullOrWhiteSpace(PasswordResetToken)
            && PasswordResetToken == token
            && PasswordResetTokenExpiry.HasValue
            && PasswordResetTokenExpiry.Value > DateTime.UtcNow;
    }
}
