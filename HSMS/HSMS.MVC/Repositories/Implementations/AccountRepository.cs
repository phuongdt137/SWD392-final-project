using HSMS.MVC.Data;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Repositories.Implementations;

public class AccountRepository(AppDbContext db) : IAccountRepository
{
    private IQueryable<Employee> Employees => db.Employees.Include(e => e.EmployeeRoles).ThenInclude(r => r.Role);
    private IQueryable<Customer> Customers => db.Customers.Include(c => c.CustomerRoles).ThenInclude(r => r.Role);

    public async Task<IAccount?> GetAsync(long id, string accountType)
    {
        // ID chỉ có ý nghĩa khi đi kèm loại tài khoản. Không dò bảng còn lại.
        if (accountType == "EMPLOYEE") return await Employees.SingleOrDefaultAsync(e => e.Id == id);
        if (accountType == "CUSTOMER") return await Customers.SingleOrDefaultAsync(c => c.Id == id);
        return null;
    }
    public async Task<IAccount?> FindByLoginAsync(string identifier)
    {
        var value = identifier.Trim().ToLowerInvariant();
        var employees = await Employees.Where(e => e.Username.ToLower() == value || e.Email.ToLower() == value).ToListAsync();
        var customers = await Customers.Where(c => c.Username.ToLower() == value || c.Email.ToLower() == value).ToListAsync();
        // Nếu dữ liệu cũ có username/email trùng giữa 2 bảng, không đoán tài khoản.
        var matches = employees.Cast<IAccount>().Concat(customers).ToList();
        return matches.Count == 1 ? matches[0] : null;
    }
    public async Task<IAccount?> FindByEmailAsync(string email)
    {
        var value = email.Trim().ToLowerInvariant();
        var employees = await Employees.Where(e => e.Email.ToLower() == value).ToListAsync();
        var customers = await Customers.Where(c => c.Email.ToLower() == value).ToListAsync();
        var matches = employees.Cast<IAccount>().Concat(customers).ToList();
        return matches.Count == 1 ? matches[0] : null;
    }
    public async Task SaveChangesAsync() => await db.SaveChangesAsync();
}
