using System.Security.Cryptography;
using System.Text;
using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using HSMS.MVC.Service.Notifications;
using HSMS.MVC.ViewModels.Attendance;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Service;

public sealed class AttendanceService(IAttendanceRepository repo, ICurrentUserService current, SchoolTime time,
    NotificationDispatchSignal dispatch, ILogger<AttendanceService> logger) : IAttendanceService
{
    private async Task<CurrentUser> User(CancellationToken ct) => await current.GetAsync(ct) ??
        throw new AppError("Vui lòng đăng nhập lại.", "UNAUTHORIZED", ErrorType.Unauthorized);
    private static AppError Forbidden() => new("Bạn không có quyền truy cập bảng điểm danh này.", "FORBIDDEN", ErrorType.Forbidden);
    private static AppError Conflict() => new("Dữ liệu đã thay đổi. Vui lòng tải lại bảng và đối chiếu.", "ATTENDANCE_CONFLICT", ErrorType.Conflict);
    private static void ValidateLesson(TimetableEntry e, DateTime date, bool write)
    {
        var a = e.TeachingAssignment;
        if (SchoolTime.Weekday(date) != e.DayOfWeek || (e.EffectiveFrom.HasValue && date < e.EffectiveFrom.Value.Date) ||
            (e.EffectiveTo.HasValue && date > e.EffectiveTo.Value.Date) || date < a.Semester.StartDate || date > a.Semester.EndDate ||
            date < a.Class.AcademicYear.StartDate || date > a.Class.AcademicYear.EndDate || a.Semester.AcademicYearId != a.Class.AcademicYearId)
            throw new AppError("Ngày không thuộc lịch học, học kỳ hoặc năm học hợp lệ.", "INVALID_LESSON_DATE");
        if (write && (e.Status != "ACTIVE" || a.Status != "ACTIVE" || a.Class.Status != "ACTIVE"))
            throw new AppError("Lớp, phân công hoặc thời khóa biểu đã ngừng hoạt động.", "INACTIVE_LESSON");
    }
    private static void ValidateRoster(List<StudentClass> roster)
    {
        if (roster.GroupBy(x => x.CustomerId).Any(g => g.Count() > 1))
            throw new AppError("Lịch sử tham gia lớp bị trùng khoảng ngày. Cần đối chiếu hồ sơ học vụ.", "OVERLAPPING_ENROLLMENT", ErrorType.Conflict);
    }
    private static string RosterToken(TimetableEntry e, DateTime date, List<StudentClass> roster)
    {
        var value = $"{e.Id}|{date:yyyy-MM-dd}|{e.DayOfWeek}|{e.EffectiveFrom:O}|{e.EffectiveTo:O}|{e.Status}|" +
            $"{e.TeachingAssignmentId}|{e.TeachingAssignment.ClassId}|{e.TeachingAssignment.EmployeeId}|{e.TeachingAssignment.SubjectId}|{e.TeachingAssignment.Status}|" +
            $"{e.TimeSlotId}|{e.TimeSlot.StartTime}|{e.TimeSlot.EndTime}|{e.RoomId}|" +
            string.Join(';', roster.OrderBy(x => x.Id).Select(x => $"{x.Id}:{x.CustomerId}:{x.JoinedAt:O}:{x.LeftAt:O}"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    }
    public Task<ServiceResult<AttendanceIndex>> IndexAsync(DateTime date, CancellationToken ct = default) =>
        IndexAsync(new AttendanceScheduleFilter { Date = date }, ct);
    public async Task<ServiceResult<AttendanceIndex>> IndexAsync(AttendanceScheduleFilter filter, CancellationToken ct = default)
    {
        try
        {
            filter.Date = (filter.Date ?? time.Today).Date;
            filter.Query = string.IsNullOrWhiteSpace(filter.Query) ? null : filter.Query.Trim();
            if (filter.Query?.Length > 100) throw new AppError("Từ khóa tìm kiếm tối đa 100 ký tự.");
            filter.Status = string.IsNullOrWhiteSpace(filter.Status) ? null : filter.Status.ToUpperInvariant();
            if (filter.Status != null && filter.Status is not ("COMPLETED" or "PENDING" or "UPCOMING" or "LOCKED"))
                throw new AppError("Tình trạng điểm danh không hợp lệ.");
            var date = filter.Date.Value;
            var user = await User(ct);
            var localNow = SchoolTime.Local(time.UtcNow);
            var locked = time.IsLocked(date) && !AttendanceAccess.CanEditAfterDeadline(user);
            var data = await repo.GetSchedulePageAsync(filter, AttendanceAccess.Schedules(user), localNow, locked, ct);
            filter.Page = data.Page;
            var rows = data.Rows.Select(x => new AttendanceLesson(x.Entry, x.Students, x.Recorded, locked,
                localNow >= date.Add(x.Entry.TimeSlot.StartTime))
            {
                CanWrite = AttendanceAccess.CanWrite(user, x.Entry) && !locked && x.Students > 0 && localNow >= date.Add(x.Entry.TimeSlot.StartTime) &&
                    x.Entry.Status == "ACTIVE" && x.Entry.TeachingAssignment.Status == "ACTIVE" && x.Entry.TeachingAssignment.Class.Status == "ACTIVE"
            }).ToList();
            return ServiceResult<AttendanceIndex>.Success(new(date, rows)
            { Filter = filter, Choices = data.Choices, Total = data.Total, Classes = data.Classes, Completed = data.Completed, Pending = data.Pending });
        }
        catch (AppError e) { return ServiceResult<AttendanceIndex>.Failed(e); }
    }
    public async Task<ServiceResult<AttendanceSheet>> SheetAsync(long entryId, DateTime date, CancellationToken ct = default)
    {
        try
        {
            date = date.Date; var user = await User(ct);
            var e = await repo.GetLessonAsync(entryId, ct) ?? throw new AppError("Không tìm thấy tiết học.", "NOT_FOUND", ErrorType.NotFound);
            if (!AttendanceAccess.CanViewSheet(user, e)) throw Forbidden();
            ValidateLesson(e, date, false);
            var roster = await repo.GetRosterAsync(e.TeachingAssignment.ClassId, date, ct); ValidateRoster(roster);
            await EnsureEnrollment(e, date, roster, ct);
            var records = await repo.GetSheetAsync(entryId, date, ct);
            var leaves = await repo.GetLeavesAsync(roster.Select(x => x.CustomerId).ToArray(), date, ct);
            string? reason = null;
            if (!AttendanceAccess.CanWrite(user, e)) reason = "Bạn có quyền xem; giáo viên được phân công, STAFF hoặc ADMIN có hồ sơ nhân viên được ghi nhận.";
            try { ValidateLesson(e, date, true); time.EnsureWritable(date, e.TimeSlot.StartTime, AttendanceAccess.CanEditAfterDeadline(user)); }
            catch (AppError ex) { reason = ex.Description; }
            if (roster.Count == 0) reason = "Lớp chưa có học sinh trong ngày học này.";
            var ids = roster.Select(x => x.CustomerId).ToHashSet();
            var historical = records.Where(x => !ids.Contains(x.CustomerId)).ToList();
            if (historical.Count > 0) reason = "Danh sách đã thay đổi so với kết quả lưu. Cần đối chiếu hồ sơ; các kết quả cũ được giữ nguyên.";
            return ServiceResult<AttendanceSheet>.Success(new()
            {
                Entry = e, Date = date, CanWrite = reason == null, CanEditAfterDeadline = AttendanceAccess.CanEditAfterDeadline(user), ReadOnlyReason = reason, RosterToken = RosterToken(e, date, roster),
                Students = roster.Select(x => new AttendanceStudent(x.Customer, records.SingleOrDefault(r => r.CustomerId == x.CustomerId),
                    leaves.Where(l => l.StudentCustomerId == x.CustomerId).ToList())).ToList(), HistoricalRecords = historical
            });
        }
        catch (AppError e) { return ServiceResult<AttendanceSheet>.Failed(e); }
    }
    public async Task<ServiceResult<SaveAttendanceResult>> SaveAsync(SaveAttendanceDto input, CancellationToken ct = default)
    {
        try
        {
            await using var tx = await repo.BeginAsync(ct);
            var user = await User(ct);
            var date = input.Date.Date;
            var e = await repo.GetLessonAsync(input.TimetableEntryId, ct) ?? throw new AppError("Không tìm thấy tiết học.", "NOT_FOUND", ErrorType.NotFound);
            if (!AttendanceAccess.CanWrite(user, e)) throw Forbidden();
            ValidateLesson(e, date, true); time.EnsureWritable(date, e.TimeSlot.StartTime, AttendanceAccess.CanEditAfterDeadline(user));
            if (input.ChangeReason?.Length > 1000) throw new AppError("Lý do sửa tối đa 1.000 ký tự.");
            var roster = await repo.GetRosterAsync(e.TeachingAssignment.ClassId, date, ct); ValidateRoster(roster);
            await EnsureEnrollment(e, date, roster, ct);
            if (roster.Count == 0) throw new AppError("Không thể điểm danh lớp rỗng.", "EMPTY_ROSTER");
            var ids = roster.Select(x => x.CustomerId).ToHashSet();
            if (RosterToken(e, date, roster) != input.RosterToken || input.Rows == null || input.Rows.Count != ids.Count ||
                input.Rows.Select(x => x.CustomerId).Distinct().Count() != ids.Count || input.Rows.Any(x => !ids.Contains(x.CustomerId)))
                throw Conflict();
            var records = await repo.GetSheetAsync(e.Id, date, ct);
            if (records.Any(x => !ids.Contains(x.CustomerId))) throw Conflict();
            var leaves = await repo.GetLeavesAsync(ids.ToArray(), date, ct);
            // Validate the entire form before changing tracked entities.
            foreach (var row in input.Rows)
            {
                row.Status = AttendanceStatuses.Validate(row.Status);
                row.Comment = string.IsNullOrWhiteSpace(row.Comment) ? null : row.Comment.Trim();
                if (row.Comment?.Length > 1000) throw new AppError("Ghi chú tối đa 1.000 ký tự.");
                if (row.Status == AttendanceStatuses.Excused && !leaves.Any(x => x.Id == row.LeaveRequestId && x.StudentCustomerId == row.CustomerId))
                    throw new AppError("Đơn nghỉ cần được duyệt, đúng học sinh và bao phủ ngày học.", "INVALID_APPROVED_LEAVE");
                if (row.Status != AttendanceStatuses.Excused) row.LeaveRequestId = null;
            }
            bool Changed(AttendanceInput row, AttendanceRecord? record) => record == null ||
                record.Status != row.Status || record.Comment != row.Comment || record.LeaveRequestId != row.LeaveRequestId;
            var changes = input.Rows.Where(row => Changed(row, records.SingleOrDefault(r => r.CustomerId == row.CustomerId))).ToList();
            if (changes.Count > 0)
            {
                foreach (var row in input.Rows)
                    if (records.SingleOrDefault(r => r.CustomerId == row.CustomerId)?.Version != row.Version) throw Conflict();
                if (changes.Any(row => records.Any(r => r.CustomerId == row.CustomerId)) && string.IsNullOrWhiteSpace(input.ChangeReason))
                    throw new AppError("Nhập lý do khi thay đổi kết quả đã lưu.", "CHANGE_REASON_REQUIRED");
            }
            if (changes.Count > 0 && user.Has("STAFF") && !user.EmployeeId.HasValue)
                user = user with { EmployeeId = await repo.EnsureStaffEmployeeAsync(user.UserId, ct) };
            var parents = await repo.GetActiveParentsAsync(changes.Select(x => x.CustomerId).ToArray(), ct);
            var missingParents = 0;
            foreach (var row in changes)
            {
                var record = records.SingleOrDefault(x => x.CustomerId == row.CustomerId);
                var oldStatus = record?.Status; var oldComment = record?.Comment; var oldLeave = record?.LeaveRequestId;
                if (record == null)
                {
                    record = new AttendanceRecord(row.CustomerId, e.Id, user.EmployeeId!.Value, date, AttendanceStatuses.Present, createdAtUtc: time.UtcNow);
                    await repo.AddAsync(record);
                }
                record.RecordResult(row.Status, row.Comment, leaves.SingleOrDefault(x => x.Id == row.LeaveRequestId), user.EmployeeId!.Value, time.UtcNow);
                var audit = new AttendanceAuditLog(record, oldStatus, oldComment, oldLeave, input.ChangeReason?.Trim(), user.EmployeeId!.Value, time.UtcNow);
                repo.AddAudit(audit);
                if (oldStatus == record.Status || (!AttendanceStatuses.IsAlert(oldStatus) && !AttendanceStatuses.IsAlert(record.Status))) continue;
                var parent = parents.SingleOrDefault(p => p.Children.Any(c => c.Id == row.CustomerId));
                if (parent == null) { missingParents++; continue; }
                var student = roster.Single(x => x.CustomerId == row.CustomerId).Customer;
                var text = $"{student.FullName} ({student.StudentCode}) — {e.TeachingAssignment.Class.ClassName}, {e.TeachingAssignment.Subject.SubjectName}, tiết {e.TimeSlot.PeriodNo}, {date:dd/MM/yyyy}: {AttendanceStatuses.Label(record.Status)}.";
                var notification = new Notification(oldStatus == null ? "Kết quả điểm danh" : "Cập nhật điểm danh", text, parent.UserId, user.UserId);
                notification.AttachAttendance(audit, $"/Attendance/History?studentId={row.CustomerId}&from={date:yyyy-MM-dd}&to={date:yyyy-MM-dd}", time.UtcNow);
                repo.AddNotification(notification);
            }
            time.EnsureWritable(date, e.TimeSlot.StartTime, AttendanceAccess.CanEditAfterDeadline(user));
            await repo.FlushAsync(ct);
            // Recheck at commit: teachers stop at midnight; STAFF and ADMIN may correct past dates.
            time.EnsureWritable(date, e.TimeSlot.StartTime, AttendanceAccess.CanEditAfterDeadline(user));
            await tx.CommitAsync(ct);
            if (changes.Count > 0) dispatch.Wake();
            return ServiceResult<SaveAttendanceResult>.Success(new(changes.Count, missingParents));
        }
        catch (AppError e) { repo.DiscardChanges(); return ServiceResult<SaveAttendanceResult>.Failed(e); }
        catch (Exception e) when (IsConcurrencyConflict(e))
        { repo.DiscardChanges(); return ServiceResult<SaveAttendanceResult>.Failed(Conflict()); }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            repo.DiscardChanges(); logger.LogError(e, "Attendance save failed for lesson {LessonId}", input.TimetableEntryId);
            return ServiceResult<SaveAttendanceResult>.Failed(new AppError("Không thể lưu điểm danh. Vui lòng thử lại.", "SYSTEM_ERROR", ErrorType.Failure));
        }
    }
    public async Task<ServiceResult<AttendanceHistory>> HistoryAsync(AttendanceFilter f, CancellationToken ct = default)
    {
        try
        {
            var user = await User(ct); f.Page = Math.Clamp(f.Page, 1, 100000);
            f.From ??= time.Today.AddDays(-30); f.To ??= time.Today;
            if (f.From > f.To) throw new AppError("Ngày bắt đầu phải trước ngày kết thúc.");
            if (!string.IsNullOrWhiteSpace(f.Status)) f.Status = AttendanceStatuses.Validate(f.Status); else f.Status = null;
            var data = await repo.HistoryAsync(AttendanceAccess.Records(user), f, ct);
            var choices = await repo.GetHistoryChoicesAsync(AttendanceAccess.Records(user), ct);
            if (!user.SchoolReader && !user.Has("TEACHER"))
                choices = choices with { Students = await repo.GetStudentChoicesAsync(user.CustomerId, user.Has("PARENT"), ct) };
            return ServiceResult<AttendanceHistory>.Success(new(f, data.Records, data.Total, data.Summary, choices));
        }
        catch (AppError e) { return ServiceResult<AttendanceHistory>.Failed(e); }
    }
    public async Task<ServiceResult<AttendanceAudit>> AuditAsync(long recordId, CancellationToken ct = default)
    {
        try
        {
            var user = await User(ct);
            var record = await repo.GetRecordAsync(recordId, AttendanceAccess.Records(user), ct) ?? throw new AppError("Không tìm thấy kết quả trong phạm vi truy cập.", "NOT_FOUND", ErrorType.NotFound);
            if (!user.SchoolReader && !user.Has("TEACHER")) throw Forbidden();
            return ServiceResult<AttendanceAudit>.Success(new(record, await repo.GetAuditAsync(recordId, ct)));
        }
        catch (AppError e) { return ServiceResult<AttendanceAudit>.Failed(e); }
    }
    public async Task<ServiceResult<StudentAttendanceHistory>> StudentHistoryAsync(AttendanceFilter filter, CancellationToken ct = default)
    {
        try
        {
            var user = await User(ct);
            if ((!user.Has("STUDENT") && !user.Has("PARENT")) || !user.CustomerId.HasValue)
                throw Forbidden();
            var isParent = user.Has("PARENT");
            var students = await repo.GetStudentChoicesAsync(user.CustomerId, isParent, ct);
            var student = filter.StudentId.HasValue ? students.SingleOrDefault(x => x.Id == filter.StudentId) : students.FirstOrDefault();
            if (filter.StudentId.HasValue && student == null) throw Forbidden();
            var localNow = SchoolTime.Local(time.UtcNow);
            if (student == null)
                return ServiceResult<StudentAttendanceHistory>.Success(new() { IsParent = isParent, Students = students, LocalNow = localNow });
            var assignments = await repo.GetStudentAssignmentsAsync(student.Id, ct);
            var referenceDate = filter.From?.Date ?? time.Today;
            var semesters = assignments.Select(x => x.Semester).DistinctBy(x => x.Id)
                .OrderByDescending(x => x.StartDate).ToList();
            var semester = filter.SemesterId.HasValue ? semesters.SingleOrDefault(x => x.Id == filter.SemesterId) :
                semesters.FirstOrDefault(x => x.StartDate <= referenceDate && referenceDate <= x.EndDate) ??
                semesters.FirstOrDefault(x => x.StartDate <= referenceDate) ?? semesters.FirstOrDefault();
            if (filter.SemesterId.HasValue && semester == null) throw Forbidden();
            var courses = assignments.Where(x => x.SemesterId == semester?.Id).ToList();
            var selected = filter.TeachingAssignmentId.HasValue ? courses.SingleOrDefault(x => x.Id == filter.TeachingAssignmentId) :
                courses.FirstOrDefault(x => (!filter.ClassId.HasValue || x.ClassId == filter.ClassId) &&
                    (!filter.SubjectId.HasValue || x.SubjectId == filter.SubjectId));
            if (filter.TeachingAssignmentId.HasValue && selected == null) throw Forbidden();
            if (selected == null)
                return ServiceResult<StudentAttendanceHistory>.Success(new()
                {
                    IsParent = isParent, Students = students, StudentId = student.Id, StudentName = student.Name, LocalNow = localNow,
                    Semesters = semesters, Assignments = courses, SemesterId = semester?.Id
                });

            var data = await repo.GetStudentScheduleAsync(student.Id, selected.Id, selected.ClassId, ct);
            var records = data.Records.ToDictionary(x => (x.TimetableEntryId, x.AttendanceDate.Date));
            var sessions = new Dictionary<(long EntryId, DateTime Date), StudentAttendanceSession>();
            foreach (var entry in data.Entries.Where(x => x.Status == "ACTIVE" && selected.Status == "ACTIVE" && selected.Class.Status == "ACTIVE"))
            {
                var start = new[] { selected.Semester.StartDate.Date, selected.Class.AcademicYear.StartDate.Date,
                    entry.EffectiveFrom?.Date ?? selected.Semester.StartDate.Date }.Max();
                var end = new[] { selected.Semester.EndDate.Date, selected.Class.AcademicYear.EndDate.Date,
                    entry.EffectiveTo?.Date ?? selected.Semester.EndDate.Date }.Min();
                var first = start.AddDays((entry.DayOfWeek - SchoolTime.Weekday(start) + 7) % 7);
                for (var date = first; date <= end; date = date.AddDays(7))
                {
                    if (!data.Memberships.Any(m => m.JoinedAt.Date <= date && (!m.LeftAt.HasValue || date < m.LeftAt.Value.Date))) continue;
                    records.TryGetValue((entry.Id, date), out var record);
                    sessions[(entry.Id, date)] = new(date, entry, record);
                }
            }
            // Preserve recorded history even when enrollment or schedule status has since changed.
            foreach (var record in data.Records)
            {
                var entry = data.Entries.Single(x => x.Id == record.TimetableEntryId);
                sessions[(entry.Id, record.AttendanceDate.Date)] = new(record.AttendanceDate.Date, entry, record);
            }
            var ordered = sessions.Values.OrderBy(x => x.Date).ThenBy(x => x.Entry.TimeSlot.StartTime).ThenBy(x => x.Entry.Id).ToList();
            var recorded = ordered.Count(x => x.Record != null);
            var absent = ordered.Count(x => x.Record?.Status is AttendanceStatuses.Absent or AttendanceStatuses.Excused);
            var pageCount = Math.Max(1, (int)Math.Ceiling(ordered.Count / (double)StudentAttendanceHistory.PageSize));
            var page = Math.Clamp(filter.Page, 1, pageCount);
            return ServiceResult<StudentAttendanceHistory>.Success(new()
            {
                IsParent = isParent, Students = students, StudentId = student.Id, StudentName = student.Name, LocalNow = localNow,
                Semesters = semesters, Assignments = courses, SemesterId = semester?.Id, Assignment = selected,
                Sessions = ordered.Skip((page - 1) * StudentAttendanceHistory.PageSize).Take(StudentAttendanceHistory.PageSize).ToList(),
                Recorded = recorded, Absent = absent,
                Unmarked = ordered.Count(x => x.Record == null && x.Date.Add(x.Entry.TimeSlot.StartTime) <= localNow),
                Upcoming = ordered.Count(x => x.Record == null && x.Date.Add(x.Entry.TimeSlot.StartTime) > localNow),
                Total = ordered.Count, Page = page
            });
        }
        catch (AppError e) { return ServiceResult<StudentAttendanceHistory>.Failed(e); }
    }
    public async Task<AttendanceSummary> DashboardAsync(CancellationToken ct = default)
    {
        var history = await HistoryAsync(new() { From = time.Today, To = time.Today }, ct);
        var index = await IndexAsync(time.Today, ct);
        var pending = index.Data?.Pending ?? 0;
        return (history.Data?.Summary ?? new AttendanceSummary(0, 0, 0, 0)) with { Pending = pending };
    }
    private async Task EnsureEnrollment(TimetableEntry e, DateTime date, List<StudentClass> roster, CancellationToken ct)
    {
        if (await repo.HasOverlappingEnrollmentAsync(e.TeachingAssignment.ClassId, date, roster.Select(x => x.CustomerId).ToArray(), ct))
            throw new AppError("Học sinh có khoảng tham gia lớp chồng nhau trong năm học. Cần đối chiếu hồ sơ.", "OVERLAPPING_ENROLLMENT", ErrorType.Conflict);
    }
    // EF's SQL Server execution strategy can wrap a deadlock in InvalidOperationException.
    private static bool IsConcurrencyConflict(Exception error) => error is DbUpdateConcurrencyException ||
        error is SqlException { Number: 1205 or 2601 or 2627 } ||
        (error.InnerException != null && IsConcurrencyConflict(error.InnerException));
}
