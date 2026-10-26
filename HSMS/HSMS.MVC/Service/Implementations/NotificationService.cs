using System.Globalization;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;

namespace HSMS.MVC.Service;

/// <summary>
/// Thực thi gửi thông báo điểm số bằng cách ghi thực thể Notification cho học sinh và phụ huynh.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IGenericRepository<Notification> _notificationRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IGenericRepository<Notification> notificationRepository,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
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
