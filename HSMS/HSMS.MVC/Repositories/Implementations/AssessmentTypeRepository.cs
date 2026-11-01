using HSMS.MVC.Data;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Repositories.Implementations;

/// <summary>
/// Thực thi Repository cho thực thể AssessmentType.
/// </summary>
public class AssessmentTypeRepository : GenericRepository<AssessmentType>, IAssessmentTypeRepository
{
    public AssessmentTypeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AssessmentType>> GetAllOrderedAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .OrderBy(t => t.TypeCode)
            .ToListAsync();
    }
}
