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

    public async Task<ServiceResult<NotificationFeed>> FeedAsync(CancellationToken ct = default)
    {
        var user = GetCurrentUser();
        return user == null
            ? ServiceResult<NotificationFeed>.Failed(AuthErrors.MissingCredentials)
            : ServiceResult<NotificationFeed>.Success(await _repo.FeedAsync(user.Id, ct));
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

