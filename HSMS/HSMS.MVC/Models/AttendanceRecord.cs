using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

// Kết quả của một học sinh trong một tiết học vào một ngày; chưa có bản ghi là chưa ghi nhận.
public class AttendanceRecord
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    // Người ghi lần đầu được giữ riêng với người cập nhật gần nhất.
    public long RecordedByEmployeeId { get; private set; }
    public long? LeaveRequestId { get; private set; }
    public long TimetableEntryId { get; private set; }
    public DateTime AttendanceDate { get; private set; }
    public string Status { get; private set; } = null!;
    public string? Comment { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public long? UpdatedByEmployeeId { get; private set; }
    public virtual Employee? UpdatedByEmployee { get; private set; }

    public virtual Customer Customer { get; private set; } = null!;
    public virtual Employee RecordedByEmployee { get; private set; } = null!;
    public virtual LeaveRequest? LeaveRequest { get; private set; }
    public virtual TimetableEntry TimetableEntry { get; private set; } = null!;

    // Constructor rỗng dành cho EF Core khôi phục entity từ database.
    protected AttendanceRecord() { }

    // Tạo kết quả hợp lệ về dữ liệu cơ bản; Service chịu trách nhiệm quyền và danh sách lớp.
    public AttendanceRecord(long customerId, long timetableEntryId, long recordedByEmployeeId, DateTime attendanceDate, string status, string? comment = null, LeaveRequest? leave = null, DateTime? createdAtUtc = null)
    {
        if (customerId <= 0)
            throw new AppError("Id học sinh không hợp lệ.", "INVALID_CUSTOMER_ID");

        if (timetableEntryId <= 0)
            throw new AppError("Id thời khóa biểu không hợp lệ.", "INVALID_TIMETABLE_ENTRY_ID");

        if (recordedByEmployeeId <= 0)
            throw new AppError("Id cán bộ điểm danh không hợp lệ.", "INVALID_EMPLOYEE_ID");

        if (attendanceDate == default)
            throw new AppError("Ngày điểm danh không hợp lệ.", "INVALID_ATTENDANCE_DATE");

        CustomerId = customerId;
        TimetableEntryId = timetableEntryId;
        RecordedByEmployeeId = recordedByEmployeeId;
        AttendanceDate = attendanceDate.Date;
        SetResult(status, comment, leave);
        CreatedAt = createdAtUtc ?? DateTime.UtcNow;
    }

    // Mọi lần sửa đi qua đây để luôn lưu người và thời điểm cập nhật.
    public void RecordResult(string status, string? comment, LeaveRequest? leave, long employeeId, DateTime utcNow)
    {
        if (employeeId <= 0)
            throw new AppError("Id người cập nhật không hợp lệ.", "INVALID_EMPLOYEE_ID");

        SetResult(status, comment, leave);
        UpdatedAt = utcNow;
        UpdatedByEmployeeId = employeeId;
    }

    // Dùng chung quy tắc cho cả tạo mới và chỉnh sửa, kiểm tra xong mới đổi dữ liệu.
    private void SetResult(string status, string? comment, LeaveRequest? leave)
    {
        status = AttendanceStatuses.Validate(status);
        if (comment?.Length > 1000)
            throw new AppError("Ghi chú tối đa 1.000 ký tự.", "COMMENT_TOO_LONG");

        if (status == AttendanceStatuses.Excused)
        {
            if (leave == null)
                throw new AppError("Vắng có phép cần đơn nghỉ được duyệt.", "LEAVE_REQUIRED");

            if (leave.Id <= 0)
                throw new AppError("Đơn nghỉ phải tồn tại trong hệ thống.", "INVALID_LEAVE_REQUEST_ID");

            if (leave.StudentCustomerId != CustomerId)
                throw new AppError("Đơn nghỉ không thuộc học sinh này.", "LEAVE_STUDENT_MISMATCH");

            if (leave.Status != "APPROVED")
                throw new AppError("Đơn xin nghỉ phép chưa được duyệt.", "LEAVE_NOT_APPROVED");

            if (AttendanceDate < leave.StartDate || AttendanceDate > leave.EndDate)
                throw new AppError("Ngày điểm danh không nằm trong khoảng thời gian nghỉ phép của đơn.", "DATE_OUT_OF_LEAVE_RANGE");
        }

        Status = status;
        Comment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        LeaveRequest = status == AttendanceStatuses.Excused ? leave : null;
        LeaveRequestId = LeaveRequest?.Id;
    }
}
