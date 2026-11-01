using HSMS.MVC.Data;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Repositories.Implementations;

/// <summary>
/// Thực thi Repository chuyên biệt cho thực thể User.
/// </summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameOrEmailWithDetailsAsync(string usernameOrEmail)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail))
            return null;

        var identifier = usernameOrEmail.Trim().ToLowerInvariant();

        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.EmployeeProfile)
            .Include(u => u.CustomerProfile)
            .FirstOrDefaultAsync(u => u.Username.ToLower() == identifier || u.Email.ToLower() == identifier);
    }

    public async Task<User?> GetByIdWithDetailsAsync(long id)
    {
        return await _dbSet
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .Include(u => u.EmployeeProfile)
            .Include(u => u.CustomerProfile)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return false;

        var normalized = username.Trim().ToLowerInvariant();
        return await _dbSet.AnyAsync(u => u.Username.ToLower() == normalized);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalized = email.Trim().ToLower();
        return await _dbSet.AnyAsync(u => u.Email.ToLower() == normalized);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalized = email.Trim().ToLower();
        return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == normalized);
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        return await _dbSet.FirstOrDefaultAsync(u => u.PasswordResetToken == token);
    }
}
