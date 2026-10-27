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
                .ThenInclude(c => c.Parent)
            .Include(s => s.ScoreAuditLogs)
                .ThenInclude(l => l.ChangedByEmployee)
                    
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<IReadOnlyList<StudentScore>> GetByStudentAndSemesterAsync(long customerId, long? semesterId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(s => s.Assessment)
                .ThenInclude(a => a.AssessmentType)
            .Include(s => s.Assessment)
                .ThenInclude(a => a.TeachingAssignment)
                    .ThenInclude(ta => ta.Subject)
            .Where(s => s.CustomerId == customerId
                && (semesterId == null || s.Assessment.TeachingAssignment.SemesterId == semesterId))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Semester>> GetSemestersByStudentAsync(long customerId)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(s => s.CustomerId == customerId)
            .Select(s => s.Assessment.TeachingAssignment.Semester)
            .Distinct()
            .OrderByDescending(sem => sem.StartDate)
            .ToListAsync();
    }
}
