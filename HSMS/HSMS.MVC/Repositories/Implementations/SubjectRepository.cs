using HSMS.MVC.Data;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Repositories.Implementations
{
    public class SubjectRepository : GenericRepository<Subject>, ISubjectRepository
    {
        public SubjectRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Subject>> GetAllWithAssignmentsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(s => s.TeachingAssignments)
                .OrderBy(s => s.SubjectCode)
                .ToListAsync();
        }

        public async Task<bool> CodeExistsAsync(string subjectCode, long? excludeId = null)
        {
            return await _dbSet.AnyAsync(s => s.SubjectCode == subjectCode && (excludeId == null || s.Id != excludeId));
        }

        public async Task<bool> IsInUseAsync(long id)
        {
            return await _dbSet.AnyAsync(s => s.Id == id &&
                (s.TeachingAssignments.Any() || s.SemesterSubjectResults.Any()));
        }

        public void UpdateInfo(Subject subject, string subjectCode, string subjectName, string? evaluationType)
        {
            var entry = _context.Entry(subject);
            entry.Property(s => s.SubjectCode).CurrentValue = subjectCode;
            entry.Property(s => s.SubjectName).CurrentValue = subjectName;
            entry.Property(s => s.EvaluationType).CurrentValue = evaluationType;
        }

    }
}
