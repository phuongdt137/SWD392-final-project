using HSMS.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Data;

/// <summary>
/// Dữ liệu mẫu cho phân hệ Đánh giá & Điểm số: loại bài kiểm tra, năm học, học kỳ, lớp, môn và phân công.
/// Chạy sau DbSeeder. Mọi bản ghi được tìm theo mã trước khi thêm nên chạy lại vẫn an toàn.
/// </summary>
public static class ScoreSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await EnsureAssessmentTypeAsync(context, "TX", "Thường xuyên", 20m, 2, 5);
        await EnsureAssessmentTypeAsync(context, "GK", "Giữa kỳ", 30m, 1, 1);
        await EnsureAssessmentTypeAsync(context, "CK", "Cuối kỳ", 50m, 1, 1);
        await context.SaveChangesAsync();

        var academicYear = await context.AcademicYears.FirstOrDefaultAsync(y => y.YearCode == "2026-2027");
        if (academicYear == null)
        {
            academicYear = new AcademicYear("2026-2027", new DateTime(2026, 9, 1), new DateTime(2027, 6, 30));
            context.AcademicYears.Add(academicYear);
            await context.SaveChangesAsync();
        }

        var semester = await context.Semesters.FirstOrDefaultAsync(s => s.SemesterCode == "HK1-2026");
        if (semester == null)
        {
            semester = new Semester("HK1-2026", "Học kỳ 1 (2026-2027)", new DateTime(2026, 9, 1), new DateTime(2027, 1, 15), academicYear.Id);
            context.Semesters.Add(semester);
            await context.SaveChangesAsync();
        }

        var gradeLevel = await context.GradeLevels.FirstOrDefaultAsync(g => g.LevelCode == "K10");
        if (gradeLevel == null)
        {
            gradeLevel = new GradeLevel("K10", "Khối 10");
            context.GradeLevels.Add(gradeLevel);
            await context.SaveChangesAsync();
        }

        var subject = await context.Subjects.FirstOrDefaultAsync(s => s.SubjectCode == "TOAN");
        if (subject == null)
        {
            subject = new Subject("TOAN", "Toán học");
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();
        }

        var teacher = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == "EMP_TEA001");
        var student = await context.Customers.FirstOrDefaultAsync(c => c.StudentCode == "HS2024001");
        var parent = await context.Customers.Include(c => c.User).FirstOrDefaultAsync(c => c.User.Username == "parent");
        if (teacher == null || student == null || parent == null)
            return;

        var schoolClass = await context.Classes.FirstOrDefaultAsync(c => c.ClassName == "10A1" && c.AcademicYearId == academicYear.Id);
        if (schoolClass == null)
        {
            schoolClass = new Class("10A1", academicYear.Id, gradeLevel.Id, teacher.Id);
            context.Classes.Add(schoolClass);
            await context.SaveChangesAsync();
        }

        if (!await context.StudentClasses.AnyAsync(sc => sc.CustomerId == student.Id && sc.ClassId == schoolClass.Id))
        {
            context.StudentClasses.Add(new StudentClass(student.Id, schoolClass.Id, new DateTime(2026, 9, 5)));
        }

        if (!await context.TeachingAssignments.AnyAsync(ta =>
                ta.EmployeeId == teacher.Id && ta.SubjectId == subject.Id && ta.ClassId == schoolClass.Id && ta.SemesterId == semester.Id))
        {
            context.TeachingAssignments.Add(new TeachingAssignment(teacher.Id, subject.Id, schoolClass.Id, semester.Id));
        }

        if (student.ParentId == null)
        {
            student.AssignParent(parent.Id, "Bố");
        }

        await context.SaveChangesAsync();
    }

    private static async Task EnsureAssessmentTypeAsync(
        AppDbContext context, string typeCode, string typeName, decimal weight, int minEntries, int? maxEntries)
    {
        if (!await context.AssessmentTypes.AnyAsync(t => t.TypeCode == typeCode))
        {
            context.AssessmentTypes.Add(new AssessmentType(typeCode, typeName, weight, minEntries, maxEntries));
        }
    }
}
