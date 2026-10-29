using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.DTOs.Timetable;
using HSMS.MVC.ViewModels.Timetable;

namespace HSMS.MVC.Service.Interfaces;

/// <summary>
/// Service xử lý nghiệp vụ cho phân hệ Thời khóa biểu (TimetableService).
/// Tuân thủ theo Sequence Diagram (TimetableController <-> TimetableService <-> TimetableRepo).
/// </summary>
public interface ITimetableService
{
    /// <summary>
    /// Thực hiện tạo phân công giảng dạy và xếp lịch học theo Sequence Diagram (Step 3 đến 19).
    /// </summary>
    Task<ServiceResult<CreateAssignmentResult>> CreateAssignmentAndTimetableAsync(CreateTeachingAssignmentRequest request);

    /// <summary>
    /// Chuẩn bị dữ liệu hiển thị thời khóa biểu dựa theo quyền và thông tin người dùng đăng nhập.
    /// - Staff/Principal: xem toàn trường hoặc lọc theo lớp/giáo viên.
    /// - Teacher: xem lịch dạy của chính mình (chỉ xem).
    /// - Student: xem lịch học của lớp mình (chỉ xem).
    /// - Parent: xem lịch học của con mình (chỉ xem).
    /// </summary>
    Task<ServiceResult<TimetableIndexViewModel>> GetTimetableIndexViewModelAsync(TimetableFilterDto filter, LoginResponseDto currentUser);

    /// <summary>
    /// Lấy dữ liệu danh mục để đổ vào form tạo thời khóa biểu cho Staff.
    /// </summary>
    Task<CreateTimetableViewModel> GetCreateTimetableViewModelAsync(CreateTeachingAssignmentRequest? initialRequest = null);

    /// <summary>
    /// Chuyển đổi kết quả tạo thành TeachingAssignmentResponseDto (Step 20: mapToResponseDto).
    /// </summary>
    Task<TeachingAssignmentResponseDto> MapToResponseDtoAsync(CreateAssignmentResult result);
}

