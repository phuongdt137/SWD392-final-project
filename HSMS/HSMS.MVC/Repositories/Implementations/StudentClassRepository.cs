using HSMS.MVC.Data;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Repositories.Implementations
{
    public class StudentClassRepository : GenericRepository<StudentClass>, IStudentClassRepository
    {
        public StudentClassRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<Class?> GetClassWithYearAsync(long classId)
        {
            return await _context.Classes
                .Include(c => c.AcademicYear)
                .FirstOrDefaultAsync(c => c.Id == classId);
        }

        public async Task<List<Class>> GetAllClassesAsync()
        {
            return await _context.Classes
                .AsNoTracking()
                .Include(c => c.AcademicYear)
                .OrderByDescending(c => c.AcademicYear.StartDate)
                .ThenBy(c => c.ClassName)
                .ToListAsync();
        }

        public async Task<List<Class>> GetOtherActiveClassesInYearAsync(long academicYearId, long excludeClassId)
        {
            return await _context.Classes
                .AsNoTracking()
                .Where(c => c.AcademicYearId == academicYearId && c.Id != excludeClassId && c.Status == "ACTIVE")
                .OrderBy(c => c.ClassName)
                .ToListAsync();
        }

        public async Task<List<StudentClass>> GetActiveStudentsOfClassAsync(long classId)
        {
            return await _dbSet
                .AsNoTracking()
                .Include(sc => sc.Customer).ThenInclude(c => c.User)
                .Where(sc => sc.ClassId == classId && sc.Status == "ACTIVE")
                .OrderBy(sc => sc.Customer.StudentCode)
                .ToListAsync();
        }

        public async Task<StudentClass?> GetWithDetailsAsync(long studentClassId)
        {
            return await _dbSet
                .Include(sc => sc.Customer).ThenInclude(c => c.User)
                .Include(sc => sc.Class).ThenInclude(c => c.AcademicYear)
                .FirstOrDefaultAsync(sc => sc.Id == studentClassId);
        }

        public async Task<bool> IsStudentAsync(long customerId)
        {
            return await _context.Customers.AnyAsync(c =>
                c.Id == customerId &&
                c.User.IsActive &&
                c.User.UserRoles.Any(ur => ur.IsActive && ur.Role.RoleCode == "STUDENT"));
        }

        public async Task<bool> HasActiveInYearAsync(long customerId, long academicYearId)
        {
            return await _dbSet.AnyAsync(sc =>
                sc.CustomerId == customerId &&
                sc.Status == "ACTIVE" &&
                sc.Class.AcademicYearId == academicYearId);
        }

        public async Task<List<Customer>> GetAssignableStudentsAsync(long academicYearId)
        {
            return await _context.Customers
                .AsNoTracking()
                .Include(c => c.User)
                .Where(c => c.User.IsActive &&
                            c.User.UserRoles.Any(ur => ur.IsActive && ur.Role.RoleCode == "STUDENT") &&
                            !c.StudentClasses.Any(sc => sc.Status == "ACTIVE" && sc.Class.AcademicYearId == academicYearId))
                .OrderBy(c => c.StudentCode)
                .ToListAsync();
        }

        public async Task AddTransferAsync(StudentTransfer transfer)
        {
            await _context.StudentTransfers.AddAsync(transfer);
        }
    }
}
