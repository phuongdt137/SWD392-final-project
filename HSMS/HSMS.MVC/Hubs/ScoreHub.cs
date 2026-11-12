using System.Security.Claims;
using HSMS.MVC.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace HSMS.MVC.Hubs;

/// <summary>
/// Kênh realtime của module Đánh giá & Điểm số.
/// Server chỉ gửi tín hiệu "ScoresUpdated" (không kèm dữ liệu điểm). Trình duyệt nhận tín hiệu rồi
/// tải lại bảng điểm bằng endpoint thường, nên quyền xem vẫn do ScoreService kiểm tra.
/// </summary>
[Authorize]
public sealed class ScoreHub(IScoreService scoreService) : Hub
{
    /// <summary>Tên sự kiện client nhận được.</summary>
    public const string ScoresUpdated = "ScoresUpdated";

    /// <summary>Nhóm theo dõi bảng điểm của một bài kiểm tra (dành cho giáo viên được phân công).</summary>
    public static string AssessmentGroup(long assessmentId) => $"assessment:{assessmentId}";

    /// <summary>Nhóm theo dõi điểm của một học sinh (dành cho học sinh và phụ huynh của học sinh đó).</summary>
    public static string StudentGroup(long studentCustomerId) => $"student:{studentCustomerId}";

    /// <summary>Giáo viên được phân công lớp/môn của bài kiểm tra mới được theo dõi bảng điểm đó.</summary>
    public async Task JoinAssessment(long assessmentId)
    {
        if (!await scoreService.CanWatchAssessmentAsync(assessmentId, CurrentUserId()))
            throw new HubException("Bạn không có quyền theo dõi bảng điểm này.");

        await Groups.AddToGroupAsync(Context.ConnectionId, AssessmentGroup(assessmentId));
    }

    /// <summary>Học sinh theo dõi điểm của chính mình; phụ huynh theo dõi điểm của con.</summary>
    public async Task JoinStudentScores(long studentCustomerId)
    {
        if (!await scoreService.CanWatchStudentScoresAsync(studentCustomerId, CurrentUserId()))
            throw new HubException("Bạn không có quyền theo dõi điểm của học sinh này.");

        await Groups.AddToGroupAsync(Context.ConnectionId, StudentGroup(studentCustomerId));
    }

    private long CurrentUserId()
    {
        var value = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.TryParse(value, out var userId)
            ? userId
            : throw new HubException("Phiên đăng nhập không hợp lệ.");
    }
}

/// <summary>Nội dung tín hiệu "ScoresUpdated": chỉ cho client biết bài kiểm tra nào vừa đổi điểm.</summary>
public sealed record ScoresUpdatedMessage(long AssessmentId);
