namespace HSMS.MVC.Common;

// Tên enum trùng mã trong database; chưa ghi nhận là thiếu bản ghi, không phải trạng thái thứ năm.
public enum AttendanceStatus
{
    PRESENT = 1,
    ABSENT = 2,
    LATE = 3,
    EXCUSED = 4
}

// Đặt các hàm hỗ trợ cùng file enum; giữ API chuỗi để Models hiện tại không cần thay đổi.
public static class AttendanceStatuses
{
    public const string Present = nameof(AttendanceStatus.PRESENT);
    public const string Absent = nameof(AttendanceStatus.ABSENT);
    public const string Late = nameof(AttendanceStatus.LATE);
    public const string Excused = nameof(AttendanceStatus.EXCUSED);
    public static readonly string[] All = Enum.GetNames<AttendanceStatus>();

    /// <summary>Kiểm tra mã chuỗi có thuộc nhóm đi muộn hoặc vắng không phép cần cảnh báo hay không.</summary>
    public static bool IsAlert(string? status) => status is Absent or Late;
    /// <summary>Kiểm tra giá trị enum có thuộc nhóm đi muộn hoặc vắng không phép cần cảnh báo hay không.</summary>
    public static bool IsAlert(this AttendanceStatus status) => status is AttendanceStatus.ABSENT or AttendanceStatus.LATE;

    /// <summary>Đổi mã trạng thái chuỗi thành nhãn tiếng Việt; chưa có mã thì hiển thị chưa ghi nhận.</summary>
    public static string Label(string? status) => status switch
    {
        Present => AttendanceStatus.PRESENT.Label(),
        Absent => AttendanceStatus.ABSENT.Label(),
        Late => AttendanceStatus.LATE.Label(),
        Excused => AttendanceStatus.EXCUSED.Label(),
        _ => "Chưa ghi nhận"
    };

    /// <summary>Đổi giá trị enum điểm danh thành nhãn tiếng Việt để hiển thị.</summary>
    public static string Label(this AttendanceStatus status) => status switch
    {
        AttendanceStatus.PRESENT => "Có mặt",
        AttendanceStatus.ABSENT => "Vắng không phép",
        AttendanceStatus.LATE => "Đi muộn",
        AttendanceStatus.EXCUSED => "Vắng có phép",
        _ => "Chưa ghi nhận"
    };

    /// <summary>Chuẩn hóa tên trạng thái từ chuỗi sang enum; báo lỗi nếu không thuộc bốn trạng thái hợp lệ.</summary>
    public static AttendanceStatus Parse(string? status) => status?.Trim().ToUpperInvariant() switch
    {
        Present => AttendanceStatus.PRESENT,
        Absent => AttendanceStatus.ABSENT,
        Late => AttendanceStatus.LATE,
        Excused => AttendanceStatus.EXCUSED,
        _ => throw InvalidStatus()
    };

    /// <summary>Kiểm tra và chuẩn hóa trạng thái thành mã chuỗi dùng khi lưu kết quả.</summary>
    public static string Validate(string? status) => Parse(status).ToString();

    /// <summary>Kiểm tra giá trị enum thuộc một trong bốn trạng thái điểm danh hợp lệ.</summary>
    public static AttendanceStatus Validate(this AttendanceStatus status) => Enum.IsDefined(status)
        ? status
        : throw InvalidStatus();

    /// <summary>Tạo lỗi yêu cầu chọn một trong bốn trạng thái điểm danh hợp lệ.</summary>
    private static AppError InvalidStatus() =>
        new("Chọn một trong bốn trạng thái điểm danh hợp lệ.", "INVALID_ATTENDANCE_STATUS");
}
