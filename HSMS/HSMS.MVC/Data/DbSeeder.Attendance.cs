using System.Linq.Expressions;
using HSMS.MVC.Common;
using HSMS.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Data;

public static partial class DbSeeder
{
    /// <summary>Development sample data. Re-running preserves saved attendance and existing passwords.</summary>
    public static async Task SeedAttendanceDemoAsync(AppDbContext db, SchoolTime time)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();
        var today = time.Today;
        // Chọn năm học/học kỳ mẫu bao phủ ngày Việt Nam hiện tại để có thể mở luồng demo ngay.
        var startYear = today.Month >= 8 ? today.Year : today.Year - 1;
        var yearStart = new DateTime(startYear, 8, 1);
        var yearEnd = new DateTime(startYear + 1, 7, 31);
        var firstTerm = today.Month >= 8 || today.Month == 1;
        var termStart = firstTerm ? yearStart : new DateTime(startYear + 1, 2, 1);
        var termEnd = firstTerm ? new DateTime(startYear + 1, 1, 31) : yearEnd;
        var termCode = $"ATT-DEMO-{startYear}-HK{(firstTerm ? 1 : 2)}";
        var yearCode = $"ATT-DEMO-{startYear}-{startYear + 1}";
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword);

        // Tái sử dụng dữ liệu theo khóa nhận diện, chỉ tạo khi thiếu; không ghi đè kết quả có sẵn.
        async Task<T> GetOrAdd<T>(DbSet<T> set, Expression<Func<T, bool>> predicate, Func<T> create) where T : class
        {
            var item = await set.SingleOrDefaultAsync(predicate);
            if (item != null) return item;
            item = create(); set.Add(item); await db.SaveChangesAsync(); return item;
        }
        // Tạo tài khoản demo khi thiếu và bổ sung role; không đặt lại mật khẩu tài khoản đã tồn tại.
        async Task<User> DemoUser(string username, string name, string roleCode)
        {
            var user = await GetOrAdd(db.Users, u => u.Username == username,
                () => new User(username, username + "@example.invalid", passwordHash, name));
            var role = await db.Roles.SingleAsync(r => r.RoleCode == roleCode);
            if (!await db.UserRoles.AnyAsync(r => r.UserId == user.Id && r.RoleId == role.Id))
            { db.UserRoles.Add(new UserRole(user.Id, role.Id)); await db.SaveChangesAsync(); }
            return user;
        }
        // Dùng hồ sơ Customer chung cho học sinh/phụ huynh; chỉ gắn phụ huynh nếu chưa có liên kết.
        async Task<Customer> Profile(User user, string? code = null, long? parentId = null)
        {
            var customer = await GetOrAdd(db.Customers, c => c.UserId == user.Id,
                () => new Customer(user.Id, studentCode: code, admissionDate: code == null ? null : yearStart,
                    parentId: parentId, relationshipType: parentId.HasValue ? "Con" : null));
            if (parentId.HasValue && !customer.ParentId.HasValue)
            { customer.AssignParent(parentId.Value, "Con"); await db.SaveChangesAsync(); }
            return customer;
        }

        // Reuse the existing sample accounts created by SeedAdminAccountsAsync.
        var teacherUser = await db.Users.SingleAsync(u => u.Username == "teacher");
        var teacher = await GetOrAdd(db.Employees, e => e.UserId == teacherUser.Id,
            () => new Employee(teacherUser.Id, "ATT-DEMO-TEACHER", position: "Giáo viên mẫu"));
        var parentUser = await db.Users.SingleAsync(u => u.Username == "parent");
        var parent = await Profile(parentUser);
        var studentUser = await db.Users.SingleAsync(u => u.Username == "student");
        var student = await Profile(studentUser, "HS2024001", parent.Id);
        var secondParent = await Profile(await DemoUser("attendance.parent2", "Lê Thu Mai — Phụ huynh mẫu 2", "PARENT"));
        var second = await Profile(await DemoUser("attendance.student2", "Nguyễn Ngọc Linh", "STUDENT"), "ATT-DEMO-HS02", parent.Id);
        var third = await Profile(await DemoUser("attendance.student3", "Trần Minh Khang", "STUDENT"), "ATT-DEMO-HS03", secondParent.Id);
        var fourth = await Profile(await DemoUser("attendance.student4", "Lê Gia Hân", "STUDENT"), "ATT-DEMO-HS04", secondParent.Id);
        var adminRole = await GetOrAdd(db.Roles, r => r.RoleCode == "ADMIN", () => new Role("ADMIN", "Administrator"));
        var adminUser = await DemoUser("attendance.admin", "Quản trị viên điểm danh mẫu", adminRole.RoleCode);
        await GetOrAdd(db.Employees, e => e.UserId == adminUser.Id,
            () => new Employee(adminUser.Id, "ATT-DEMO-ADMIN", department: "Quản trị", position: "ADMIN"));

        // Tạo ngữ cảnh học vụ mẫu bằng các bảng sẵn có: năm, kỳ, lớp, học sinh, môn và phân công.
        var year = await GetOrAdd(db.AcademicYears, y => y.YearCode == yearCode, () => new AcademicYear(yearCode, yearStart, yearEnd));
        var semester = await GetOrAdd(db.Semesters, s => s.SemesterCode == termCode,
            () => new Semester(termCode, "Học kỳ mẫu " + (firstTerm ? "1" : "2"), termStart, termEnd, year.Id));
        var grade = await GetOrAdd(db.GradeLevels, g => g.LevelCode == "ATT-DEMO-10", () => new GradeLevel("ATT-DEMO-10", "Khối 10 mẫu"));
        var classroom = await GetOrAdd(db.Classes, c => c.AcademicYearId == year.Id && c.ClassName == "10A1 — Điểm danh mẫu",
            () => new Class("10A1 — Điểm danh mẫu", year.Id, grade.Id, teacher.Id));
        foreach (var pupil in new[] { student, second, third, fourth })
            await GetOrAdd(db.StudentClasses, s => s.CustomerId == pupil.Id && s.ClassId == classroom.Id,
                () => new StudentClass(pupil.Id, classroom.Id, yearStart));
        var math = await GetOrAdd(db.Subjects, s => s.SubjectCode == "ATT-DEMO-TOAN", () => new Subject("ATT-DEMO-TOAN", "Toán (mẫu)"));
        var literature = await GetOrAdd(db.Subjects, s => s.SubjectCode == "ATT-DEMO-VAN", () => new Subject("ATT-DEMO-VAN", "Ngữ văn (mẫu)"));
        var room = await GetOrAdd(db.Rooms, r => r.RoomCode == "ATT-DEMO-P101", () => new Room("ATT-DEMO-P101", "Phòng học mẫu 101", capacity: 40));
        var subjects = new[] { math, literature };
        var assignments = new List<TeachingAssignment>();
        foreach (var subject in subjects)
            assignments.Add(await GetOrAdd(db.TeachingAssignments, a => a.EmployeeId == teacher.Id && a.SubjectId == subject.Id && a.ClassId == classroom.Id && a.SemesterId == semester.Id,
                () => new TeachingAssignment(teacher.Id, subject.Id, classroom.Id, semester.Id)));

        // The first sample period is already open at any hour; normal midnight locking still applies.
        var starts = new[] { 0, 7, 9, 13 };
        for (var index = 0; index < starts.Length; index++)
        {
            var code = "ATT-DEMO-T" + (index + 1);
            var slot = await GetOrAdd(db.TimeSlots, s => s.SlotCode == code,
                () => new TimeSlot(code, index + 1, TimeSpan.FromHours(starts[index]), TimeSpan.FromHours(starts[index]).Add(TimeSpan.FromMinutes(45)), "DEMO"));
            var assignment = assignments[index % assignments.Count];
            for (byte day = 1; day <= 7; day++)
                await GetOrAdd(db.TimetableEntries, e => e.TeachingAssignmentId == assignment.Id && e.TimeSlotId == slot.Id && e.DayOfWeek == day,
                    () => new TimetableEntry(assignment.Id, room.Id, slot.Id, day, termStart, termEnd));
        }

        // Approved leave is only suggested on the sheet; no attendance results are invented.
        await GetOrAdd(db.LeaveRequests, l => l.StudentCustomerId == second.Id && l.StartDate == today && l.EndDate == today && l.Reason == "Đơn mẫu: nghỉ ốm đã được duyệt",
            () => { var leave = new LeaveRequest(second.Id, parent.Id, "Đơn mẫu: nghỉ ốm đã được duyệt", today, today); leave.Approve(teacher, "Dữ liệu mẫu để thử trạng thái vắng có phép"); return leave; });
        await GetOrAdd(db.LeaveRequests, l => l.StudentCustomerId == fourth.Id && l.StartDate == today && l.EndDate == today && l.Reason == "Đơn mẫu: đang chờ duyệt",
            () => new LeaveRequest(fourth.Id, secondParent.Id, "Đơn mẫu: đang chờ duyệt", today, today));
        // Hoàn tất dữ liệu demo trong một transaction; hàm không tự ghi kết quả điểm danh.
        await transaction.CommitAsync();
    }
}
