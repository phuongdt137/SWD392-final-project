using HSMS.MVC.Data;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Repositories.Implementations;

/// <summary>
/// Thực thi Repository cho thực thể Customer.
/// </summary>
public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Customer>> GetActiveStudentsByClassAsync(long classId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(c => c.StudentClasses.Any(sc => sc.ClassId == classId && sc.Status == "ACTIVE"))
            .OrderBy(c => c.StudentCode)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdWithChildrenAsync(long userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == userId);
    }

    public async Task<Customer?> GetByIdWithParentAsync(long id)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
