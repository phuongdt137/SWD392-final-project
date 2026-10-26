namespace HSMS.MVC.Common;

/// <summary>School dates use Vietnam time; persisted event timestamps use UTC.</summary>
public sealed class SchoolTime(TimeProvider clock)
{
    public static readonly TimeSpan Offset = TimeSpan.FromHours(7);
    public DateTime UtcNow => clock.GetUtcNow().UtcDateTime;
    public DateTime Today => Local(UtcNow).Date;
    public static DateTime Local(DateTime utc) => DateTime.SpecifyKind(utc, DateTimeKind.Utc).Add(Offset);
    public static DateTime LockUtc(DateTime date) => DateTime.SpecifyKind(date.Date.AddDays(1).Subtract(Offset), DateTimeKind.Utc);
    public static int Weekday(DateTime date) => ((int)date.DayOfWeek + 6) % 7 + 1;
    public bool IsLocked(DateTime date) => UtcNow >= LockUtc(date);
    public void EnsureWritable(DateTime date, TimeSpan start, bool allowAfterDeadline = false)
    {
        var now = UtcNow;
        if (!allowAfterDeadline && now >= LockUtc(date))
            throw new AppError("Bảng đã khóa lúc 00:00 ngày kế tiếp theo giờ Việt Nam.", "ATTENDANCE_LOCKED", ErrorType.Forbidden);
        if (Local(now) < date.Date.Add(start))
            throw new AppError("Chưa đến giờ bắt đầu tiết học.", "ATTENDANCE_NOT_STARTED");
    }
}
