using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Notifications;
using HSMS.MVC.Repositories.Interfaces;

namespace HSMS.MVC.Service;

// Dùng chung luồng danh tính server cho hộp thông báo; không lấy người nhận từ request.
public sealed class NotificationService(INotificationRepository repo, ICurrentUserService current) : INotificationService
{
    // Lấy số chưa đọc và danh sách của chính người đang đăng nhập.
    public async Task<ServiceResult<NotificationFeed>> FeedAsync(CancellationToken ct = default)
    {
        var user = await current.GetAsync(ct);
        return user == null ? ServiceResult<NotificationFeed>.Failed(AuthErrors.MissingCredentials) :
            ServiceResult<NotificationFeed>.Success(await repo.FeedAsync(user.UserId, ct));
    }
    // Chuông thông báo lấy số chưa đọc của tài khoản Session khi render trang.
    public async Task<int> UnreadCountAsync(CancellationToken ct = default)
    {
        var user = await current.GetAsync(ct);
        return user == null ? 0 : await repo.UnreadCountAsync(user.UserId, ct);
    }
    // Chỉ đánh dấu đã đọc khi thông báo thuộc phạm vi người dùng hiện tại.
    public async Task<ServiceResult> ReadAsync(long id, CancellationToken ct = default)
    {
        var user = await current.GetAsync(ct);
        if (user == null) return ServiceResult.Unauthorized();
        return await repo.ReadAsync(user.UserId, id, ct) ? ServiceResult.Success() : ServiceResult.NotFound();
    }
}
