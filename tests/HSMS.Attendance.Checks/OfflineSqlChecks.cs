using System.Data;
using System.Data.Common;
using HSMS.MVC.Data;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace HSMS.Attendance.Checks;

internal static class OfflineSqlChecks
{
    public static async Task RunAsync(Action<bool, string> assert)
    {
        var commands = new EmptyResults();
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=127.0.0.1,1;Database=Offline;User Id=unused;Password=unused;TrustServerCertificate=True")
            .AddInterceptors(new NoConnection(), commands).Options);
        var repo = new AttendanceRepository(db);
        var date = new DateTime(2026, 10, 28);
        await repo.GetScheduleAsync(new() { Date = date, ClassId = 100, Page = 1 }, 30, default);
        await repo.GetClassesAsync(30, default);
        await repo.GetLessonAsync(400, default);
        await repo.GetStudentsAsync(100, date, default);
        await repo.GetSheetAsync(400, date, default);
        await repo.GetLeavesAsync([10], date, default);
        await repo.GetHistoryAsync(new() { SemesterId = 300, SubjectId = 200, Date = date, From = date, To = date, ClassId = 100, StudentId = 10, Status = "LATE", Page = 1 },
            r => r.Customer.ParentId == 20 || r.TimetableEntry.TeachingAssignment.EmployeeId == 30, default);
        await repo.GetHistoryChoicesAsync(r => r.Customer.ParentId == 20, default);
        await repo.GetChildrenAsync(20, default);
        await repo.GetStudentSemestersAsync(10, default);
        await repo.GetStudentSubjectsAsync(10, 300, default);
        await repo.GetRecordAsync(900, r => r.CustomerId == 10, default);
        await repo.GetAuditAsync(900, default);
        assert(commands.Count >= 14, "All Attendance repository queries should compile to SQL");

        // EF phải nhận diện đủ đồ thị kết quả -> nhật ký -> thông báo -> email khi lưu cùng transaction.
        var attendanceRecord = new AttendanceRecord(10, 400, 30, date, "ABSENT");
        repo.AddRecord(attendanceRecord);
        var audit = new AttendanceAuditLog(attendanceRecord, null, null, null, null, 30, DateTime.UtcNow);
        repo.AddAudit(audit);
        var notification = new Notification("Điểm danh", "Vắng không phép", 2, 1);
        notification.AttachAttendance(audit, "/Attendance/History", DateTime.UtcNow);
        repo.AddNotification(notification);
        notification.MessageLogs.Add(new MessageLog("EMAIL", "parent@example.test", "Vắng không phép", templateCode: "ATTENDANCE"));
        db.ChangeTracker.DetectChanges();
        assert(db.ChangeTracker.Entries().Count(e => e.State == EntityState.Added) == 4, "Four new entities must be tracked");
        assert(db.Entry(audit).Property(a => a.AttendanceRecordId).CurrentValue == db.Entry(attendanceRecord).Property(a => a.Id).CurrentValue,
            "Audit must reference the new attendance result");
        assert(db.Entry(notification).Property(n => n.AttendanceAuditLogId).CurrentValue == db.Entry(audit).Property(a => a.Id).CurrentValue,
            "Notification must reference the audit");
        var email = notification.MessageLogs.Single();
        assert(db.Entry(email).Property(m => m.NotificationId).CurrentValue == db.Entry(notification).Property(n => n.Id).CurrentValue,
            "Email must reference the notification");
        Console.WriteLine("PASS SQL generation for every repository query and EF graph relationships (database connection suppressed)");
    }

    private sealed class NoConnection : DbConnectionInterceptor
    {
        public override ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection,
            ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(InterceptionResult.Suppress());
    }

    private sealed class EmptyResults : DbCommandInterceptor
    {
        public int Count { get; private set; }
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result, CancellationToken cancellationToken = default)
        {
            Count++;
            var table = new DataTable();
            if (command.CommandText.TrimStart().StartsWith("SELECT COUNT(*)", StringComparison.Ordinal))
            {
                table.Columns.Add("Count", typeof(int));
                table.Rows.Add(0);
            }
            return ValueTask.FromResult(InterceptionResult<DbDataReader>.SuppressWithResult(table.CreateDataReader()));
        }
    }
}
