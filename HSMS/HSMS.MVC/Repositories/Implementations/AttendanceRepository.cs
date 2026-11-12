using System.Linq.Expressions;
using HSMS.MVC.Common;
using HSMS.MVC.Data;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HSMS.MVC.Repositories.Implementations;

public class AttendanceRepository(AppDbContext db) : IAttendanceRepository
{
    private IQueryable<TimetableEntry> Lessons => db.TimetableEntries
        .Include(e => e.TimeSlot).Include(e => e.Room)
        .Include(e => e.TeachingAssignment.Subject)
        .Include(e => e.TeachingAssignment.Class)
        .Include(e => e.TeachingAssignment.Employee.User)
        .Include(e => e.TeachingAssignment.Semester.AcademicYear);

    private IQueryable<AttendanceRecord> Records => db.AttendanceRecords
        .Include(r => r.Customer.User)
        .Include(r => r.Customer.Parent!).ThenInclude(p => p.User.UserRoles).ThenInclude(r => r.Role)
        .Include(r => r.TimetableEntry.TimeSlot)
        .Include(r => r.TimetableEntry.TeachingAssignment.Class)
        .Include(r => r.TimetableEntry.TeachingAssignment.Subject);

    /// <summary>Lấy một trang các tiết đúng lịch, kèm tổng số tiết và số học sinh đã được điểm danh.</summary>
    public async Task<(List<AttendanceLessonDto> Items, int Total)> GetScheduleAsync(
        AttendanceScheduleFilter filter, long? employeeId, CancellationToken ct)
    {
        var date = filter.Date!.Value.Date;
        var weekday = SchoolTime.Weekday(date);
        var query = Lessons.AsNoTracking().Where(e => e.Status == "ACTIVE" &&
            e.TeachingAssignment.Status == "ACTIVE" && e.DayOfWeek == weekday &&
            (!employeeId.HasValue || e.TeachingAssignment.EmployeeId == employeeId) &&
            (!filter.ClassId.HasValue || e.TeachingAssignment.ClassId == filter.ClassId) &&
            (!e.EffectiveFrom.HasValue || e.EffectiveFrom <= date) &&
            (!e.EffectiveTo.HasValue || e.EffectiveTo >= date) &&
            e.TeachingAssignment.Semester.StartDate <= date && e.TeachingAssignment.Semester.EndDate >= date &&
            e.TeachingAssignment.Semester.AcademicYear.StartDate <= date && e.TeachingAssignment.Semester.AcademicYear.EndDate >= date);

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(e => e.TimeSlot.StartTime).ThenBy(e => e.TeachingAssignment.Class.ClassName)
            .ThenBy(e => e.Id).Skip((filter.Page - 1) * 20).Take(20)
            .Select(e => new AttendanceLessonDto(e,
                e.TeachingAssignment.Class.StudentClasses.Where(s => s.JoinedAt <= date &&
                    (!s.LeftAt.HasValue || date < s.LeftAt)).Select(s => s.CustomerId).Distinct().Count(),
                e.AttendanceRecords.Count(r => r.AttendanceDate == date))).ToListAsync(ct);
        return (items, total);
    }

    /// <summary>Lấy danh sách lớp theo phân công để hiển thị trong bộ lọc lịch điểm danh.</summary>
    public async Task<List<AttendanceChoice>> GetClassesAsync(long? employeeId, CancellationToken ct)
    {
        var classes = await db.TeachingAssignments.AsNoTracking()
            .Where(a => !employeeId.HasValue || a.EmployeeId == employeeId)
            .Select(a => new { Id = a.ClassId, Name = a.Class.ClassName }).Distinct().OrderBy(c => c.Name).ToListAsync(ct);
        return classes.Select(c => new AttendanceChoice(c.Id, c.Name)).ToList();
    }

    /// <summary>Lấy một tiết theo ID, kèm lớp, môn, giáo viên, khung giờ và học kỳ.</summary>
    public Task<TimetableEntry?> GetLessonAsync(long id, CancellationToken ct) =>
        Lessons.AsNoTracking().SingleOrDefaultAsync(e => e.Id == id, ct);

