using System.Linq.Expressions;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace HSMS.Attendance.Checks;

// Kho thử nghiệm chỉ ở bộ nhớ, không kết nối SQL Server hoặc gửi email.
internal class FakeRepository : IAttendanceRepository
{
    public LoginResponseDto? User { get; set; }
    public TimetableEntry Entry { get; set; } = null!;
    public List<Customer> Students { get; set; } = [];
    public List<AttendanceRecord> Records { get; } = [];
    public List<LeaveRequest> Leaves { get; } = [];
    public List<AttendanceAuditLog> Audits { get; } = [];
    public List<Notification> Notifications { get; } = [];
    public Dictionary<long, List<AttendanceSemesterChoice>> Semesters { get; } = [];
    public Dictionary<(long StudentId, long SemesterId), List<AttendanceChoice>> Subjects { get; } = [];
    public Action? OnSave { get; set; }
    public bool FailSave { get; set; }
    public int Commits { get; private set; }
    private readonly List<AttendanceRecord> _working = [];
    private readonly List<AttendanceAuditLog> _audits = [];
    private readonly List<Notification> _notifications = [];

    public Task<(List<AttendanceLessonDto> Items, int Total)> GetScheduleAsync(AttendanceScheduleFilter filter, long? employeeId, CancellationToken ct)
    {
        List<AttendanceLessonDto> items = employeeId.HasValue && employeeId != Entry.TeachingAssignment.EmployeeId
            ? [] : [new(Entry, Students.Count, Records.Count)];
        return Task.FromResult((items, items.Count));
    }
    public Task<List<AttendanceChoice>> GetClassesAsync(long? employeeId, CancellationToken ct) => Task.FromResult(new List<AttendanceChoice> { new(100, "10A1") });
    public Task<TimetableEntry?> GetLessonAsync(long id, CancellationToken ct) => Task.FromResult(id == Entry.Id ? Entry : null);
    public Task<List<Customer>> GetStudentsAsync(long classId, DateTime date, CancellationToken ct) => Task.FromResult(Students.ToList());
    public Task<List<AttendanceRecord>> GetSheetAsync(long entryId, DateTime date, CancellationToken ct)
    {
        _working.Clear();
        _working.AddRange(Records.Where(r => r.TimetableEntryId == entryId && r.AttendanceDate == date).Select(Fixture.Clone));
        return Task.FromResult(_working.ToList());
    }
    public Task<List<LeaveRequest>> GetLeavesAsync(long[] studentIds, DateTime date, CancellationToken ct) =>
        Task.FromResult(Leaves.Where(l => studentIds.Contains(l.StudentCustomerId) && l.Status == "APPROVED" && l.StartDate <= date && l.EndDate >= date).ToList());
    public Task<(List<AttendanceRecord> Items, int Total)> GetHistoryAsync(AttendanceHistoryFilter filter, Expression<Func<AttendanceRecord, bool>> scope, CancellationToken ct)
    {
        var rows = Records.Where(scope.Compile());
        if (filter.SemesterId.HasValue) rows = rows.Where(r => r.TimetableEntry.TeachingAssignment.SemesterId == filter.SemesterId);
        if (filter.SubjectId.HasValue) rows = rows.Where(r => r.TimetableEntry.TeachingAssignment.SubjectId == filter.SubjectId);
        if (filter.Date.HasValue) rows = rows.Where(r => r.AttendanceDate == filter.Date);
        if (filter.StudentId.HasValue) rows = rows.Where(r => r.CustomerId == filter.StudentId);
        if (filter.ClassId.HasValue) rows = rows.Where(r => r.TimetableEntry.TeachingAssignment.ClassId == filter.ClassId);
        if (filter.From.HasValue) rows = rows.Where(r => r.AttendanceDate >= filter.From);
        if (filter.To.HasValue) rows = rows.Where(r => r.AttendanceDate <= filter.To);
        if (filter.Status != null) rows = rows.Where(r => r.Status == filter.Status);
        var result = rows.ToList();
        return Task.FromResult((result.Skip((filter.Page - 1) * 30).Take(30).ToList(), result.Count));
    }
    public Task<(List<AttendanceChoice> Classes, List<AttendanceChoice> Students)> GetHistoryChoicesAsync(Expression<Func<AttendanceRecord, bool>> scope, CancellationToken ct)
    {
        var visible = Records.Where(scope.Compile()).ToList();
        return Task.FromResult((visible.Select(r => new AttendanceChoice(r.TimetableEntry.TeachingAssignment.ClassId, "10A1")).Distinct().ToList(),
            visible.Select(r => new AttendanceChoice(r.CustomerId, r.Customer.FullName)).Distinct().ToList()));
    }
    public Task<AttendanceRecord?> GetRecordAsync(long id, Expression<Func<AttendanceRecord, bool>> scope, CancellationToken ct) =>
        Task.FromResult(Records.Where(scope.Compile()).SingleOrDefault(r => r.Id == id));
    public Task<List<AttendanceChoice>> GetChildrenAsync(long parentId, CancellationToken ct) => Task.FromResult(
        Students.Where(s => s.ParentId == parentId).Select(s => new AttendanceChoice(s.Id, s.FullName)).ToList());
    public Task<List<AttendanceSemesterChoice>> GetStudentSemestersAsync(long studentId, CancellationToken ct) =>
        Task.FromResult(Semesters.GetValueOrDefault(studentId, []).ToList());
    public Task<List<AttendanceChoice>> GetStudentSubjectsAsync(long studentId, long semesterId, CancellationToken ct) =>
        Task.FromResult(Subjects.GetValueOrDefault((studentId, semesterId), []).ToList());
    public Task<List<AttendanceAuditLog>> GetAuditAsync(long recordId, CancellationToken ct) =>
        Task.FromResult(Audits.Where(a => a.AttendanceRecordId == recordId).ToList());
    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct) => Task.FromResult<IDbContextTransaction>(new FakeTransaction(this));
    public void AddRecord(AttendanceRecord record)
    {
        Fixture.Set(record, "Id", 1000L + _working.Count);
        Fixture.Set(record, "Customer", Students.Single(s => s.Id == record.CustomerId));
        Fixture.Set(record, "TimetableEntry", Entry);
        _working.Add(record);
    }
    public void AddAudit(AttendanceAuditLog audit)
    {
        var employee = new Employee(1, "EMP01");
        Fixture.Set(employee, "User", new User("employee", "employee@example.test", "unused", "Nhân viên mẫu"));
        Fixture.Set(audit, "ChangedByEmployee", employee);
        _audits.Add(audit);
    }
    public void AddNotification(Notification notification) => _notifications.Add(notification);
    public Task SaveAsync(CancellationToken ct)
    {
        OnSave?.Invoke();
        if (FailSave) throw new InvalidOperationException("Simulated database failure");
        return Task.CompletedTask;
    }
    public void ClearChanges() { _working.Clear(); _audits.Clear(); _notifications.Clear(); }
    private void Commit()
    {
        Records.Clear();
        Records.AddRange(_working.Select(Fixture.Clone));
        Audits.AddRange(_audits);
        Notifications.AddRange(_notifications);
        Commits++;
        ClearChanges();
    }

    private sealed class FakeTransaction(FakeRepository repo) : IDbContextTransaction
    {
        public Guid TransactionId { get; } = Guid.NewGuid();
        private bool _committed;
        public void Commit() { repo.Commit(); _committed = true; }
        public Task CommitAsync(CancellationToken cancellationToken = default) { Commit(); return Task.CompletedTask; }
        public void Rollback() => repo.ClearChanges();
        public Task RollbackAsync(CancellationToken cancellationToken = default) { Rollback(); return Task.CompletedTask; }
        public void Dispose() { if (!_committed) Rollback(); }
        public ValueTask DisposeAsync() { Dispose(); return ValueTask.CompletedTask; }
    }
}
