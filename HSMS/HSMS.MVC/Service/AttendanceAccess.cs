using System.Linq.Expressions;
using HSMS.MVC.Models;

namespace HSMS.MVC.Service;

public static class AttendanceAccess
{
    public static bool CanEditAfterDeadline(CurrentUser user) => user.Has("STAFF") || (user.Has("ADMIN") && user.EmployeeId.HasValue);
    public static bool CanViewSheet(CurrentUser user, TimetableEntry entry) => user.SchoolReader ||
        (user.Has("TEACHER") && user.EmployeeId.HasValue &&
         (entry.TeachingAssignment.EmployeeId == user.EmployeeId || entry.TeachingAssignment.Class.HomeroomEmployeeId == user.EmployeeId));
    public static bool CanWrite(CurrentUser user, TimetableEntry entry) => user.Has("STAFF") || (user.EmployeeId.HasValue &&
        (user.Has("ADMIN") || (user.Has("TEACHER") && entry.TeachingAssignment.EmployeeId == user.EmployeeId)));
    public static Expression<Func<TimetableEntry, bool>> Schedules(CurrentUser user) => e =>
        user.SchoolReader || (user.Has("TEACHER") && user.EmployeeId.HasValue &&
        (e.TeachingAssignment.EmployeeId == user.EmployeeId || e.TeachingAssignment.Class.HomeroomEmployeeId == user.EmployeeId));
    public static Expression<Func<AttendanceRecord, bool>> Records(CurrentUser user) => r =>
        user.SchoolReader || (user.Has("TEACHER") && user.EmployeeId.HasValue &&
        (r.TimetableEntry.TeachingAssignment.EmployeeId == user.EmployeeId || r.TimetableEntry.TeachingAssignment.Class.HomeroomEmployeeId == user.EmployeeId)) ||
        (user.Has("STUDENT") && r.CustomerId == user.CustomerId) ||
        (user.Has("PARENT") && user.CustomerId.HasValue && r.Customer.ParentId == user.CustomerId);
}
