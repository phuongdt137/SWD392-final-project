using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.ViewModels.Attendance;

namespace HSMS.MVC.Service;

// Hợp đồng nghiệp vụ Controller sử dụng; ServiceResult mang dữ liệu hoặc lỗi kèm HTTP status.
public interface IAttendanceService
{
    // Lấy lịch theo ngày cho nơi không truyền bộ lọc đầy đủ.
    Task<ServiceResult<AttendanceIndex>> IndexAsync(DateTime date, CancellationToken ct = default);
    // Lấy lịch có lọc, phân trang và thống kê theo phạm vi tài khoản.
    Task<ServiceResult<AttendanceIndex>> IndexAsync(AttendanceScheduleFilter filter, CancellationToken ct = default);
    // Đọc bảng gồm học sinh, kết quả, đơn nghỉ và quyền sửa; không ghi dữ liệu.
    Task<ServiceResult<AttendanceSheet>> SheetAsync(long entryId, DateTime date, CancellationToken ct = default);
    // Kiểm tra và lưu kết quả/audit/thông báo trong cùng transaction.
    Task<ServiceResult<SaveAttendanceResult>> SaveAsync(SaveAttendanceDto input, CancellationToken ct = default);
    // Tra cứu kết quả theo phạm vi được phép, bộ lọc và phân trang.
    Task<ServiceResult<AttendanceHistory>> HistoryAsync(AttendanceFilter filter, CancellationToken ct = default);
    // Lịch theo buổi cho bản thân học sinh hoặc con được liên kết của phụ huynh.
    Task<ServiceResult<StudentAttendanceHistory>> StudentHistoryAsync(AttendanceFilter filter, CancellationToken ct = default);
    // Đọc các lần thay đổi của một kết quả sau kiểm tra quyền.
    Task<ServiceResult<AttendanceAudit>> AuditAsync(long recordId, CancellationToken ct = default);
    // Tổng hợp nhanh kết quả và số tiết còn chờ điểm danh trong ngày.
    Task<AttendanceSummary> DashboardAsync(CancellationToken ct = default);
}
