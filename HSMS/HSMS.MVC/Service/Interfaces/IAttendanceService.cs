using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.ViewModels.Attendance;

namespace HSMS.MVC.Service;

public interface IAttendanceService
{
    // Controller kiểm tra vai trò bằng [Authorize]; Service xử lý phạm vi dữ liệu và nghiệp vụ.
    /// <summary>Chuẩn bị lịch điểm danh: giáo viên xem các tiết hôm nay, Staff/Principal dùng bộ lọc ngày và lớp.</summary>
    Task<ServiceResult<AttendanceIndexViewModel>> IndexAsync(LoginResponseDto user, AttendanceScheduleFilter filter, CancellationToken ct = default);
    /// <summary>Tải học sinh, kết quả đã lưu và đơn nghỉ để hiển thị bảng điểm danh của một tiết trong ngày.</summary>
    Task<ServiceResult<AttendanceSheetViewModel>> SheetAsync(LoginResponseDto user, long entryId, DateTime date, CancellationToken ct = default);
    /// <summary>Kiểm tra và lưu cả bảng điểm danh, nhật ký, thông báo phụ huynh trong cùng một giao dịch.</summary>
    Task<ServiceResult<AttendanceSaveResult>> SaveAsync(LoginResponseDto user, SaveAttendanceDto input, CancellationToken ct = default);
    /// <summary>Chuẩn bị kết quả và bộ lọc cho màn hình quản lý hoặc màn hình học sinh/phụ huynh.</summary>
    Task<ServiceResult<AttendanceHistoryViewModel>> HistoryAsync(LoginResponseDto user, AttendanceHistoryFilter filter, CancellationToken ct = default);
    /// <summary>Lấy kết quả điểm danh và các lần thay đổi để hiển thị nhật ký.</summary>
    Task<ServiceResult<AttendanceAuditViewModel>> AuditAsync(long recordId, CancellationToken ct = default);
}
