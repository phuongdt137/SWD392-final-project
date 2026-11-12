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
            .Include(c => c.User)
            .Where(c => c.StudentClasses.Any(sc => sc.ClassId == classId && sc.Status == "ACTIVE"))
            .OrderBy(c => c.StudentCode)
            .ToListAsync();
    }

    public async Task<Customer?> GetByUserIdWithChildrenAsync(long userId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(c => c.User)
            .Include(c => c.Children)
                .ThenInclude(ch => ch.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Customer?> GetWithScoresAsync(long id)
    {
        return await _dbSet
            .AsNoTrackingWithIdentityResolution()
            .AsSplitQuery()
            .Include(c => c.User)
            .Include(c => c.StudentScores)
                .ThenInclude(s => s.Assessment)
                    .ThenInclude(a => a.AssessmentType)
            .Include(c => c.StudentScores)
                .ThenInclude(s => s.Assessment)
                    .ThenInclude(a => a.TeachingAssignment)
                        .ThenInclude(ta => ta.Subject)
            .Include(c => c.StudentScores)
                .ThenInclude(s => s.Assessment)
                    .ThenInclude(a => a.TeachingAssignment)
                        .ThenInclude(ta => ta.Semester)
            .Include(c => c.StudentScores)
                .ThenInclude(s => s.ScoreAuditLogs)
                    .ThenInclude(l => l.ChangedByEmployee)
                        .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
}
