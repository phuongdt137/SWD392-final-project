using System.Linq.Expressions;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace HSMS.MVC.Repositories.Interfaces;

// Hợp đồng truy cập dữ liệu Attendance, kế thừa CRUD chung từ IGenericRepository.
// Service truyền scope đã tính theo tài khoản; Repository thực thi truy vấn EF Core.
public interface IAttendanceRepository : IGenericRepository<AttendanceRecord>
{
    Task<TimetableEntry?> GetLessonAsync(long id, CancellationToken ct);
    Task<List<TimetableEntry>> GetLessonsAsync(DateTime date, Expression<Func<TimetableEntry, bool>> scope, CancellationToken ct);
    Task<AttendanceScheduleResult> GetSchedulePageAsync(AttendanceScheduleFilter filter, Expression<Func<TimetableEntry, bool>> scope,
        DateTime localNow, bool locked, CancellationToken ct);
    // Lấy/tạo Employee cho STAFF trong transaction lưu do Service quản lý.
    Task<long> EnsureStaffEmployeeAsync(long userId, CancellationToken ct);
    Task<List<StudentClass>> GetRosterAsync(long classId, DateTime date, CancellationToken ct);
    // Đọc kết quả có tracking để cập nhật; các truy vấn chỉ hiển thị chủ yếu dùng AsNoTracking.
    Task<List<AttendanceRecord>> GetSheetAsync(long entryId, DateTime date, CancellationToken ct);
    Task<List<LeaveRequest>> GetLeavesAsync(long[] students, DateTime date, CancellationToken ct);
    // Trả transaction để Service quyết định commit sau khi đã kiểm tra và FlushAsync.
    Task<IDbContextTransaction> BeginAsync(CancellationToken ct);
    // Các hàm Add chỉ chuẩn bị entity; FlushAsync mới ghi các thay đổi đang theo dõi.
    void AddAudit(AttendanceAuditLog audit);
    void AddNotification(Notification notification);
    Task<List<Customer>> GetActiveParentsAsync(long[] studentIds, CancellationToken ct);
    Task<(List<StudentClass> Roster, List<AttendanceRecord> Records)> GetOverviewAsync(long[] entries, long[] classes, DateTime date, CancellationToken ct);
    Task<(List<AttendanceRecord> Records, int Total, AttendanceSummary Summary)> HistoryAsync(
        Expression<Func<AttendanceRecord, bool>> scope, AttendanceFilter filter, CancellationToken ct);
    // Customer ID của phụ huynh -> các con; Customer ID học sinh -> chính học sinh đó.
    Task<List<StudentChoice>> GetStudentChoicesAsync(long? customerId, bool isParent, CancellationToken ct);
    Task<AttendanceChoices> GetHistoryChoicesAsync(Expression<Func<AttendanceRecord, bool>> scope, CancellationToken ct);
    Task<bool> HasOverlappingEnrollmentAsync(long classId, DateTime date, long[] students, CancellationToken ct);
    Task<AttendanceRecord?> GetRecordAsync(long id, Expression<Func<AttendanceRecord, bool>> scope, CancellationToken ct);
    Task<List<AttendanceAuditLog>> GetAuditAsync(long recordId, CancellationToken ct);
    Task<List<TeachingAssignment>> GetStudentAssignmentsAsync(long studentId, CancellationToken ct);
    Task<(List<StudentClass> Memberships, List<TimetableEntry> Entries, List<AttendanceRecord> Records)> GetStudentScheduleAsync(
        long studentId, long assignmentId, long classId, CancellationToken ct);
    // Ghi SQL trong transaction hiện tại, không tự commit transaction.
    Task<int> FlushAsync(CancellationToken ct);
    // Bỏ thay đổi trong ChangeTracker sau khi luồng lưu thất bại.
    void DiscardChanges();
}
