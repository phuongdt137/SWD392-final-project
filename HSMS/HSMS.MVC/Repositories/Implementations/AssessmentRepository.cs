using HSMS.MVC.Data;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Repositories.Implementations;

/// <summary>
/// Thực thi Repository cho thực thể Assessment.
/// </summary>
public class AssessmentRepository : GenericRepository<Assessment>, IAssessmentRepository
{
    public AssessmentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Assessment?> GetWithDetailsAsync(long id)
    {
        return await _dbSet
            .Include(a => a.TeachingAssignment)
                .ThenInclude(ta => ta.Class)
            .Include(a => a.TeachingAssignment)
                .ThenInclude(ta => ta.Subject)
            .Include(a => a.AssessmentType)
            .Include(a => a.StudentScores)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}
