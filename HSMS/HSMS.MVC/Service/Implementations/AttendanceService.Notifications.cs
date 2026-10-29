using System.Net.Mail;
using HSMS.MVC.Common;
using HSMS.MVC.Models;

namespace HSMS.MVC.Service;

public partial class AttendanceService
{
    // Kết quả tạo cảnh báo để màn hình báo rõ phần chưa gửi được.
    private enum ParentAlertResult { Queued, MissingParent, MissingEmail }

    /// <summary>Tạo thông báo và xếp hàng email cho phụ huynh; trả về tình trạng thiếu người nhận hoặc email.</summary>
    private ParentAlertResult QueueParentAlert(Customer student, TimetableEntry entry, AttendanceRecord record,
        AttendanceAuditLog audit, long senderId)
    {
        var parent = student.Parent?.User;
        if (parent == null || !parent.IsActive || !parent.UserRoles.Any(r => r.IsActive && r.Role.IsActive && r.Role.RoleCode == "PARENT"))
            return ParentAlertResult.MissingParent;

        var title = $"Điểm danh: {AttendanceStatuses.Label(record.Status)}";
        var content = $"{student.FullName} ({student.StudentCode}) - lớp {entry.TeachingAssignment.Class.ClassName}, " +
            $"môn {entry.TeachingAssignment.Subject.SubjectName}, tiết {entry.TimeSlot.PeriodNo}, " +
            $"ngày {record.AttendanceDate:dd/MM/yyyy}: {AttendanceStatuses.Label(record.Status)}.";
        var notification = new Notification(title, content, parent.Id, senderId);
        notification.AttachAttendance(audit,
            $"/Attendance/History?studentId={student.Id}&semesterId={entry.TeachingAssignment.SemesterId}&subjectId={entry.TeachingAssignment.SubjectId}&date={record.AttendanceDate:yyyy-MM-dd}", time.UtcNow);
        repo.AddNotification(notification);

        if (!MailAddress.TryCreate(parent.Email, out var email) || email.Host.Equals("invalid", StringComparison.OrdinalIgnoreCase) ||
            email.Host.EndsWith(".invalid", StringComparison.OrdinalIgnoreCase))
        {
            notification.Delivered(time.UtcNow);
            return ParentAlertResult.MissingEmail;
        }

        notification.MessageLogs.Add(new MessageLog("EMAIL", email.Address, content, title,
            templateCode: "ATTENDANCE", recipientUserId: parent.Id, createdByUserId: senderId));
        return ParentAlertResult.Queued;
    }

}
