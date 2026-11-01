namespace HSMS.MVC.Common;

// Bốn trạng thái được lưu trong database; chưa ghi nhận là thiếu bản ghi, không thêm trạng thái thứ năm.
public static class AttendanceStatuses
{
    public const string Present = "PRESENT";
    public const string Absent = "ABSENT";
    public const string Late = "LATE";
    public const string Excused = "EXCUSED";
    public static readonly string[] All = [Present, Absent, Late, Excused];
    // Nhóm cần cảnh báo; Service còn dùng trạng thái cũ để gửi thông báo khi sửa khỏi nhóm này.
    public static bool IsAlert(string? status) => status is Absent or Late;
    // Nhãn tiếng Việt dùng chung; null/giá trị không nhận diện hiển thị chưa ghi nhận.
    public static string Label(string? status) => status switch
    {
        Present => "Có mặt", Absent => "Vắng không phép", Late => "Đi muộn",
        Excused => "Vắng có phép", _ => "Chưa ghi nhận"
    };
    // Chuẩn hóa mã rồi từ chối giá trị ngoài danh sách cho phép.
    public static string Validate(string? status)
    {
        var value = status?.Trim().ToUpperInvariant();
        if (value == null || !All.Contains(value))
            throw new AppError("Chọn một trong bốn trạng thái điểm danh hợp lệ.", "INVALID_ATTENDANCE_STATUS");
        return value;
    }
}
