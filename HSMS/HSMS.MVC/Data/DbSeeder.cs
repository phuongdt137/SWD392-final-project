using HSMS.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Data;

/// <summary>
/// Hỗ trợ khởi tạo dữ liệu mẫu (Seeder) cho hệ thống HSMS.
/// </summary>
public static class DbSeeder
{
    public const string DefaultPassword = "Admin@123";

    public static async Task SeedAdminAccountsAsync(AppDbContext context)
    {
        // 1. Đảm bảo Role 'ADMIN' tồn tại
        var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleCode == "ADMIN");
        if (adminRole == null)
        {
            adminRole = new Role("ADMIN", "Quản trị viên", "Quản trị viên toàn quyền hệ thống", true);
            context.Roles.Add(adminRole);
            await context.SaveChangesAsync();
        }

        // Danh sách 3 tài khoản Admin mẫu
        var adminAccounts = new[]
        {
            new
            {
                Username = "admin",
                Email = "phuongw98yb@gmai.com",
                FullName = "Quản Trị Viên Hệ Thống",
                PhoneNumber = "0901234567",
                EmployeeCode = "EMP_ADM001",
                Department = "Ban Giám Hiệu",
                Position = "Trưởng Ban Quản Trị CNTT"
            },
            new
            {
                Username = "admin.nguyen",
                Email = "nguyen.admin@hsms.edu.vn",
                FullName = "Nguyễn Văn Quản Trị",
                PhoneNumber = "0902345678",
                EmployeeCode = "EMP_ADM002",
                Department = "Ban Giám Hiệu",
                Position = "Phó Hiệu Trưởng / Quản Trị Học Vụ"
            },
            new
            {
                Username = "admin.tech",
                Email = "tech.admin@hsms.edu.vn",
                FullName = "Trần Kỹ Thuật Admin",
                PhoneNumber = "0903456789",
                EmployeeCode = "EMP_ADM003",
                Department = "Phòng Kỹ Thuật CNTT",
                Position = "Kỹ Sư Phần Mềm & Hạ Tầng"
            }
        };

        foreach (var acc in adminAccounts)
        {
            var existingUser = await context.Users
                .Include(u => u.UserRoles)
                .Include(u => u.EmployeeProfile)
                .FirstOrDefaultAsync(u => u.Username == acc.Username || u.Email == acc.Email);

            // Băm mật khẩu trực tiếp bằng BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword);

            if (existingUser == null)
            {
                // 1. Tạo User
                var user = new User(
                    acc.Username,
                    acc.Email,
                    passwordHash,
                    acc.FullName,
                    acc.PhoneNumber,
                    avatarUrl: null,
                    isActive: true
                );
                context.Users.Add(user);
                await context.SaveChangesAsync();

                // 2. Gán Role ADMIN
                var userRole = new UserRole(user.Id, adminRole.Id, isActive: true);
                context.UserRoles.Add(userRole);

                // 3. Tạo Employee Profile
                var employee = new Employee(
                    user.Id,
                    acc.EmployeeCode,
                    department: acc.Department,
                    position: acc.Position,
                    specialization: "Công nghệ thông tin",
                    qualification: "Đại học / Thạc sĩ",
                    hireDate: new DateTime(2024, 1, 1),
                    gender: "Nam",
                    dateOfBirth: new DateTime(1985, 1, 1),
                    address: "Việt Nam"
                );
                context.Employees.Add(employee);

                await context.SaveChangesAsync();
            }
            else
            {
                // Đảm bảo mật khẩu và email luôn đồng bộ chuẩn xác
                existingUser.ChangePassword(passwordHash);
                await context.SaveChangesAsync();
            }
        }
    }
}

