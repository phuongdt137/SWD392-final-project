namespace HSMS.MVC.Common;

public static class AttendanceStatuses
{
    public const string Present = "PRESENT";
    public const string Absent = "ABSENT";
    public const string Late = "LATE";
    public const string Excused = "EXCUSED";
    public static readonly string[] All = [Present, Absent, Late, Excused];
    public static bool IsAlert(string? status) => status is Absent or Late;
    public static string Label(string? status) => status switch
    {
        Present => "Có mặt", Absent => "Vắng không phép", Late => "Đi muộn",
        Excused => "Vắng có phép", _ => "Chưa ghi nhận"
    };
    public static string Validate(string? status)
    {
        var value = status?.Trim().ToUpperInvariant();
        if (value == null || !All.Contains(value))
            throw new AppError("Chọn một trong bốn trạng thái điểm danh hợp lệ.", "INVALID_ATTENDANCE_STATUS");
        return value;
    }
}
