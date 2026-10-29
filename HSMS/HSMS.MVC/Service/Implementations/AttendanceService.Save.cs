using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.Models;

namespace HSMS.MVC.Service;

public partial class AttendanceService
{
    /// <summary>Nạp kết quả, học sinh và đơn nghỉ; báo học sinh còn thiếu trạng thái trước khi xử lý lưu.</summary>
    private async Task<AttendanceSaveData> LoadSaveDataAsync(LoginResponseDto user, TimetableEntry entry,
        SaveAttendanceDto input, CancellationToken ct)
    {
        var date = input.Date.Date;
        var roster = await repo.GetStudentsAsync(entry.TeachingAssignment.ClassId, date, ct);
        var records = await repo.GetSheetAsync(entry.Id, date, ct);
        if (!user.Has("STAFF"))
        {
            foreach (var student in roster)
            {
                var row = input.Rows.FirstOrDefault(r => r.CustomerId == student.Id);
                if (string.IsNullOrWhiteSpace(row?.Status))
                    throw new AppError($"Học sinh {student.FullName} chưa được điểm danh.", "INCOMPLETE_ATTENDANCE");
            }
        }

        var students = roster.Concat(records.Select(r => r.Customer)).DistinctBy(s => s.Id).ToList();
        var leaves = await repo.GetLeavesAsync(students.Select(s => s.Id).ToArray(), date, ct);
        return new AttendanceSaveData
        {
            Records = records, Students = students, Leaves = leaves
        };
    }

    /// <summary>Xử lý từng dòng, tạo cảnh báo cần thiết và tổng hợp số thay đổi cùng tình trạng hàng chờ email.</summary>
    private (AttendanceSaveResult Result, bool HasEmail) ApplyAttendanceRows(LoginResponseDto user,
        TimetableEntry entry, SaveAttendanceDto input, AttendanceSaveData data)
    {
        var changed = 0;
        var missingParents = 0;
        var missingEmails = 0;
        var hasEmail = false;

        foreach (var row in input.Rows)
        {
            var audit = ApplyStudentResult(user, entry, input, row, data);
            if (audit == null) continue; // Kết quả không thay đổi.
            changed++;

            var record = audit.AttendanceRecord;
            if (record.Status == audit.OldStatus || !AttendanceStatuses.IsAlert(record.Status))
                continue;

            var student = data.Students.First(s => s.Id == row.CustomerId);
            var delivery = QueueParentAlert(student, entry, record, audit, user.Id);
            if (delivery == ParentAlertResult.MissingParent) missingParents++;
            else if (delivery == ParentAlertResult.MissingEmail) missingEmails++;
            else hasEmail = true;
        }

        return (new AttendanceSaveResult(changed, missingParents, missingEmails), hasEmail);
    }

    /// <summary>Tạo hoặc sửa kết quả một học sinh và ghi nhật ký; trả về null khi dữ liệu không thay đổi.</summary>
    private AttendanceAuditLog? ApplyStudentResult(LoginResponseDto user, TimetableEntry entry,
        SaveAttendanceDto input, AttendanceInputDto row, AttendanceSaveData data)
    {
        var existing = data.Records.FirstOrDefault(r => r.CustomerId == row.CustomerId);
        if (user.Has("STAFF") && existing == null)
            throw new AppError("Staff chỉ được sửa kết quả đã có, không được điểm danh lần đầu.", "STAFF_EDIT_ONLY", ErrorType.Forbidden);

        var status = AttendanceStatuses.Validate(row.Status);
        var comment = string.IsNullOrWhiteSpace(row.Comment) ? null : row.Comment.Trim();
        var leave = status == AttendanceStatuses.Excused
            ? data.Leaves.FirstOrDefault(l => l.Id == row.LeaveRequestId && l.StudentCustomerId == row.CustomerId)
            : null;
        if (data.Leaves.Any(l => l.StudentCustomerId == row.CustomerId) &&
            (status != AttendanceStatuses.Excused || leave == null))
            throw new AppError("Học sinh có đơn nghỉ đã duyệt phải ở trạng thái Vắng có phép và chọn đơn nghỉ hợp lệ.", "APPROVED_LEAVE_REQUIRED");

        var oldStatus = existing?.Status;
        var oldComment = existing?.Comment;
        var oldLeaveId = existing?.LeaveRequestId;
        if (existing != null && oldStatus == status && oldComment == comment && oldLeaveId == leave?.Id)
            return null;

        AttendanceRecord record;
        if (existing == null)
        {
            record = new AttendanceRecord(row.CustomerId, entry.Id, user.EmployeeId!.Value,
                input.Date.Date, status, comment, leave, time.UtcNow);
            repo.AddRecord(record);
        }
        else
        {
            record = existing;
            record.RecordResult(status, row.Comment, leave, user.EmployeeId!.Value, time.UtcNow);
        }

        var audit = new AttendanceAuditLog(record, oldStatus, oldComment, oldLeaveId,
            input.ChangeReason, user.EmployeeId!.Value, time.UtcNow);
        repo.AddAudit(audit);
        return audit;
    }

    // Chỉ chứa dữ liệu trong lần lưu này, không phải Model hay bảng database.
    private sealed class AttendanceSaveData
    {
        public List<AttendanceRecord> Records { get; init; } = [];
        public List<Customer> Students { get; init; } = [];
        public List<LeaveRequest> Leaves { get; init; } = [];
    }
}
