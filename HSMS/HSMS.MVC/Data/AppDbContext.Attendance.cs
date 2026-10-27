using HSMS.MVC.Models;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Data;

public partial class AppDbContext
{
    public DbSet<AttendanceAuditLog> AttendanceAuditLogs => Set<AttendanceAuditLog>();
    // Bổ sung mapping cho entity hiện có; các ràng buộc database hỗ trợ quy tắc Service.
    private static void ConfigureAttendance(ModelBuilder builder)
    {
        builder.Entity<AttendanceRecord>(e =>
        {
            // ConcurrencyToken đưa Version vào điều kiện UPDATE để phát hiện người khác đã sửa.
            e.Property(x => x.Version).IsConcurrencyToken().HasDefaultValueSql("NEWID()");
            e.HasIndex(x => new { x.TimetableEntryId, x.AttendanceDate });
            // Chỉ bốn trạng thái; EXCUSED phải có đơn nghỉ và trạng thái khác không được giữ liên kết đơn.
            e.ToTable("AttendanceRecord", t => t.HasCheckConstraint("CK_AttendanceRecord_Status",
                "[Status] IN ('PRESENT','ABSENT','LATE','EXCUSED') AND (([Status] = 'EXCUSED' AND [LeaveRequestId] IS NOT NULL) OR ([Status] <> 'EXCUSED' AND [LeaveRequestId] IS NULL))"));
            e.HasOne(x => x.UpdatedByEmployee).WithMany().HasForeignKey(x => x.UpdatedByEmployeeId).OnDelete(DeleteBehavior.NoAction);
        });
        // Nhật ký giữ người sửa và giá trị trước/sau; NoAction tránh xóa dây chuyền làm mất dấu vết.
        builder.Entity<AttendanceAuditLog>(e =>
        {
            e.ToTable("AttendanceAuditLog");
            e.Property(x => x.OldStatus).HasMaxLength(30);
            e.Property(x => x.NewStatus).HasMaxLength(30).IsRequired();
            e.Property(x => x.OldComment).HasMaxLength(1000);
            e.Property(x => x.NewComment).HasMaxLength(1000);
            e.Property(x => x.ChangeReason).HasMaxLength(1000);
            e.HasOne(x => x.AttendanceRecord).WithMany().HasForeignKey(x => x.AttendanceRecordId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.ChangedByEmployee).WithMany().HasForeignKey(x => x.ChangedByEmployeeId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(x => new { x.AttendanceRecordId, x.ChangedAt });
        });
        builder.Entity<Notification>(e =>
        {
            e.Property(x => x.Link).HasMaxLength(500);
            e.HasOne(x => x.AttendanceAuditLog).WithMany().HasForeignKey(x => x.AttendanceAuditLogId).OnDelete(DeleteBehavior.NoAction);
            // Mỗi audit tối đa một thông báo liên kết; các chỉ mục còn lại phục vụ hàng chờ và hộp thông báo.
            e.HasIndex(x => x.AttendanceAuditLogId).IsUnique().HasFilter("[AttendanceAuditLogId] IS NOT NULL");
            e.HasIndex(x => new { x.DispatchedAt, x.NextDeliveryAttemptAt }).HasFilter("[AttendanceAuditLogId] IS NOT NULL AND [DispatchedAt] IS NULL");
            e.HasIndex(x => new { x.RecipientId, x.IsRead, x.SentAt });
        });
    }
}
