using HSMS.MVC.Data;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Repositories.Implementations
{
    public class AcademicYearRepository : GenericRepository<AcademicYear>, IAcademicYearRepository
    {
        public AcademicYearRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<AcademicYear>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Include(y => y.Semesters)
                .Include(y => y.Classes)
                .OrderByDescending(y => y.StartDate)
                .ToListAsync();
        }

        public async Task<AcademicYear?> GetWithSemestersAsync(long id)
        {
            return await _dbSet
                .Include(y => y.Semesters)
                .FirstOrDefaultAsync(y => y.Id == id);
        }

        public async Task<bool> CodeExistsAsync(string yearCode, long? excludeId = null)
        {
            return await _dbSet.AnyAsync(y => y.YearCode == yearCode && (excludeId == null || y.Id != excludeId));
        }

        public async Task<bool> OverlapsAsync(DateTime start, DateTime end, long? excludeId = null)
        {
            // Hai khoảng ngày chồng nhau khi: bắt đầu A <= kết thúc B và kết thúc A >= bắt đầu B
            return await _dbSet.AnyAsync(y =>
                (excludeId == null || y.Id != excludeId) &&
                y.StartDate <= end && y.EndDate >= start);
        }

        public async Task<bool> SemesterCodeExistsAsync(string semesterCode)
        {
            return await _context.Semesters.AnyAsync(s => s.SemesterCode == semesterCode);
        }

        public async Task AddSemesterAsync(Semester semester)
        {
            await _context.Semesters.AddAsync(semester);
        }

        public void UpdateInfo(AcademicYear year, string yearCode, DateTime start, DateTime end)
        {
            var entry = _context.Entry(year);
            entry.Property(y => y.YearCode).CurrentValue = yearCode;
            entry.Property(y => y.StartDate).CurrentValue = start;
            entry.Property(y => y.EndDate).CurrentValue = end;
        }
    }
}
