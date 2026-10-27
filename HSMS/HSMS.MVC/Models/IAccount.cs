namespace HSMS.MVC.Models;

// Hợp đồng dùng chung cho đăng nhập. Interface này KHÔNG được ánh xạ thành bảng.
public interface IAccount
{
    long Id { get; }
    string Username { get; }
    string Email { get; }
    string PasswordHash { get; }
    string FullName { get; }
    string? AvatarUrl { get; }
    bool IsActive { get; }
    IReadOnlyList<string> GetActiveRoleCodes();
    void RecordLogin();
    void ChangePassword(string passwordHash);
    void SetPasswordResetToken(string token, DateTime expiry);
    void ClearPasswordResetToken();
    bool IsPasswordResetTokenValid(string token);
}
