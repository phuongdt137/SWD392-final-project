using System.Globalization;
using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Notifications;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;

namespace HSMS.MVC.Service;

public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly ICurrentUserService _current;
    private readonly IGenericRepository<Notification> _notificationRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository repo,
        ICurrentUserService current,
        IGenericRepository<Notification> notificationRepository,
        ILogger<NotificationService> logger)
    {
        _repo = repo;
        _current = current;
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<NotificationFeed>> FeedAsync(CancellationToken ct = default)
    {
        var user = await _current.GetAsync(ct);
        return user == null
            ? ServiceResult<NotificationFeed>.Failed(AuthErrors.MissingCredentials)
            : ServiceResult<NotificationFeed>.Success(await _repo.FeedAsync(user.UserId, ct));
    }

    public async Task<ServiceResult> ReadAsync(long id, CancellationToken ct = default)
    {
        var user = await _current.GetAsync(ct);
        if (user == null) return ServiceResult.Unauthorized();
        return await _repo.ReadAsync(user.UserId, id, ct) ? ServiceResult.Success() : ServiceResult.NotFound();
    }

    /// <summary>
    /// Gửi thông báo cho học sinh và phụ huynh khi điểm bị thay đổi.
    /// Lỗi gửi thông báo không làm thất bại thao tác cập nhật điểm.
    /// </summary>
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
