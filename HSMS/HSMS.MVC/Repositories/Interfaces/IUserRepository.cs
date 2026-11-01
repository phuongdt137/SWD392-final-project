using HSMS.MVC.Models;

namespace HSMS.MVC.Repositories.Interfaces;

/// <summary>
/// Giao diện Repository chuyên biệt cho thực thể User.
/// </summary>
public interface IUserRepository : IGenericRepository<User>
{
    /// <summary>
    /// Tìm người dùng theo tên đăng nhập hoặc email, kèm theo các Roles và Profiles (Employee/Customer).
    /// </summary>
    Task<User?> GetByUsernameOrEmailWithDetailsAsync(string usernameOrEmail);

    /// <summary>
    /// Tìm người dùng theo Id kèm theo các Roles và Profiles.
    /// </summary>
    Task<User?> GetByIdWithDetailsAsync(long id);

    /// <summary>
    /// Kiểm tra xem Username đã tồn tại trong hệ thống hay chưa.
    /// </summary>
    Task<bool> ExistsByUsernameAsync(string username);

    /// <summary>
    /// Kiểm tra xem Email đã tồn tại trong hệ thống hay chưa.
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email);

    /// <summary>
    /// Tìm người dùng theo Email.
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Tìm người dùng theo mã Token đặt lại mật khẩu.
    /// </summary>
    Task<User?> GetByPasswordResetTokenAsync(string token);
}
