using HSMS.MVC.Data;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Repositories.Implementations;

/// <summary>
/// Thực thi Repository cho thực thể StudentScore.
/// </summary>
public class StudentScoreRepository : GenericRepository<StudentScore>, IStudentScoreRepository
{
    public StudentScoreRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<StudentScore?> GetWithDetailsAsync(long id)
    {
        return await _dbSet
            .Include(s => s.Assessment)
                .ThenInclude(a => a.TeachingAssignment)
                    .ThenInclude(ta => ta.Subject)
            .Include(s => s.Customer)
                .ThenInclude(c => c.User)
            .Include(s => s.Customer)
                .ThenInclude(c => c.Parent)
            .Include(s => s.ScoreAuditLogs)
                .ThenInclude(l => l.ChangedByEmployee)
                    .ThenInclude(e => e.User)
            .FirstOrDefaultAsync(s => s.Id == id);
    }
}
