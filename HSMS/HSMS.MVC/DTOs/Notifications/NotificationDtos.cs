namespace HSMS.MVC.DTOs.Notifications;

public sealed record NotificationItem(long Id, string Title, string Content, bool IsRead, DateTime SentAt, string? Link);
public sealed record NotificationFeed(int Unread, IReadOnlyList<NotificationItem> Items);
