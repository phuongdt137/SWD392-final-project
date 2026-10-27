using System.Security.Claims;
using HSMS.MVC.Common;
using HSMS.MVC.Data;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using HSMS.MVC.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

public static class AccountChecks
{
    public static async Task RunAsync()
    {
        void Check(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
            Console.WriteLine("PASS: " + message);
        }
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=.;Database=hsms-db;Integrated Security=true;TrustServerCertificate=true").Options);
        var tables = db.Model.GetEntityTypes().Select(x => x.GetTableName()).ToList();
        Check(!tables.Contains("User") && !tables.Contains("UserRole") && !tables.Contains("IAccount"), "EF has no User/UserRole/account table");
        Check(tables.Contains("EmployeeRole") && tables.Contains("CustomerRole"), "Separate employee/customer role mappings");
        var sql = db.Employees.Where(e => e.IsActive && e.EmployeeRoles.Any(r => r.IsActive && r.Role.IsActive && r.Role.RoleCode == "TEACHER"))
            .Select(e => new { e.Id, e.FullName }).ToQueryString();
        Check(sql.Contains("[Employee]") && sql.Contains("[EmployeeRole]") && !sql.Contains("[User]"), "Teacher query translates without User join");
        Check(!db.Database.HasPendingModelChanges(), "Migration snapshot matches current model");
        var migrations = db.GetService<IMigrationsAssembly>();
        Check(!migrations.Migrations.Keys.Any(x => x.Contains("ToUser") || x.Contains("Refactor-Auth")), "Removed unapplied User migrations");
        var latest = migrations.CreateMigration(migrations.Migrations.Last().Value, db.Database.ProviderName!);
        Check(!latest.UpOperations.Any(x => x is DropColumnOperation or DropTableOperation or DeleteDataOperation), "New migration keeps existing columns/tables/data");
        var employee = new Employee("TEST", "employee", "employee@example.invalid", "hash", "Employee");
        var customer = new Customer("customer", "customer@example.invalid", "hash", "Customer");
        typeof(Employee).GetProperty("Id")!.SetValue(employee, 1L);
        typeof(Customer).GetProperty("Id")!.SetValue(customer, 1L);
        var accounts = new Accounts(employee, customer);
        var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        var current = new CurrentUserService(http, accounts);
        http.HttpContext.User = UserPrincipal.Create(1, "Employee", new[] { "TEACHER" }, "EMPLOYEE");
        var first = await current.GetAsync();
        Check(first?.EmployeeId == 1 && first.CustomerId == null, "Employee ID 1 resolves only Employee");
        http.HttpContext.User = UserPrincipal.Create(1, "Customer", new[] { "STUDENT" }, "CUSTOMER");
        var second = await current.GetAsync();
        Check(second?.CustomerId == 1 && second.EmployeeId == null, "Customer ID 1 resolves only Customer");
        http.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "1") }, "Cookies"));
        Check(await current.GetAsync() == null, "Old cookie without account type is rejected");
        var notification = new Notification("Test", "Test", senderEmployeeId: 1, recipientCustomerId: 1);
        Check(notification.SenderEmployeeId == 1 && notification.RecipientCustomerId == 1 && notification.RecipientEmployeeId == null,
            "Notification distinguishes sender Employee and recipient Customer with equal IDs");
        // Điểm danh không thay đổi quyền truy cập khi hai bảng cùng ID.
        var entry = new TimetableEntry(1,1,1,1);
        var assignment = new TeachingAssignment(1,1,1,1);
        typeof(TimetableEntry).GetProperty("TeachingAssignment")!.SetValue(entry, assignment);
        Check(!AttendanceAccess.CanWrite(second!, entry), "Student cannot edit attendance through colliding Employee ID");
    }
    private class Accounts(Employee employee, Customer customer) : IAccountRepository
    {
        public Task<IAccount?> GetAsync(long id, string type) => Task.FromResult<IAccount?>(id != 1 ? null : type == "EMPLOYEE" ? employee : type == "CUSTOMER" ? customer : null);
        public Task<IAccount?> FindByLoginAsync(string value) => throw new NotSupportedException();
        public Task<IAccount?> FindByEmailAsync(string value) => throw new NotSupportedException();
        public Task SaveChangesAsync() => Task.CompletedTask;
    }
}
