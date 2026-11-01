using HSMS.MVC.Common;
using HSMS.MVC.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HSMS.MVC.Data;

public partial class AppDbContext
{
    // Kiểm tra EF đánh dấu thay đổi thuộc tính nào; chỉ bảo vệ các trường làm đổi ý nghĩa lịch sử.
    private static bool Changes(EntityEntry entry, params string[] names) => names.Any(name => entry.Property(name).IsModified);
    // Tìm kết quả sẽ bị đổi ngữ cảnh hoặc bị nằm ngoài hiệu lực nếu cập nhật lịch hiện tại.
    private IQueryable<AttendanceRecord> HistoricalChanges(EntityEntry<TimetableEntry> entry)
    {
        var e = entry.Entity;
        var changesMeaning = Changes(entry, nameof(e.TeachingAssignmentId), nameof(e.DayOfWeek), nameof(e.TimeSlotId), nameof(e.RoomId));
        return AttendanceRecords.Where(r => r.TimetableEntryId == e.Id && (changesMeaning ||
            (e.EffectiveFrom.HasValue && r.AttendanceDate < e.EffectiveFrom) || (e.EffectiveTo.HasValue && r.AttendanceDate > e.EffectiveTo)));
    }
    // Tạo các truy vấn kiểm tra lịch/phân công Modified đang được theo dõi trước khi ghi SQL.
    private IEnumerable<IQueryable<AttendanceRecord>> HistoryChecks()
    {
        foreach (var e in ChangeTracker.Entries<TimetableEntry>().Where(x => x.State == EntityState.Modified).ToList())
            yield return HistoricalChanges(e);
        foreach (var e in ChangeTracker.Entries<TeachingAssignment>().Where(x => x.State == EntityState.Modified).ToList())
            if (Changes(e, nameof(TeachingAssignment.ClassId), nameof(TeachingAssignment.EmployeeId), nameof(TeachingAssignment.SubjectId), nameof(TeachingAssignment.SemesterId)))
            {
                var id = e.Entity.Id;
                yield return AttendanceRecords.Where(r => r.TimetableEntry.TeachingAssignmentId == id);
            }
    }
    private static AppError ImmutableSchedule() => new("Lịch hoặc phân công đã có điểm danh. Hãy tạo phiên bản lịch mới để giữ nguyên lịch sử.", "ATTENDANCE_HISTORY_IMMUTABLE", ErrorType.Conflict);
    // Bảo vệ lịch sử cả khi module khác dùng đường lưu đồng bộ của DbContext.
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        foreach (var query in HistoryChecks()) if (query.Any()) throw ImmutableSchedule();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }
    // Bảo vệ tương tự với đường lưu bất đồng bộ; có điểm danh liên quan thì yêu cầu tạo lịch mới.
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        foreach (var query in HistoryChecks()) if (await query.AnyAsync(cancellationToken)) throw ImmutableSchedule();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }
}
