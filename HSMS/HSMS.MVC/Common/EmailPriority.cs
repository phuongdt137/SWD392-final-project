namespace HSMS.MVC.Common;

/// <summary>
/// Độ ưu tiên gửi email trong PriorityQueue (Min-Heap: số nhỏ hơn được xử lý trước).
/// </summary>
public enum EmailPriority
{
    /// <summary>
    /// Ưu tiên tối đa (OTP, Đặt lại mật khẩu, Cảnh báo bảo mật) -> xử lý đầu tiên.
    /// </summary>
    Critical = 1,

    /// <summary>
    /// Ưu tiên cao (Điểm danh vắng học, Thông báo khẩn cho phụ huynh).
    /// </summary>
    High = 2,

    /// <summary>
    /// Ưu tiên thông thường (Thông báo điểm số, cập nhật lớp học).
    /// </summary>
    Normal = 3,

    /// <summary>
    /// Ưu tiên thấp (Báo cáo định kỳ, bản tin hàng loạt).
    /// </summary>
    Low = 4
}
