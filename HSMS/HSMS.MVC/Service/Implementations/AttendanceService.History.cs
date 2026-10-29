using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.ViewModels.Attendance;

namespace HSMS.MVC.Service;

public partial class AttendanceService
{
    /// <summary>Chuẩn bị kết quả và bộ lọc cho màn hình quản lý hoặc màn hình học sinh/phụ huynh.</summary>
    public async Task<ServiceResult<AttendanceHistoryViewModel>> HistoryAsync(LoginResponseDto user, AttendanceHistoryFilter filter,
        CancellationToken ct = default)
    {
        try
        {
            // Chọn cách hiển thị và lọc dữ liệu; quyền vào màn hình do Controller kiểm tra.
            if (!user.SchoolReader)
                return ServiceResult<AttendanceHistoryViewModel>.Success(await GetStudentHistoryAsync(user, filter, ct));
            if (filter.From == null && filter.To == null && filter.Date == null)
            {
                filter.From = time.Today.AddDays(-30);
                filter.To = time.Today;
            }
            filter.From = filter.From?.Date;
            filter.To = filter.To?.Date;
            filter.Date = filter.Date?.Date;
            if (filter.From > filter.To) throw new AppError("Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.");
            filter.Page = Math.Clamp(filter.Page, 1, 100000);
            filter.Status = string.IsNullOrWhiteSpace(filter.Status) ? null : AttendanceStatuses.Validate(filter.Status);
            var page = await repo.GetHistoryAsync(filter, r => true, ct);
            var choices = await repo.GetHistoryChoicesAsync(r => true, ct);
            return ServiceResult<AttendanceHistoryViewModel>.Success(new AttendanceHistoryViewModel
            {
                Filter = filter, Records = page.Items, Total = page.Total,
                Classes = choices.Classes, Students = choices.Students
            });
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return HandleError<AttendanceHistoryViewModel>(error);
        }
    }

    /// <summary>Lấy kết quả theo học sinh được chọn, học kỳ, môn, ngày và trạng thái.</summary>
    private async Task<AttendanceHistoryViewModel> GetStudentHistoryAsync(LoginResponseDto user,
        AttendanceHistoryFilter filter, CancellationToken ct)
    {
        if (!user.CustomerId.HasValue) throw new AppError("Tài khoản chưa có hồ sơ học sinh/phụ huynh.", "MISSING_PROFILE", ErrorType.Forbidden);
        var isParent = user.Has("PARENT");
        var students = isParent
            ? await repo.GetChildrenAsync(user.CustomerId.Value, ct)
            : new List<AttendanceChoice> { new(user.CustomerId.Value, user.FullName) };
        filter.ClassId = null;
        filter.Page = Math.Clamp(filter.Page, 1, 100000);
        filter.Status = string.IsNullOrWhiteSpace(filter.Status) ? null : AttendanceStatuses.Validate(filter.Status);
        // Giữ liên kết thông báo cũ có from=to, nhưng giao diện mới chỉ dùng một ngày học.
        filter.Date = (filter.Date ?? (filter.From == filter.To ? filter.From : null))?.Date;
        filter.From = null;
        filter.To = null;

        filter.StudentId = students.FirstOrDefault(s => s.Id == filter.StudentId)?.Id ?? students.FirstOrDefault()?.Id;
        if (!filter.StudentId.HasValue)
            return new AttendanceHistoryViewModel { IsLearner = true, IsParent = isParent, Filter = filter, Students = students };

        var semesters = await repo.GetStudentSemestersAsync(filter.StudentId.Value, ct);
        var selectedDate = filter.Date ?? time.Today;
        var semester = semesters.FirstOrDefault(s => s.Id == filter.SemesterId)
            ?? semesters.FirstOrDefault(s => s.StartDate <= selectedDate && s.EndDate >= selectedDate)
            ?? semesters.FirstOrDefault();
        if (semester == null)
            return new AttendanceHistoryViewModel { IsLearner = true, IsParent = isParent, Filter = filter, Students = students };

        filter.SemesterId = semester.Id;
        var subjects = await repo.GetStudentSubjectsAsync(filter.StudentId.Value, semester.Id, ct);
        if (!subjects.Any(s => s.Id == filter.SubjectId)) filter.SubjectId = null;

        var page = await repo.GetHistoryAsync(filter, r => r.CustomerId == filter.StudentId, ct);
        return new AttendanceHistoryViewModel
        {
            IsLearner = true, IsParent = isParent, Filter = filter, Students = students,
            Semesters = semesters, Subjects = subjects, Records = page.Items, Total = page.Total
        };
    }

    /// <summary>Lấy kết quả điểm danh và các lần thay đổi để hiển thị nhật ký.</summary>
    public async Task<ServiceResult<AttendanceAuditViewModel>> AuditAsync(long recordId,
        CancellationToken ct = default)
    {
        try
        {
            // Action Audit chỉ cho Staff/Principal, được xem nhật ký toàn trường.
            var record = await repo.GetRecordAsync(recordId, r => true, ct)
                ?? throw new AppError("Không tìm thấy kết quả trong phạm vi được phép.", "NOT_FOUND", ErrorType.NotFound);
            return ServiceResult<AttendanceAuditViewModel>.Success(new AttendanceAuditViewModel(record, await repo.GetAuditAsync(recordId, ct)));
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            return HandleError<AttendanceAuditViewModel>(error);
        }
    }

}
