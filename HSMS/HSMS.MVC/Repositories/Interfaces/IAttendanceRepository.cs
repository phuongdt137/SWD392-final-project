using System.Linq.Expressions;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace HSMS.MVC.Repositories.Interfaces;

public interface IAttendanceRepository
{
    /// <summary>Lấy một trang các tiết đúng lịch, kèm tổng số tiết và số học sinh đã được điểm danh.</summary>
    Task<(List<AttendanceLessonDto> Items, int Total)> GetScheduleAsync(AttendanceScheduleFilter filter, long? employeeId, CancellationToken ct);
    /// <summary>Lấy danh sách lớp theo phân công để hiển thị trong bộ lọc lịch điểm danh.</summary>
    Task<List<AttendanceChoice>> GetClassesAsync(long? employeeId, CancellationToken ct);
    /// <summary>Lấy một tiết theo ID, kèm lớp, môn, giáo viên, khung giờ và học kỳ.</summary>
    Task<TimetableEntry?> GetLessonAsync(long id, CancellationToken ct);
    /// <summary>Lấy học sinh thuộc lớp tại ngày học, kèm tài khoản và phụ huynh liên kết.</summary>
    Task<List<Customer>> GetStudentsAsync(long classId, DateTime date, CancellationToken ct);
    /// <summary>Lấy các kết quả đã lưu của đúng tiết và ngày để hiển thị hoặc cập nhật.</summary>
    Task<List<AttendanceRecord>> GetSheetAsync(long entryId, DateTime date, CancellationToken ct);
    /// <summary>Lấy đơn nghỉ đã duyệt của các học sinh, có khoảng ngày bao phủ ngày điểm danh.</summary>
    Task<List<LeaveRequest>> GetLeavesAsync(long[] studentIds, DateTime date, CancellationToken ct);
    /// <summary>Lọc và phân trang kết quả điểm danh theo các điều kiện được truyền vào.</summary>
    Task<(List<AttendanceRecord> Items, int Total)> GetHistoryAsync(AttendanceHistoryFilter filter, Expression<Func<AttendanceRecord, bool>> scope, CancellationToken ct);
    /// <summary>Lấy danh sách lớp và học sinh từ kết quả hiện có để tạo bộ lọc quản lý.</summary>
    Task<(List<AttendanceChoice> Classes, List<AttendanceChoice> Students)> GetHistoryChoicesAsync(Expression<Func<AttendanceRecord, bool>> scope, CancellationToken ct);
    /// <summary>Lấy các học sinh liên kết với phụ huynh để hiển thị danh sách chọn con.</summary>
    Task<List<AttendanceChoice>> GetChildrenAsync(long parentId, CancellationToken ct);
    /// <summary>Lấy các học kỳ thuộc quá trình học của học sinh, kể cả khi chưa có kết quả điểm danh.</summary>
    Task<List<AttendanceSemesterChoice>> GetStudentSemestersAsync(long studentId, CancellationToken ct);
    /// <summary>Lấy các môn của học sinh trong học kỳ được chọn để hiển thị bộ lọc.</summary>
    Task<List<AttendanceChoice>> GetStudentSubjectsAsync(long studentId, long semesterId, CancellationToken ct);
    /// <summary>Tìm một kết quả theo ID và điều kiện dữ liệu được truyền vào.</summary>
    Task<AttendanceRecord?> GetRecordAsync(long id, Expression<Func<AttendanceRecord, bool>> scope, CancellationToken ct);
    /// <summary>Lấy nhật ký của một kết quả, kèm người sửa, sắp xếp lần mới nhất trước.</summary>
    Task<List<AttendanceAuditLog>> GetAuditAsync(long recordId, CancellationToken ct);
    /// <summary>Mở giao dịch để kết quả, nhật ký và cảnh báo được lưu hoặc hoàn tác cùng nhau.</summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);
    /// <summary>Đánh dấu kết quả mới để thêm khi lưu; chưa ghi ngay vào database.</summary>
    void AddRecord(AttendanceRecord record);
    /// <summary>Đánh dấu nhật ký mới để thêm cùng kết quả điểm danh khi lưu.</summary>
    void AddAudit(AttendanceAuditLog audit);
    /// <summary>Đánh dấu thông báo phụ huynh và hàng chờ email liên quan để lưu cùng giao dịch.</summary>
    void AddNotification(Notification notification);
    /// <summary>Ghi các thay đổi EF đang theo dõi xuống database trong giao dịch hiện tại.</summary>
    Task SaveAsync(CancellationToken ct);
    /// <summary>Xóa trạng thái EF đang theo dõi khi xử lý thất bại; không thực hiện rollback database.</summary>
    void ClearChanges();
}
