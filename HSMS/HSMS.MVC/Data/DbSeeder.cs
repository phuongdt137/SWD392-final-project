using HSMS.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Data;

/// <summary>
/// Hỗ trợ khởi tạo dữ liệu mẫu (Seeder) cho hệ thống HSMS.
/// </summary>
public static class DbSeeder
{
    public const string DefaultPassword = "Password@123";

    public static async Task SeedAdminAccountsAsync(AppDbContext context)
    {
        // 1. Đảm bảo 5 Role chuẩn tồn tại (STUDENT, PARENT, TEACHER, STAFF, PRINCIPAL)
        var studentRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleCode == "STUDENT");
        if (studentRole == null)
        {
            studentRole = new Role("STUDENT", "Student", "High school student", true);
            context.Roles.Add(studentRole);
            await context.SaveChangesAsync();
        }

        var parentRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleCode == "PARENT");
        if (parentRole == null)
        {
            parentRole = new Role("PARENT", "Parent", "Parent or guardian of student", true);
            context.Roles.Add(parentRole);
            await context.SaveChangesAsync();
        }

        var teacherRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleCode == "TEACHER");
        if (teacherRole == null)
        {
            teacherRole = new Role("TEACHER", "Teacher", "School teacher", true);
            context.Roles.Add(teacherRole);
            await context.SaveChangesAsync();
        }

        var staffRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleCode == "STAFF");
        if (staffRole == null)
        {
            staffRole = new Role("STAFF", "Staff", "School staff", true);
            context.Roles.Add(staffRole);
            await context.SaveChangesAsync();
        }

        var principalRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleCode == "PRINCIPAL");
        if (principalRole == null)
        {
            principalRole = new Role("PRINCIPAL", "Principal", "School principal", true);
            context.Roles.Add(principalRole);
            await context.SaveChangesAsync();
        }

        // Băm mật khẩu mặc định
        var defaultPasswordHash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword);

        // 2. Tài khoản Staff mẫu
        var staffAccounts = new[]
        {
            new
            {
                Username = "staff",
                Email = "phuongw98yb@gmail.com",
                FullName = "Cán Bộ Quản Trị Học Vụ",
                PhoneNumber = "0901234567",
                EmployeeCode = "EMP_STF001",
                Department = "Phòng Học Vụ",
                Position = "Trưởng Phòng Quản Trị Học Vụ"
            },
            new
            {
                Username = "staff.tech",
                Email = "tech.staff@hsms.edu.vn",
                FullName = "Trần Kỹ Thuật Học Vụ",
                PhoneNumber = "0903456789",
                EmployeeCode = "EMP_STF002",
                Department = "Phòng CNTT & Học Vụ",
                Position = "Chuyên Viên Hệ Thống"
            }
        };

        foreach (var acc in staffAccounts)
        {
            var existingUser = await context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.EmployeeProfile)
                .FirstOrDefaultAsync(u => u.Username == acc.Username || u.Email == acc.Email);

            if (existingUser == null)
            {
                var user = new User(
                    acc.Username,
                    acc.Email,
                    defaultPasswordHash,
                    acc.FullName,
                    acc.PhoneNumber,
                    avatarUrl: null,
                    isActive: true
                );
                context.Users.Add(user);
                await context.SaveChangesAsync();

                context.UserRoles.Add(new UserRole(user.Id, staffRole.Id, isActive: true));
                context.Employees.Add(new Employee(
                    user.Id,
                    acc.EmployeeCode,
                    department: acc.Department,
                    position: acc.Position,
                    specialization: "Quản lý giáo dục & CNTT",
                    qualification: "Đại học",
                    hireDate: new DateTime(2024, 1, 1),
                    gender: "Nam",
                    dateOfBirth: new DateTime(1992, 1, 1),
                    address: "Việt Nam"
                ));

                await context.SaveChangesAsync();
            }
            else
            {
                existingUser.UpdateProfile(acc.FullName, acc.PhoneNumber, existingUser.AvatarUrl);
                existingUser.ChangePassword(defaultPasswordHash);
                if (!existingUser.UserRoles.Any(ur => ur.RoleId == staffRole.Id && ur.IsActive))
                {
                    context.UserRoles.Add(new UserRole(existingUser.Id, staffRole.Id, isActive: true));
                }
                await context.SaveChangesAsync();
            }
        }

        // 3. Tài khoản Hiệu trưởng mẫu (Principal)
        var principalUser = await context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Username == "principal" || u.Email == "principal@hsms.edu.vn");
        if (principalUser == null)
        {
            var principal = new User(
                "principal",
                "principal@hsms.edu.vn",
                defaultPasswordHash,
                "Thầy Nguyễn Văn Hiệu Trưởng",
                "0902345678",
                avatarUrl: null,
                isActive: true
            );
            context.Users.Add(principal);
            await context.SaveChangesAsync();

            context.UserRoles.Add(new UserRole(principal.Id, principalRole.Id, isActive: true));
            context.Employees.Add(new Employee(
                principal.Id,
                "EMP_PRI001",
                department: "Ban Giám Hiệu",
                position: "Hiệu Trưởng Nhà Trường",
                specialization: "Quản lý Giáo dục",
                qualification: "Tiến sĩ Giáo dục học",
                hireDate: new DateTime(2020, 1, 1),
                gender: "Nam",
                dateOfBirth: new DateTime(1975, 5, 10),
                address: "Hà Nội"
            ));
            await context.SaveChangesAsync();
        }
        else
        {
            principalUser.UpdateProfile("Thầy Nguyễn Văn Hiệu Trưởng", "0902345678", principalUser.AvatarUrl);
            principalUser.ChangePassword(defaultPasswordHash);
            if (!principalUser.UserRoles.Any(ur => ur.RoleId == principalRole.Id && ur.IsActive))
            {
                context.UserRoles.Add(new UserRole(principalUser.Id, principalRole.Id, isActive: true));
            }
            await context.SaveChangesAsync();
        }

        // 4. Tài khoản Giáo viên mẫu (Teacher)
        var teacherUser = await context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Username == "teacher");
        if (teacherUser == null)
        {
            var teacher = new User(
                "teacher",
                "teacher@hsms.edu.vn",
                defaultPasswordHash,
                "Cô Nguyễn Thu Hà",
                "0912345678",
                avatarUrl: null,
                isActive: true
            );
            context.Users.Add(teacher);
            await context.SaveChangesAsync();

            context.UserRoles.Add(new UserRole(teacher.Id, teacherRole.Id, isActive: true));
            context.Employees.Add(new Employee(
                teacher.Id,
                "EMP_TEA001",
                department: "Tổ Toán - Tin",
                position: "Giáo viên Toán & Tin học",
                specialization: "Sư phạm Toán học",
                qualification: "Cử nhân Sư phạm",
                hireDate: new DateTime(2023, 8, 1),
                gender: "Nữ",
                dateOfBirth: new DateTime(1990, 5, 15),
                address: "Hà Nội"
            ));
            await context.SaveChangesAsync();
        }
        else
        {
            teacherUser.UpdateProfile("Cô Nguyễn Thu Hà", "0912345678", teacherUser.AvatarUrl);
            teacherUser.ChangePassword(defaultPasswordHash);
            await context.SaveChangesAsync();
        }

        // 5. Tài khoản Học sinh mẫu (Student)
        var studentUser = await context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Username == "student");
        if (studentUser == null)
        {
            var student = new User(
                "student",
                "student@hsms.edu.vn",
                defaultPasswordHash,
                "Nguyễn Hoàng Long",
                "0987654321",
                avatarUrl: null,
                isActive: true
            );
            context.Users.Add(student);
            await context.SaveChangesAsync();

            context.UserRoles.Add(new UserRole(student.Id, studentRole.Id, isActive: true));
            context.Customers.Add(new Customer(
                student.Id,
                studentCode: "HS2024001",
                admissionDate: new DateTime(2024, 9, 5),
                gender: "Nam",
                dateOfBirth: new DateTime(2008, 10, 20),
                address: "Hà Nội"
            ));
            await context.SaveChangesAsync();
        }
        else
        {
            studentUser.UpdateProfile("Nguyễn Hoàng Long", "0987654321", studentUser.AvatarUrl);
            studentUser.ChangePassword(defaultPasswordHash);
            await context.SaveChangesAsync();
        }

        // 6. Tài khoản Phụ huynh mẫu (Parent)
        var parentUser = await context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.Username == "parent");
        if (parentUser == null)
        {
            var parent = new User(
                "parent",
                "parent@hsms.edu.vn",
                defaultPasswordHash,
                "Nguyễn Văn Bình (Phụ huynh)",
                "0978901234",
                avatarUrl: null,
                isActive: true
            );
            context.Users.Add(parent);
            await context.SaveChangesAsync();

            context.UserRoles.Add(new UserRole(parent.Id, parentRole.Id, isActive: true));
            context.Customers.Add(new Customer(
                parent.Id,
                occupation: "Kỹ sư",
                emergencyContact: "0978901234",
                relationshipType: "Bố",
                gender: "Nam",
                dateOfBirth: new DateTime(1980, 3, 10),
                address: "Hà Nội"
            ));
            await context.SaveChangesAsync();
        }
        else
        {
            parentUser.UpdateProfile("Nguyễn Văn Bình (Phụ huynh)", "0978901234", parentUser.AvatarUrl);
            parentUser.ChangePassword(defaultPasswordHash);
            await context.SaveChangesAsync();
        }
    }
}
