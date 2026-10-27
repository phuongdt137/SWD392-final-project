using System.Data;
using System.Linq.Expressions;
using HSMS.MVC.Common;
using HSMS.MVC.Data;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace HSMS.MVC.Repositories.Implementations;

public sealed class AttendanceRepository : GenericRepository<AttendanceRecord>, IAttendanceRepository
{
    public AttendanceRepository(AppDbContext db) : base(db) { }
    private IQueryable<TimetableEntry> Lessons => _context.TimetableEntries
        .Include(x => x.TeachingAssignment).ThenInclude(x => x.Class).ThenInclude(x => x.AcademicYear)
        .Include(x => x.TeachingAssignment).ThenInclude(x => x.Class).ThenInclude(x => x.GradeLevel)
        .Include(x => x.TeachingAssignment).ThenInclude(x => x.Semester)
        .Include(x => x.TeachingAssignment).ThenInclude(x => x.Subject)
        .Include(x => x.TeachingAssignment).ThenInclude(x => x.Employee)
        .Include(x => x.TimeSlot).Include(x => x.Room);
    private IQueryable<AttendanceRecord> Details => _context.AttendanceRecords
        .Include(x => x.Customer)
        .Include(x => x.TimetableEntry).ThenInclude(x => x.TimeSlot)
        .Include(x => x.TimetableEntry).ThenInclude(x => x.TeachingAssignment).ThenInclude(x => x.Class)
        .Include(x => x.TimetableEntry).ThenInclude(x => x.TeachingAssignment).ThenInclude(x => x.Subject);
    public Task<TimetableEntry?> GetLessonAsync(long id, CancellationToken ct) => Lessons.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<List<TimetableEntry>> GetLessonsAsync(DateTime date, Expression<Func<TimetableEntry, bool>> scope, CancellationToken ct)
        => DailyLessons(date, scope).OrderBy(x => x.TimeSlot.StartTime).ThenBy(x => x.TeachingAssignment.Class.ClassName).ToListAsync(ct);
    private IQueryable<TimetableEntry> DailyLessons(DateTime date, Expression<Func<TimetableEntry, bool>> scope)
    {
        var day = SchoolTime.Weekday(date);
        return Lessons.AsNoTracking().Where(scope).Where(x => x.DayOfWeek == day &&
            (!x.EffectiveFrom.HasValue || x.EffectiveFrom <= date) && (!x.EffectiveTo.HasValue || x.EffectiveTo >= date) &&
            x.TeachingAssignment.Semester.StartDate <= date && x.TeachingAssignment.Semester.EndDate >= date &&
            x.TeachingAssignment.Class.AcademicYear.StartDate <= date && x.TeachingAssignment.Class.AcademicYear.EndDate >= date &&
            ((x.Status == "ACTIVE" && x.TeachingAssignment.Status == "ACTIVE" && x.TeachingAssignment.Class.Status == "ACTIVE") ||
             x.AttendanceRecords.Any(r => r.AttendanceDate == date)));
    }
    public async Task<AttendanceScheduleResult> GetSchedulePageAsync(AttendanceScheduleFilter filter,
        Expression<Func<TimetableEntry, bool>> scope, DateTime localNow, bool locked, CancellationToken ct)
    {
        var date = filter.Date!.Value.Date;
        var daily = DailyLessons(date, scope);
        var options = await daily.Select(e => new
        {
            GradeId = e.TeachingAssignment.Class.GradeLevelId, GradeName = e.TeachingAssignment.Class.GradeLevel.LevelName,
            ClassId = e.TeachingAssignment.ClassId, ClassName = e.TeachingAssignment.Class.ClassName,
            SubjectId = e.TeachingAssignment.SubjectId, SubjectName = e.TeachingAssignment.Subject.SubjectName,
            TeacherId = e.TeachingAssignment.EmployeeId, TeacherName = e.TeachingAssignment.Employee.FullName
        }).Distinct().ToListAsync(ct);
        var choices = new AttendanceScheduleChoices(
            options.Select(x => new StudentChoice(x.GradeId, x.GradeName)).DistinctBy(x => x.Id).OrderBy(x => x.Name).ToList(),
            options.Where(x => !filter.GradeLevelId.HasValue || x.GradeId == filter.GradeLevelId).Select(x => new StudentChoice(x.ClassId, x.ClassName)).DistinctBy(x => x.Id).OrderBy(x => x.Name).ToList(),
            options.Select(x => new StudentChoice(x.SubjectId, x.SubjectName)).DistinctBy(x => x.Id).OrderBy(x => x.Name).ToList(),
            options.Select(x => new StudentChoice(x.TeacherId, x.TeacherName)).DistinctBy(x => x.Id).OrderBy(x => x.Name).ToList());
        if (filter.GradeLevelId.HasValue) daily = daily.Where(e => e.TeachingAssignment.Class.GradeLevelId == filter.GradeLevelId);
        if (filter.ClassId.HasValue) daily = daily.Where(e => e.TeachingAssignment.ClassId == filter.ClassId);
        if (filter.SubjectId.HasValue) daily = daily.Where(e => e.TeachingAssignment.SubjectId == filter.SubjectId);
        if (filter.TeacherId.HasValue) daily = daily.Where(e => e.TeachingAssignment.EmployeeId == filter.TeacherId);
        if (!string.IsNullOrEmpty(filter.Query)) daily = daily.Where(e => e.TeachingAssignment.Class.ClassName.Contains(filter.Query) ||
            e.TeachingAssignment.Subject.SubjectName.Contains(filter.Query) || e.TeachingAssignment.Employee.FullName.Contains(filter.Query));

        var rows = daily.Select(e => new
        {
            Entry = e,
            Students = e.TeachingAssignment.Class.StudentClasses.Where(m => m.JoinedAt <= date && (!m.LeftAt.HasValue || date < m.LeftAt))
                .Select(m => m.CustomerId).Distinct().Count(),
            Recorded = e.AttendanceRecords.Count(r => r.AttendanceDate == date && r.Customer.StudentClasses.Any(m =>
                m.ClassId == e.TeachingAssignment.ClassId && m.JoinedAt <= date && (!m.LeftAt.HasValue || date < m.LeftAt)))
        });
        var past = date < localNow.Date;
        var future = date > localNow.Date;
        var today = date == localNow.Date;
        var now = localNow.TimeOfDay;
        rows = filter.Status switch
        {
            "LOCKED" => rows.Where(x => locked),
            "UPCOMING" => rows.Where(x => !locked && (future || (today && x.Entry.TimeSlot.StartTime > now))),
            "COMPLETED" => rows.Where(x => !locked && (past || (today && x.Entry.TimeSlot.StartTime <= now)) && x.Students > 0 && x.Recorded >= x.Students),
            "PENDING" => rows.Where(x => !locked && (past || (today && x.Entry.TimeSlot.StartTime <= now)) && (x.Students == 0 || x.Recorded < x.Students)),
            _ => rows
        };
        var total = await rows.CountAsync(ct);
        var classes = await rows.Select(x => x.Entry.TeachingAssignment.ClassId).Distinct().CountAsync(ct);
        var completed = await rows.CountAsync(x => x.Students > 0 && x.Recorded >= x.Students, ct);
        var pending = await rows.CountAsync(x => (past || (today && x.Entry.TimeSlot.StartTime <= now)) && (x.Students == 0 || x.Recorded < x.Students), ct);
        var pages = Math.Max(1, (int)Math.Ceiling(total / (double)AttendanceScheduleFilter.PageSize));
        var page = Math.Clamp(filter.Page, 1, pages);
        var items = await rows.OrderBy(x => x.Entry.TeachingAssignment.Class.ClassName).ThenBy(x => x.Entry.TimeSlot.StartTime).ThenBy(x => x.Entry.Id)
            .Skip((page - 1) * AttendanceScheduleFilter.PageSize).Take(AttendanceScheduleFilter.PageSize).ToListAsync(ct);
        return new(items.Select(x => new AttendanceScheduleRow(x.Entry, x.Students, x.Recorded)).ToList(), total, classes, completed, pending, page, choices);
    }
    public Task<List<StudentClass>> GetRosterAsync(long classId, DateTime date, CancellationToken ct) => _context.StudentClasses.AsNoTracking()
        .Include(x => x.Customer)
        .Where(x => x.ClassId == classId && x.JoinedAt <= date && (!x.LeftAt.HasValue || date < x.LeftAt))
        .OrderBy(x => x.Customer.FullName).ToListAsync(ct);
    public Task<List<AttendanceRecord>> GetSheetAsync(long entryId, DateTime date, CancellationToken ct) => Details
        .Where(x => x.TimetableEntryId == entryId && x.AttendanceDate == date).ToListAsync(ct);
    public Task<List<LeaveRequest>> GetLeavesAsync(long[] students, DateTime date, CancellationToken ct) => _context.LeaveRequests
        .Where(x => students.Contains(x.StudentCustomerId) && x.Status == "APPROVED" && x.StartDate <= date && date <= x.EndDate)
        .OrderBy(x => x.Id).ToListAsync(ct);
    public Task<IDbContextTransaction> BeginAsync(CancellationToken ct) => _context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
    public void AddAudit(AttendanceAuditLog audit) => _context.AttendanceAuditLogs.Add(audit);
    public void AddNotification(Notification notification) => _context.Notifications.Add(notification);
    public Task<List<Customer>> GetActiveParentsAsync(long[] studentIds, CancellationToken ct) => _context.Customers.AsNoTracking()
        .Where(x => x.Children.Any(c => studentIds.Contains(c.Id)) && x.IsActive &&
            x.CustomerRoles.Any(r => r.IsActive && r.Role.IsActive && r.Role.RoleCode == "PARENT"))
        .Include(x => x.Children).ToListAsync(ct);
    public async Task<(List<StudentClass> Roster, List<AttendanceRecord> Records)> GetOverviewAsync(long[] entries, long[] classes, DateTime date, CancellationToken ct)
    {
        var roster = await _context.StudentClasses.AsNoTracking().Where(x => classes.Contains(x.ClassId) && x.JoinedAt <= date && (!x.LeftAt.HasValue || date < x.LeftAt)).ToListAsync(ct);
        var records = await _context.AttendanceRecords.AsNoTracking().Where(x => entries.Contains(x.TimetableEntryId) && x.AttendanceDate == date).ToListAsync(ct);
        return (roster, records);
    }
    public async Task<(List<AttendanceRecord> Records, int Total, AttendanceSummary Summary)> HistoryAsync(
        Expression<Func<AttendanceRecord, bool>> scope, AttendanceFilter f, CancellationToken ct)
    {
        var q = Details.AsNoTracking().Where(scope);
        if (f.From.HasValue) q = q.Where(x => x.AttendanceDate >= f.From.Value.Date);
        if (f.To.HasValue) q = q.Where(x => x.AttendanceDate <= f.To.Value.Date);
        if (f.StudentId.HasValue) q = q.Where(x => x.CustomerId == f.StudentId);
        if (f.ClassId.HasValue) q = q.Where(x => x.TimetableEntry.TeachingAssignment.ClassId == f.ClassId);
        if (f.SubjectId.HasValue) q = q.Where(x => x.TimetableEntry.TeachingAssignment.SubjectId == f.SubjectId);
        if (!string.IsNullOrEmpty(f.Status)) q = q.Where(x => x.Status == f.Status);
        var counts = await q.GroupBy(x => x.Status).Select(g => new { Status = g.Key, Count = g.Count() }).ToListAsync(ct);
        int Count(string s) => counts.FirstOrDefault(x => x.Status == s)?.Count ?? 0;
        var summary = new AttendanceSummary(Count("PRESENT"), Count("ABSENT"), Count("LATE"), Count("EXCUSED"));
        f.Page = Math.Clamp(f.Page, 1, Math.Max(1, (int)Math.Ceiling(summary.Recorded / 30d)));
        var rows = await q.OrderByDescending(x => x.AttendanceDate).ThenBy(x => x.TimetableEntry.TimeSlot.StartTime).ThenBy(x => x.Id)
            .Skip((f.Page - 1) * 30).Take(30).ToListAsync(ct);
        return (rows, summary.Recorded, summary);
    }
    public Task<List<StudentChoice>> GetStudentChoicesAsync(long? customerId, bool isParent, CancellationToken ct) => _context.Customers.AsNoTracking()
        .Where(x => isParent ? customerId.HasValue && x.ParentId == customerId : x.Id == customerId)
        .OrderBy(x => x.FullName).ThenBy(x => x.Id)
        .Select(x => new StudentChoice(x.Id, x.FullName)).ToListAsync(ct);
    public async Task<AttendanceChoices> GetHistoryChoicesAsync(Expression<Func<AttendanceRecord, bool>> scope, CancellationToken ct)
    {
        var q = _context.AttendanceRecords.AsNoTracking().Where(scope);
        var students = await q.Select(x => new { x.CustomerId, x.Customer.FullName }).Distinct().OrderBy(x => x.FullName).ToListAsync(ct);
        var classes = await q.Select(x => new { x.TimetableEntry.TeachingAssignment.ClassId, x.TimetableEntry.TeachingAssignment.Class.ClassName }).Distinct().OrderBy(x => x.ClassName).ToListAsync(ct);
        var subjects = await q.Select(x => new { x.TimetableEntry.TeachingAssignment.SubjectId, x.TimetableEntry.TeachingAssignment.Subject.SubjectName }).Distinct().OrderBy(x => x.SubjectName).ToListAsync(ct);
        return new(students.Select(x => new StudentChoice(x.CustomerId, x.FullName)).ToList(),
            classes.Select(x => new StudentChoice(x.ClassId, x.ClassName)).ToList(), subjects.Select(x => new StudentChoice(x.SubjectId, x.SubjectName)).ToList());
    }
    public Task<bool> HasOverlappingEnrollmentAsync(long classId, DateTime date, long[] students, CancellationToken ct) =>
        _context.StudentClasses.Where(x => students.Contains(x.CustomerId) && x.JoinedAt <= date && (!x.LeftAt.HasValue || date < x.LeftAt) &&
            x.Class.AcademicYearId == _context.Classes.Where(c => c.Id == classId).Select(c => c.AcademicYearId).First())
            .GroupBy(x => x.CustomerId).AnyAsync(g => g.Count() > 1, ct);
    public Task<AttendanceRecord?> GetRecordAsync(long id, Expression<Func<AttendanceRecord, bool>> scope, CancellationToken ct) => Details.AsNoTracking().Where(scope).SingleOrDefaultAsync(x => x.Id == id, ct);
    public Task<List<AttendanceAuditLog>> GetAuditAsync(long recordId, CancellationToken ct) => _context.AttendanceAuditLogs.AsNoTracking()
        .Include(x => x.ChangedByEmployee).Where(x => x.AttendanceRecordId == recordId)
        .OrderByDescending(x => x.ChangedAt).ThenByDescending(x => x.Id).ToListAsync(ct);
    public Task<int> FlushAsync(CancellationToken ct) => _context.SaveChangesAsync(ct);
    public void DiscardChanges() => _context.ChangeTracker.Clear();
    public Task<List<TeachingAssignment>> GetStudentAssignmentsAsync(long studentId, CancellationToken ct) => _context.TeachingAssignments.AsNoTracking()
        .Include(x => x.Semester).ThenInclude(x => x.AcademicYear).Include(x => x.Subject)
        .Include(x => x.Class).ThenInclude(x => x.AcademicYear)
        .Include(x => x.Employee)
        .Where(x =>
            (x.Status == "ACTIVE" && x.Class.StudentClasses.Any(s => s.CustomerId == studentId &&
                s.JoinedAt <= x.Semester.EndDate && (!s.LeftAt.HasValue || s.LeftAt > x.Semester.StartDate)) &&
                x.TimetableEntries.Any(t => t.Status == "ACTIVE")) ||
            x.TimetableEntries.Any(t => t.AttendanceRecords.Any(r => r.CustomerId == studentId)))
        .OrderByDescending(x => x.Semester.StartDate).ThenBy(x => x.Subject.SubjectName).ThenBy(x => x.Class.ClassName).ToListAsync(ct);
    public async Task<(List<StudentClass> Memberships, List<TimetableEntry> Entries, List<AttendanceRecord> Records)> GetStudentScheduleAsync(
        long studentId, long assignmentId, long classId, CancellationToken ct)
    {
        var memberships = await _context.StudentClasses.AsNoTracking().Where(x => x.CustomerId == studentId && x.ClassId == classId).ToListAsync(ct);
        var entries = await Lessons.AsNoTracking().Where(x => x.TeachingAssignmentId == assignmentId).ToListAsync(ct);
        var records = await _context.AttendanceRecords.AsNoTracking()
            .Where(x => x.CustomerId == studentId && x.TimetableEntry.TeachingAssignmentId == assignmentId).ToListAsync(ct);
        return (memberships, entries, records);
    }
}
