using HSMS.MVC.Data;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Repositories.Implementations;

/// <summary>
/// Thực thi Repository cho thực thể TeachingAssignment.
/// </summary>
public class TeachingAssignmentRepository : GenericRepository<TeachingAssignment>, ITeachingAssignmentRepository
{
    public TeachingAssignmentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<TeachingAssignment?> GetWithDetailsAsync(long id)
    {
        return await _dbSet
            .Include(ta => ta.Class)
            .Include(ta => ta.Subject)
            .Include(ta => ta.Semester)
            .Include(ta => ta.Assessments)
                .ThenInclude(a => a.AssessmentType)
            .FirstOrDefaultAsync(ta => ta.Id == id);
    }

    public async Task<IReadOnlyList<TeachingAssignment>> GetActiveByEmployeeAsync(long employeeId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(ta => ta.Class)
            .Include(ta => ta.Subject)
            .Include(ta => ta.Semester)
            .Include(ta => ta.Assessments)
                .ThenInclude(a => a.AssessmentType)
            .Where(ta => ta.EmployeeId == employeeId && ta.Status == "ACTIVE")
            .OrderByDescending(ta => ta.Semester.StartDate)
            .ToListAsync();
    }
}
