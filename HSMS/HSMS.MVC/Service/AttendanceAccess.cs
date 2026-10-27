using System.Linq.Expressions;
using HSMS.MVC.Models;

namespace HSMS.MVC.Service;

// Tập trung quy tắc phân quyền dùng chung cho đọc bảng, ghi/sửa và truy vấn lịch sử.
public static class AttendanceAccess
{
    // STAFF hoặc ADMIN có Employee được sửa sau 00h; quyền này không cho điểm danh trước giờ.
    public static bool CanEditAfterDeadline(CurrentUser user) => user.Has("STAFF") || (user.Has("ADMIN") && user.EmployeeId.HasValue);
    // Nhóm quản lý xem toàn trường; giáo viên xem tiết được phân công hoặc lớp chủ nhiệm.
    public static bool CanViewSheet(CurrentUser user, TimetableEntry entry) => user.SchoolReader ||
        (user.Has("TEACHER") && user.EmployeeId.HasValue &&
         (entry.TeachingAssignment.EmployeeId == user.EmployeeId || entry.TeachingAssignment.Class.HomeroomEmployeeId == user.EmployeeId));
    // STAFF được ghi toàn trường; ADMIN cần Employee; giáo viên chỉ ghi tiết mình được phân công.
    public static bool CanWrite(CurrentUser user, TimetableEntry entry) => user.Has("STAFF") || (user.EmployeeId.HasValue &&
        (user.Has("ADMIN") || (user.Has("TEACHER") && entry.TeachingAssignment.EmployeeId == user.EmployeeId)));
    // Expression được Repository đưa vào WHERE SQL, giới hạn lịch trước khi lọc/phân trang.
    public static Expression<Func<TimetableEntry, bool>> Schedules(CurrentUser user) => e =>
        user.SchoolReader || (user.Has("TEACHER") && user.EmployeeId.HasValue &&
        (e.TeachingAssignment.EmployeeId == user.EmployeeId || e.TeachingAssignment.Class.HomeroomEmployeeId == user.EmployeeId));
    // Giới hạn kết quả: quản lý toàn trường, giáo viên theo phạm vi, học sinh bản thân, phụ huynh các con.
    public static Expression<Func<AttendanceRecord, bool>> Records(CurrentUser user) => r =>
        user.SchoolReader || (user.Has("TEACHER") && user.EmployeeId.HasValue &&
        (r.TimetableEntry.TeachingAssignment.EmployeeId == user.EmployeeId || r.TimetableEntry.TeachingAssignment.Class.HomeroomEmployeeId == user.EmployeeId)) ||
        (user.Has("STUDENT") && r.CustomerId == user.CustomerId) ||
        (user.Has("PARENT") && user.CustomerId.HasValue && r.Customer.ParentId == user.CustomerId);
}
