using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Email;
using HSMS.MVC.Service.Interfaces;

namespace HSMS.MVC.Service.Implementations;

/// <summary>
/// Bọc PriorityQueue&lt;EmailMessage, EmailPriority&gt; với Lock (Thread-safe) và SemaphoreSlim (Async-wait).
/// </summary>
public sealed class EmailPriorityQueue : IEmailPriorityQueue
{
    private readonly PriorityQueue<EmailMessage, EmailPriority> _queue = new();
    private readonly object _syncLock = new();
    private readonly SemaphoreSlim _signal = new(0);

    public int Count
    {
        get
        {
            lock (_syncLock)
            {
                return _queue.Count;
            }
        }
    }

    public void Enqueue(EmailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        lock (_syncLock)
        {
            _queue.Enqueue(message, message.Priority);
        }

        // Báo thức cho BackgroundWorker đang chờ tại DequeueAsync
        _signal.Release();
    }

    public void Enqueue(
        string toEmail,
        string subject,
        string body,
        EmailPriority priority = EmailPriority.Normal,
        bool isHtml = true)
    {
        Enqueue(new EmailMessage(
            ToEmail: toEmail,
            Subject: subject,
            Body: body,
            IsHtml: isHtml,
            Priority: priority));
    }

    public async ValueTask<EmailMessage> DequeueAsync(CancellationToken cancellationToken = default)
    {
        // Ngủ chờ (0% CPU) cho tới khi có ít nhất 1 email được Enqueue
        await _signal.WaitAsync(cancellationToken);

        lock (_syncLock)
        {
            return _queue.Dequeue();
        }
    }
}