    /// <summary>Lấy học sinh thuộc lớp tại ngày học, kèm tài khoản và phụ huynh liên kết.</summary>
    public Task<List<Customer>> GetStudentsAsync(long classId, DateTime date, CancellationToken ct) => db.Customers
        .AsNoTracking().Include(c => c.User)
        .Include(c => c.Parent!).ThenInclude(p => p.User.UserRoles).ThenInclude(r => r.Role)
        .Where(c => c.StudentClasses.Any(s => s.ClassId == classId && s.JoinedAt <= date &&
            (!s.LeftAt.HasValue || date < s.LeftAt)))
        .OrderBy(c => c.User.FullName).ThenBy(c => c.Id).ToListAsync(ct);

    /// <summary>Lấy các kết quả đã lưu của đúng tiết và ngày để hiển thị hoặc cập nhật.</summary>
    public Task<List<AttendanceRecord>> GetSheetAsync(long entryId, DateTime date, CancellationToken ct) =>
        Records.Where(r => r.TimetableEntryId == entryId && r.AttendanceDate == date).ToListAsync(ct);

    /// <summary>Lấy đơn nghỉ đã duyệt của các học sinh, có khoảng ngày bao phủ ngày điểm danh.</summary>
    public Task<List<LeaveRequest>> GetLeavesAsync(long[] studentIds, DateTime date, CancellationToken ct) => db.LeaveRequests
        .AsNoTracking().Where(l => studentIds.Contains(l.StudentCustomerId) && l.Status == "APPROVED" &&
            l.StartDate <= date && l.EndDate >= date).OrderBy(l => l.Id).ToListAsync(ct);

    /// <summary>Lọc và phân trang kết quả điểm danh theo các điều kiện được truyền vào.</summary>
    public async Task<(List<AttendanceRecord> Items, int Total)> GetHistoryAsync(AttendanceHistoryFilter filter,
        Expression<Func<AttendanceRecord, bool>> scope, CancellationToken ct)
    {
        var query = Records.AsNoTracking().Where(scope);
        if (filter.SemesterId.HasValue) query = query.Where(r => r.TimetableEntry.TeachingAssignment.SemesterId == filter.SemesterId);
        if (filter.SubjectId.HasValue) query = query.Where(r => r.TimetableEntry.TeachingAssignment.SubjectId == filter.SubjectId);
        if (filter.Date.HasValue) query = query.Where(r => r.AttendanceDate == filter.Date);
        if (filter.From.HasValue) query = query.Where(r => r.AttendanceDate >= filter.From);
        if (filter.To.HasValue) query = query.Where(r => r.AttendanceDate <= filter.To);
        if (filter.ClassId.HasValue) query = query.Where(r => r.TimetableEntry.TeachingAssignment.ClassId == filter.ClassId);
        if (filter.StudentId.HasValue) query = query.Where(r => r.CustomerId == filter.StudentId);
        if (filter.Status != null) query = query.Where(r => r.Status == filter.Status);

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(r => r.AttendanceDate).ThenByDescending(r => r.Id)
            .Skip((filter.Page - 1) * 30).Take(30).ToListAsync(ct);
        return (items, total);
    }

    /// <summary>Lấy danh sách lớp và học sinh từ kết quả hiện có để tạo bộ lọc quản lý.</summary>
    public async Task<(List<AttendanceChoice> Classes, List<AttendanceChoice> Students)> GetHistoryChoicesAsync(
        Expression<Func<AttendanceRecord, bool>> scope, CancellationToken ct)
    {
        var query = db.AttendanceRecords.AsNoTracking().Where(scope);
        var classes = await query.Select(r => new { Id = r.TimetableEntry.TeachingAssignment.ClassId,
            Name = r.TimetableEntry.TeachingAssignment.Class.ClassName }).Distinct().OrderBy(c => c.Name).ToListAsync(ct);
        var students = await query.Select(r => new { Id = r.CustomerId, Name = r.Customer.User.FullName })
            .Distinct().OrderBy(c => c.Name).ToListAsync(ct);
        return (classes.Select(c => new AttendanceChoice(c.Id, c.Name)).ToList(),
            students.Select(c => new AttendanceChoice(c.Id, c.Name)).ToList());
    }

    /// <summary>Tìm một kết quả theo ID và điều kiện dữ liệu được truyền vào.</summary>
    public Task<AttendanceRecord?> GetRecordAsync(long id, Expression<Func<AttendanceRecord, bool>> scope, CancellationToken ct) =>
        Records.AsNoTracking().Where(scope).SingleOrDefaultAsync(r => r.Id == id, ct);

