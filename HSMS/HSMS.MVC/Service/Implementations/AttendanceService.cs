using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using HSMS.MVC.Service.Notifications;
using HSMS.MVC.ViewModels.Attendance;

namespace HSMS.MVC.Service;

// Các file AttendanceService*.cs cùng thuộc một lớp, chia theo từng chức năng.
public partial class AttendanceService(IAttendanceRepository repo, SchoolTime time,
    NotificationDispatchSignal dispatch, ILogger<AttendanceService> logger) : IAttendanceService
{
    /// <summary>Chuẩn bị lịch điểm danh: giáo viên xem các tiết hôm nay, Staff/Principal dùng bộ lọc ngày và lớp.</summary>
    public async Task<ServiceResult<AttendanceIndexViewModel>> IndexAsync(LoginResponseDto user, AttendanceScheduleFilter filter,
        CancellationToken ct = default)
    {
        try
        {
            if (!user.SchoolReader && !user.EmployeeId.HasValue)
                throw new AppError("Tài khoản chưa có hồ sơ giáo viên.", "MISSING_PROFILE");

            filter.Date = user.SchoolReader ? (filter.Date ?? time.Today).Date : time.Today;
            if (!user.SchoolReader) filter.ClassId = null;
            filter.Page = Math.Clamp(filter.Page, 1, 100000);
            var employeeId = user.SchoolReader ? null : user.EmployeeId;
            var page = await repo.GetScheduleAsync(filter, employeeId, ct);
            return ServiceResult<AttendanceIndexViewModel>.Success(new AttendanceIndexViewModel
            {
                CanFilter = user.SchoolReader,
                CanViewHistory = user.SchoolReader,
                Filter = filter, Lessons = page.Items, Total = page.Total,
                Classes = user.SchoolReader ? await repo.GetClassesAsync(null, ct) : []
            });
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return HandleError<AttendanceIndexViewModel>(error);
        }
    }

    /// <summary>Tải học sinh, kết quả đã lưu và đơn nghỉ để hiển thị bảng điểm danh của một tiết trong ngày.</summary>
    public async Task<ServiceResult<AttendanceSheetViewModel>> SheetAsync(LoginResponseDto user, long entryId, DateTime date,
        CancellationToken ct = default)
    {
        try
        {
            var entry = await GetLessonAsync(entryId, ct);
            date = date.Date;

            var roster = await repo.GetStudentsAsync(entry.TeachingAssignment.ClassId, date, ct);
            var records = await repo.GetSheetAsync(entryId, date, ct);
            // Giữ cả kết quả của học sinh đã rời lớp để Staff còn sửa và mọi người còn đối chiếu.
            var students = roster.Concat(records.Select(r => r.Customer)).DistinctBy(s => s.Id)
                .OrderBy(s => s.FullName).ToList();
            var leaves = await repo.GetLeavesAsync(students.Select(s => s.Id).ToArray(), date, ct);
            var reason = GetReadOnlyReason(user, entry, date);
            var staffMode = user.Has("STAFF");
            if (reason == null && staffMode && records.Count == 0)
                reason = "Giáo viên chưa điểm danh. Staff chỉ được sửa kết quả đã có.";
            if (reason == null && !staffMode && roster.Count == 0)
                reason = "Lớp chưa có học sinh thuộc danh sách tại ngày học này.";

            return ServiceResult<AttendanceSheetViewModel>.Success(new AttendanceSheetViewModel
            {
                CanViewAudit = user.SchoolReader,
                Entry = entry, Date = date, CanWrite = reason == null, StaffMode = staffMode,
                ReadOnlyReason = reason,
                DeadlineUtc = SchoolTime.LockUtc(staffMode ? entry.TeachingAssignment.Semester.EndDate : date),
                Students = students.Select(s => new AttendanceStudentRow(s,
                    records.FirstOrDefault(r => r.CustomerId == s.Id),
                    leaves.Where(l => l.StudentCustomerId == s.Id).ToList(), roster.Any(r => r.Id == s.Id))).ToList()
            });
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return HandleError<AttendanceSheetViewModel>(error);
        }
    }

    /// <summary>Kiểm tra và lưu cả bảng điểm danh, nhật ký, thông báo phụ huynh trong cùng một giao dịch.</summary>
    public async Task<ServiceResult<AttendanceSaveResult>> SaveAsync(LoginResponseDto user, SaveAttendanceDto input,
        CancellationToken ct = default)
    {
        try
        {
            var entry = await GetLessonAsync(input.TimetableEntryId, ct);
            var date = input.Date.Date;
            EnsureWritable(user, entry, date);
            if (input.Rows == null || input.Rows.Count == 0)
                throw new AppError("Chưa có kết quả để lưu.");

            await using var transaction = await repo.BeginTransactionAsync(ct);
            var data = await LoadSaveDataAsync(user, entry, input, ct);
            var (result, hasEmail) = ApplyAttendanceRows(user, entry, input, data);

            await repo.SaveAsync(ct);
            // Lưu qua hạn thì hoàn tác cả giao dịch.
            EnsureWritable(user, entry, date);
            await transaction.CommitAsync(ct);

            if (hasEmail) dispatch.Wake();
            return ServiceResult<AttendanceSaveResult>.Success(result);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return HandleError<AttendanceSaveResult>(error);
        }
    }

    /// <summary>Lấy thông tin tiết học theo ID; báo lỗi nếu tiết học không tồn tại.</summary>
    private async Task<TimetableEntry> GetLessonAsync(long id, CancellationToken ct) =>
        await repo.GetLessonAsync(id, ct) ?? throw new AppError("Không tìm thấy tiết học.", "NOT_FOUND", ErrorType.NotFound);

    /// <summary>Trả về lý do bảng chỉ được xem; trả về null nếu vẫn được điểm danh hoặc chỉnh sửa.</summary>
    private string? GetReadOnlyReason(LoginResponseDto user, TimetableEntry entry, DateTime date)
    {
        if (!user.Has("STAFF") && !user.Has("TEACHER"))
            return "Tài khoản chỉ có quyền xem bảng điểm danh này.";
        if (!user.EmployeeId.HasValue)
            return "Tài khoản chưa có hồ sơ nhân viên để ghi nhận người chỉnh sửa.";
        if (entry.Status != "ACTIVE" || entry.TeachingAssignment.Status != "ACTIVE")
            return "Lịch hoặc phân công giảng dạy không còn hoạt động.";

        var term = entry.TeachingAssignment.Semester;
        if (time.Today < term.StartDate.Date || time.UtcNow >= SchoolTime.LockUtc(term.EndDate))
            return "Học kỳ đã kết thúc hoặc chưa bắt đầu. Bảng điểm danh chỉ được xem.";
        if (SchoolTime.Local(time.UtcNow) < date.Add(entry.TimeSlot.StartTime))
            return "Chưa đến giờ bắt đầu tiết học.";
        if (!user.Has("STAFF") && time.IsLocked(date))
            return "Giáo viên đã bị khóa điểm danh từ 00:00 ngày kế tiếp.";
        return null;
    }

    /// <summary>Báo lỗi nếu bảng không được ghi hoặc sửa theo giờ bắt đầu, hạn giáo viên và hạn học kỳ.</summary>
    private void EnsureWritable(LoginResponseDto user, TimetableEntry entry, DateTime date)
    {
        var error = GetReadOnlyReason(user, entry, date);
        if (error != null) throw new AppError(error, "ATTENDANCE_READ_ONLY", ErrorType.Forbidden);
    }

    /// <summary>Dọn trạng thái EF đang theo dõi và chuyển lỗi nghiệp vụ hoặc lỗi hệ thống thành kết quả thất bại.</summary>
    private ServiceResult<T> HandleError<T>(Exception error)
    {
        repo.ClearChanges();
        if (error is AppError appError)
            return ServiceResult<T>.Failed(appError);

        logger.LogError(error, "Không thể thực hiện thao tác Attendance");
        return ServiceResult<T>.Failed("Không thể xử lý điểm danh. Vui lòng tải lại trang và thử lại.", "ATTENDANCE_ERROR", 500);
    }
}
