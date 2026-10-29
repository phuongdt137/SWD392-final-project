using System.Globalization;
using System.Text.Json;
using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.DTOs.Notifications;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;

namespace HSMS.MVC.Service;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly IHttpContextAccessor _http;
    private readonly IGenericRepository<Notification> _notificationRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repo,
        IHttpContextAccessor http,
        IGenericRepository<Notification> notificationRepository,
        ILogger<NotificationService> logger)
    {
        _repo = repo;
        _http = http;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    private LoginResponseDto? GetCurrentUser()
    {
        var json = _http.HttpContext?.Session.GetString(SessionKeys.CurrentUser);
        if (string.IsNullOrEmpty(json)) return null;
        try { return JsonSerializer.Deserialize<LoginResponseDto>(json); }
        catch { return null; }
    }

    public async Task<ServiceResult<NotificationFeed>> FeedAsync(bool unreadOnly = false, CancellationToken ct = default)
    {
        var user = GetCurrentUser();
        return user == null
            ? ServiceResult<NotificationFeed>.Failed(AuthErrors.MissingCredentials)
            : ServiceResult<NotificationFeed>.Success(await _repo.FeedAsync(user.Id, unreadOnly, ct));
    }

    public async Task<int> UnreadCountAsync(CancellationToken ct = default)
    {
        var user = GetCurrentUser();
        return user == null
            ? 0
            : await _repo.UnreadCountAsync(user.Id, ct);
    }

    public async Task<ServiceResult> ReadAsync(long id, CancellationToken ct = default)
    {
        var user = GetCurrentUser();
        if (user == null) return ServiceResult.Unauthorized();
        return await _repo.ReadAsync(user.Id, id, ct) ? ServiceResult.Success() : ServiceResult.NotFound();
    }

    public async Task<ServiceResult<int>> ReadAllAsync(CancellationToken ct = default)
    {
        var user = GetCurrentUser();
        return user == null ? ServiceResult<int>.Unauthorized() : ServiceResult<int>.Success(await _repo.ReadAllAsync(user.Id, ct));
    }

    public Task<IReadOnlyList<NotificationClassOption>> ClassesAsync(CancellationToken ct = default) => _repo.ClassesAsync(ct);

    public async Task<ServiceResult<int>> SendAnnouncementAsync(SendNotificationRequest request, CancellationToken ct = default)
    {
        var user = GetCurrentUser();
        if (user == null) return ServiceResult<int>.Unauthorized();
        if (!user.Has("STAFF")) return ServiceResult<int>.Forbidden();
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Length > 200)
            return ServiceResult<int>.BadRequest("Nhập tiêu đề thông báo (tối đa 200 ký tự).");
        if (string.IsNullOrWhiteSpace(request.Content) || request.Content.Length > 4000)
            return ServiceResult<int>.BadRequest("Nhập nội dung thông báo (tối đa 4.000 ký tự).");
        if (request.TargetType == "ROLE" && !(new[] { "STUDENT", "PARENT", "TEACHER", "PRINCIPAL", "STAFF" }).Contains(request.RoleCode))
            return ServiceResult<int>.BadRequest("Vui lòng chọn vai trò nhận thông báo hợp lệ.");
        if (request.TargetType == "CLASS" && (!request.ClassId.HasValue || !(await _repo.ClassesAsync(ct)).Any(c => c.Id == request.ClassId)))
            return ServiceResult<int>.BadRequest("Vui lòng chọn lớp đang hoạt động.");
        if (request.TargetType is not ("ALL" or "ROLE" or "CLASS"))
            return ServiceResult<int>.BadRequest("Vui lòng chọn nhóm người nhận hợp lệ.");
        var count = await _repo.SendAnnouncementAsync(user.Id, request.TargetType, request.RoleCode, request.ClassId,
            request.Title.Trim(), request.Content.Trim(), ct);
        return count == 0 ? ServiceResult<int>.BadRequest("Không tìm thấy người nhận đang hoạt động trong nhóm đã chọn.") : ServiceResult<int>.Success(count);
    }

    public async Task NotifyScoreChangedAsync(StudentScore score, decimal? oldScore, long senderUserId)
    {
        try
        {
            var student = score.Customer;
            var subjectName = score.Assessment.TeachingAssignment.Subject.SubjectName;

            var title = "Điểm số đã được cập nhật";
            var content = $"Điểm {score.Assessment.AssessmentName} môn {subjectName} đã thay đổi từ " +
                          $"{FormatScore(oldScore)} thành {FormatScore(score.Score)}.";

            var recipientUserIds = new List<long> { student.UserId };
            if (student.Parent != null)
                recipientUserIds.Add(student.Parent.UserId);

            foreach (var recipientUserId in recipientUserIds.Distinct())
            {
                await _notificationRepository.AddAsync(
                    new Notification(title, content, recipientUserId, senderUserId));
            }

            await _notificationRepository.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể gửi thông báo thay đổi điểm cho ScoreId: {ScoreId}", score.Id);
        }
    }

    private static string FormatScore(decimal? score)
    {
        return score.HasValue
            ? score.Value.ToString("0.##", CultureInfo.InvariantCulture)
            : "chưa có";
    }
}

