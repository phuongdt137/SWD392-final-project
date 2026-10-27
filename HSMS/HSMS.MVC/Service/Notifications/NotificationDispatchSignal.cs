using System.Threading.Channels;

namespace HSMS.MVC.Service.Notifications;

// Tín hiệu đánh thức tác vụ email trong bộ nhớ; hàng chờ thật là Notification/MessageLog trong database.
public sealed class NotificationDispatchSignal
{
    // Chỉ cần một tín hiệu chờ; nhiều lần Wake liên tiếp được gộp để tránh tích lũy dư thừa.
    private readonly Channel<bool> channel = Channel.CreateBounded<bool>(new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropWrite });
    public void Wake() => channel.Writer.TryWrite(true);
    // Đợi tín hiệu hoặc tối đa 15 giây rồi quét lại database; token ứng dụng dừng sẽ kết thúc vòng nền.
    public async Task WaitAsync(CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        try { await channel.Reader.ReadAsync(timeout.Token); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { }
    }
}
