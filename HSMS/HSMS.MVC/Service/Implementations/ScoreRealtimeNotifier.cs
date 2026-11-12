using HSMS.MVC.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace HSMS.MVC.Service;

/// <summary>
/// Gửi tín hiệu điểm qua SignalR tới nhóm giáo viên (theo bài kiểm tra) và nhóm học sinh (theo từng học sinh).
/// </summary>
public sealed class ScoreRealtimeNotifier(IHubContext<ScoreHub> hubContext, ILogger<ScoreRealtimeNotifier> logger)
    : IScoreRealtimeNotifier
{
    public async Task ScoresUpdatedAsync(long assessmentId, IEnumerable<long> studentCustomerIds)
    {
        try
        {
            var groups = studentCustomerIds
                .Distinct()
                .Select(ScoreHub.StudentGroup)
                .Append(ScoreHub.AssessmentGroup(assessmentId))
                .ToList();

            await hubContext.Clients.Groups(groups)
                .SendAsync(ScoreHub.ScoresUpdated, new ScoresUpdatedMessage(assessmentId));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Không gửi được tín hiệu realtime điểm cho AssessmentId: {AssessmentId}", assessmentId);
        }
    }
}
