using HSMS.MVC.Models;
using Microsoft.EntityFrameworkCore;
namespace HSMS.MVC.Data;

public static partial class DbSeeder
{
    public const string DefaultPassword = "Password@123";
    public static async Task SeedAdminAccountsAsync(AppDbContext db)
    {
        foreach (var code in new[] { "STAFF", "PRINCIPAL", "TEACHER", "STUDENT", "PARENT" })
            if (!await db.Roles.AnyAsync(r => r.RoleCode == code)) db.Roles.Add(new Role(code, code));
        await db.SaveChangesAsync();
        var hash = BCrypt.Net.BCrypt.HashPassword(DefaultPassword);
        var employees = new[] {
            ("staff", "EMP_STF001", "Cán bộ học vụ", "STAFF"),
            ("staff.tech", "EMP_STF002", "Nhân viên kỹ thuật", "STAFF"),
            ("principal", "EMP_PRI001", "Hiệu trưởng", "PRINCIPAL"),
            ("teacher", "EMP_TEA001", "Giáo viên mẫu", "TEACHER")
        };
        foreach (var (username, code, name, roleCode) in employees)
        {
            var employee = await db.Employees.SingleOrDefaultAsync(e => e.Username == username);
            if (employee == null)
            {
                employee = new Employee(code, username, username + "@hsms.edu.vn", hash, name);
                db.Employees.Add(employee);
                await db.SaveChangesAsync();
            }
            var role = await db.Roles.SingleAsync(r => r.RoleCode == roleCode);
            if (!await db.EmployeeRoles.AnyAsync(r => r.EmployeeId == employee.Id && r.RoleId == role.Id))
                db.EmployeeRoles.Add(new EmployeeRole(employee.Id, role.Id));
        }
        foreach (var (username, name, roleCode) in new[] { ("parent", "Phụ huynh mẫu", "PARENT"), ("student", "Học sinh mẫu", "STUDENT") })
        {
            var customer = await db.Customers.SingleOrDefaultAsync(c => c.Username == username);
            if (customer == null)
            {
                customer = new Customer(username, username + "@hsms.edu.vn", hash, name,
                    studentCode: roleCode == "STUDENT" ? "HS2024001" : null);
                db.Customers.Add(customer);
                await db.SaveChangesAsync();
            }
            var role = await db.Roles.SingleAsync(r => r.RoleCode == roleCode);
            if (!await db.CustomerRoles.AnyAsync(r => r.CustomerId == customer.Id && r.RoleId == role.Id))
                db.CustomerRoles.Add(new CustomerRole(customer.Id, role.Id));
        }
        await db.SaveChangesAsync();
    }
}
