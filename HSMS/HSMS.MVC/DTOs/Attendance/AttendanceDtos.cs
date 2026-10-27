using System.ComponentModel.DataAnnotations;
using HSMS.MVC.Models;

namespace HSMS.MVC.DTOs.Attendance;

// Dữ liệu POST từ biểu mẫu; danh tính người ghi lấy từ server, không nằm trong DTO.
public sealed class SaveAttendanceDto
{
    [Range(1, long.MaxValue)] public long TimetableEntryId { get; set; }
    public DateTime Date { get; set; }
    // Đối chiếu lịch và danh sách lớp với thời điểm mở bảng để tránh lưu trên danh sách cũ.
    [Required] public string RosterToken { get; set; } = "";
    // Bắt buộc ở Service khi sửa kết quả đã tồn tại; giới hạn độ dài được kiểm tra từ MVC.
    [StringLength(1000)] public string? ChangeReason { get; set; }
    public List<AttendanceInput> Rows { get; set; } = [];
}
// Một dòng học sinh gửi lên; Version=null khi chưa có kết quả, có GUID khi sửa bản ghi cũ.
public sealed class AttendanceInput
{
    public long CustomerId { get; set; }
    [Required] public string Status { get; set; } = "";
    [StringLength(1000)] public string? Comment { get; set; }
    public long? LeaveRequestId { get; set; }
    public Guid? Version { get; set; }
}
// Bộ lọc tra cứu; học sinh/phụ huynh dùng thêm học kỳ và phân công lớp/môn.
public sealed class AttendanceFilter
{
    public long? SemesterId { get; set; }
    public long? TeachingAssignmentId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public long? StudentId { get; set; }
    public long? ClassId { get; set; }
    public long? SubjectId { get; set; }
    public string? Status { get; set; }
    public int Page { get; set; } = 1;
}
// Changed là số kết quả thực sự đổi; MissingParents đếm thay đổi cần báo nhưng thiếu phụ huynh hợp lệ.
public sealed record SaveAttendanceResult(int Changed, int MissingParents);
// Cặp ID/tên nhỏ gọn cho ô chọn; ID mang nghĩa theo ngữ cảnh (học sinh, lớp, môn...).
public sealed record StudentChoice(long Id, string Name);
public sealed record AttendanceChoices(IReadOnlyList<StudentChoice> Students, IReadOnlyList<StudentChoice> Classes, IReadOnlyList<StudentChoice> Subjects);
// Bộ lọc lịch theo ngày, khối, lớp, môn, giáo viên và trạng thái; phân trang 20 tiết/trang.
public sealed class AttendanceScheduleFilter
{
    public DateTime? Date { get; set; }
    public long? GradeLevelId { get; set; }
    public long? ClassId { get; set; }
    public long? SubjectId { get; set; }
    public long? TeacherId { get; set; }
    public string? Status { get; set; }
    [StringLength(100)] public string? Query { get; set; }
    public int Page { get; set; } = 1;
    public const int PageSize = 20;
}
// Các lựa chọn ô lọc được lấy trong phạm vi lịch mà tài khoản được xem.
public sealed record AttendanceScheduleChoices(IReadOnlyList<StudentChoice> Grades, IReadOnlyList<StudentChoice> Classes,
    IReadOnlyList<StudentChoice> Subjects, IReadOnlyList<StudentChoice> Teachers);
// Dữ liệu một tiết từ Repository: ngữ cảnh lịch, sĩ số và số học sinh đã ghi nhận.
public sealed record AttendanceScheduleRow(TimetableEntry Entry, int Students, int Recorded);
// Kết quả phân trang và thống kê sau lọc; Service chuyển thành ViewModel cho màn hình.
public sealed record AttendanceScheduleResult(IReadOnlyList<AttendanceScheduleRow> Rows, int Total, int Classes,
    int Completed, int Pending, int Page, AttendanceScheduleChoices Choices);
// Thống kê kết quả đã ghi; Rate tính tỷ lệ có mặt/đi muộn trên số đã ghi, không tính dòng chưa ghi.
public sealed record AttendanceSummary(int Present, int Absent, int Late, int Excused, int Pending = 0)
{
    public int Recorded => Present + Absent + Late + Excused;
    public decimal? Rate => Recorded == 0 ? null : Math.Round(100m * (Present + Late) / Recorded, 1);
}