    /// <summary>Lấy các học sinh liên kết với phụ huynh để hiển thị danh sách chọn con.</summary>
    public async Task<List<AttendanceChoice>> GetChildrenAsync(long parentId, CancellationToken ct)
    {
        var children = await db.Customers.AsNoTracking().Where(c => c.ParentId == parentId)
            .OrderBy(c => c.User.FullName).ThenBy(c => c.Id)
            .Select(c => new { c.Id, Name = c.User.FullName, c.StudentCode }).ToListAsync(ct);
        return children.Select(c => new AttendanceChoice(c.Id,
            c.StudentCode == null ? c.Name : $"{c.Name} ({c.StudentCode})")).ToList();
    }

    /// <summary>Tạo truy vấn phân công thuộc quá trình học hoặc kết quả điểm danh của học sinh.</summary>
    private IQueryable<TeachingAssignment> StudentAssignments(long studentId) => db.TeachingAssignments.AsNoTracking()
        .Where(a => a.Class.StudentClasses.Any(s => s.CustomerId == studentId && s.JoinedAt <= a.Semester.EndDate &&
            (!s.LeftAt.HasValue || s.LeftAt > a.Semester.StartDate)) ||
            a.TimetableEntries.Any(e => e.AttendanceRecords.Any(r => r.CustomerId == studentId)));

    /// <summary>Lấy các học kỳ thuộc quá trình học của học sinh, kể cả khi chưa có kết quả điểm danh.</summary>
    public async Task<List<AttendanceSemesterChoice>> GetStudentSemestersAsync(long studentId, CancellationToken ct)
    {
        var semesters = await StudentAssignments(studentId).Select(a => new
        {
            Id = a.SemesterId,
            Name = a.Semester.SemesterName + " · " + a.Semester.AcademicYear.YearCode,
            a.Semester.StartDate, a.Semester.EndDate
        }).Distinct().OrderByDescending(s => s.StartDate).ThenByDescending(s => s.Id).ToListAsync(ct);
        return semesters.Select(s => new AttendanceSemesterChoice(s.Id, s.Name, s.StartDate, s.EndDate)).ToList();
    }

    /// <summary>Lấy các môn của học sinh trong học kỳ được chọn để hiển thị bộ lọc.</summary>
    public async Task<List<AttendanceChoice>> GetStudentSubjectsAsync(long studentId, long semesterId, CancellationToken ct)
    {
        var subjects = await StudentAssignments(studentId).Where(a => a.SemesterId == semesterId)
            .Select(a => new { Id = a.SubjectId, Name = a.Subject.SubjectName }).Distinct().OrderBy(s => s.Name).ToListAsync(ct);
        return subjects.Select(s => new AttendanceChoice(s.Id, s.Name)).ToList();
    }

    /// <summary>Lấy nhật ký của một kết quả, kèm người sửa, sắp xếp lần mới nhất trước.</summary>
    public Task<List<AttendanceAuditLog>> GetAuditAsync(long recordId, CancellationToken ct) => db.AttendanceAuditLogs
        .AsNoTracking().Include(a => a.ChangedByEmployee.User).Where(a => a.AttendanceRecordId == recordId)
        .OrderByDescending(a => a.ChangedAt).ThenByDescending(a => a.Id).ToListAsync(ct);

    /// <summary>Mở giao dịch để kết quả, nhật ký và cảnh báo được lưu hoặc hoàn tác cùng nhau.</summary>
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct) => db.Database.BeginTransactionAsync(ct);
    /// <summary>Đánh dấu kết quả mới để thêm khi lưu; chưa ghi ngay vào database.</summary>
    public void AddRecord(AttendanceRecord record) => db.AttendanceRecords.Add(record);
    /// <summary>Đánh dấu nhật ký mới để thêm cùng kết quả điểm danh khi lưu.</summary>
    public void AddAudit(AttendanceAuditLog audit) => db.AttendanceAuditLogs.Add(audit);
    /// <summary>Đánh dấu thông báo phụ huynh và hàng chờ email liên quan để lưu cùng giao dịch.</summary>
    public void AddNotification(Notification notification) => db.Notifications.Add(notification);
    /// <summary>Ghi các thay đổi EF đang theo dõi xuống database trong giao dịch hiện tại.</summary>
    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    /// <summary>Xóa trạng thái EF đang theo dõi khi xử lý thất bại; không thực hiện rollback database.</summary>
    public void ClearChanges() => db.ChangeTracker.Clear();
}
