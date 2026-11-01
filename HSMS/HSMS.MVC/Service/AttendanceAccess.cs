using System.Linq.Expressions;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.Models;

namespace HSMS.MVC.Service;

// Tập trung quy tắc phân quyền dùng chung cho đọc bảng, ghi/sửa và truy vấn lịch sử.
public static class AttendanceAccess
{
    public static bool CanEditAfterDeadline(LoginResponseDto user) => user.Has("STAFF");

    public static bool CanViewSheet(LoginResponseDto user, TimetableEntry entry) => user.SchoolReader ||
        (user.Has("TEACHER") && user.EmployeeId.HasValue &&
         (entry.TeachingAssignment.EmployeeId == user.EmployeeId || entry.TeachingAssignment.Class.HomeroomEmployeeId == user.EmployeeId));

    public static bool CanWrite(LoginResponseDto user, TimetableEntry entry) => user.Has("STAFF") || (user.EmployeeId.HasValue &&
        user.Has("TEACHER") && entry.TeachingAssignment.EmployeeId == user.EmployeeId);

    public static Expression<Func<TimetableEntry, bool>> Schedules(LoginResponseDto user) => e =>
        user.SchoolReader || (user.Has("TEACHER") && user.EmployeeId.HasValue &&
        (e.TeachingAssignment.EmployeeId == user.EmployeeId || e.TeachingAssignment.Class.HomeroomEmployeeId == user.EmployeeId));

    public static Expression<Func<AttendanceRecord, bool>> Records(LoginResponseDto user) => r =>
        user.SchoolReader || (user.Has("TEACHER") && user.EmployeeId.HasValue &&
        (r.TimetableEntry.TeachingAssignment.EmployeeId == user.EmployeeId || r.TimetableEntry.TeachingAssignment.Class.HomeroomEmployeeId == user.EmployeeId)) ||
        (user.Has("STUDENT") && r.CustomerId == user.CustomerId) ||
        (user.Has("PARENT") && user.CustomerId.HasValue && r.Customer.ParentId == user.CustomerId);
}

