using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.Models;

namespace HSMS.MVC.ViewModels.Attendance;

// Dòng lịch hiển thị: trạng thái suy ra theo ưu tiên khóa -> chưa đến giờ -> hoàn thành/chưa đủ.
public sealed record AttendanceLesson(TimetableEntry Entry, int Students, int Recorded, bool Locked, bool Started)
{
    public bool CanWrite { get; init; }
    public bool Completed => Students > 0 && Recorded >= Students;
    public string Status => Locked ? "LOCKED" : !Started ? "UPCOMING" : Completed ? "COMPLETED" : "PENDING";
    public string StatusLabel => Status switch { "LOCKED" => "Đã khóa", "UPCOMING" => "Chưa đến giờ", "COMPLETED" => "Đã điểm danh", _ => "Chưa điểm danh" };
}
// Hợp đồng cho partial phân trang; PageUrl giữ lại bộ lọc khi chuyển trang.
public sealed record AttendancePagination(int Page, int Pages, Func<int, string> PageUrl);
// ViewModel trang lịch: dữ liệu trang hiện tại, lựa chọn lọc và tổng thống kê.
public sealed record AttendanceIndex(DateTime Date, IReadOnlyList<AttendanceLesson> Lessons)
{
    public AttendanceScheduleFilter Filter { get; init; } = new();
    public AttendanceScheduleChoices Choices { get; init; } = new([], [], [], []);
    public int Total { get; init; }
    public int Classes { get; init; }
    public int Completed { get; init; }
    public int Pending { get; init; }
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / (double)AttendanceScheduleFilter.PageSize));
}
// Một học sinh, kết quả có thể chưa tồn tại và các đơn nghỉ được phép chọn.
public sealed record AttendanceStudent(Customer Student, AttendanceRecord? Record, IReadOnlyList<LeaveRequest> Leaves);
// ViewModel bảng ghi/sửa; View chỉ hiển thị theo quyền do Service tính.
public sealed class AttendanceSheet
{
    public TimetableEntry Entry { get; init; } = null!;
    public DateTime Date { get; init; }
    public string RosterToken { get; init; } = "";
    public IReadOnlyList<AttendanceStudent> Students { get; init; } = [];
    // Kết quả cũ không còn khớp danh sách hiện tại, giữ để đối chiếu thay vì tự xóa.
    public IReadOnlyList<AttendanceRecord> HistoricalRecords { get; init; } = [];
    public bool CanWrite { get; init; }
    public bool CanEditAfterDeadline { get; init; }
    public string? ReadOnlyReason { get; init; }
    // Giữ nội dung biểu mẫu bị lỗi để người dùng sửa tiếp mà không mất dữ liệu đã nhập.
    public SaveAttendanceDto? Attempt { get; set; }
}
// Lịch sử cho quản lý/giáo viên, gồm bản ghi đã lọc, tổng và lựa chọn tìm kiếm.
public sealed record AttendanceHistory(AttendanceFilter Filter, IReadOnlyList<AttendanceRecord> Records,
    int Total, AttendanceSummary Summary, AttendanceChoices Choices);
// Kết quả đang xem và toàn bộ lần thay đổi được phép tra cứu.
public sealed record AttendanceAudit(AttendanceRecord Record, IReadOnlyList<AttendanceAuditLog> Logs);
// Một buổi học suy từ lịch theo ngày; Record=null nghĩa là chưa có kết quả.
public sealed record StudentAttendanceSession(DateTime Date, TimetableEntry Entry, AttendanceRecord? Record);
// Dữ liệu trang học sinh/phụ huynh: chọn con, học kỳ, lớp/môn và phân trang các buổi học.
public sealed class StudentAttendanceHistory
{
    public bool IsParent { get; init; }
    public IReadOnlyList<StudentChoice> Students { get; init; } = [];
    public long? StudentId { get; init; }
    public string? StudentName { get; init; }
    public DateTime LocalNow { get; init; }
    public IReadOnlyList<Semester> Semesters { get; init; } = [];
    public IReadOnlyList<TeachingAssignment> Assignments { get; init; } = [];
    public long? SemesterId { get; init; }
    public TeachingAssignment? Assignment { get; init; }
    public IReadOnlyList<StudentAttendanceSession> Sessions { get; init; } = [];
    public int Recorded { get; init; }
    public int Absent { get; init; }
    public int Unmarked { get; init; }
    public int Upcoming { get; init; }
    // Vắng gồm ABSENT và EXCUSED; mẫu số chỉ là các buổi đã có kết quả.
    public decimal? AbsenceRate => Recorded == 0 ? null : Math.Round(100m * Absent / Recorded, 1);
    public int Total { get; init; }
    public int Page { get; init; } = 1;
    public const int PageSize = 30;
}
