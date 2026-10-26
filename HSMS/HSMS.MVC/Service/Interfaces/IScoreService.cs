using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Score;

namespace HSMS.MVC.Service;

/// <summary>
/// Giao diện xử lý nghiệp vụ Đánh giá & Điểm số (Assessment & Score).
/// Các phương thức nhận userId của người đang đăng nhập để kiểm tra quyền.
/// </summary>
public interface IScoreService
{
    /// <summary>
    /// Lấy các phân công giảng dạy đang hoạt động của giáo viên, kèm danh sách bài kiểm tra.
    /// </summary>
    Task<ServiceResult<IReadOnlyList<TeachingAssignmentSummaryDto>>> GetMyTeachingAssignmentsAsync(long userId);

    /// <summary>
    /// Lấy danh sách loại bài kiểm tra (dùng cho form tạo bài kiểm tra).
    /// </summary>
    Task<ServiceResult<IReadOnlyList<AssessmentTypeDto>>> GetAssessmentTypesAsync();

    /// <summary>
    /// Tạo bài kiểm tra mới cho phân công của giáo viên (UC01).
    /// </summary>
    Task<ServiceResult<long>> CreateAssessmentAsync(CreateAssessmentDto dto, long userId);

    /// <summary>
    /// Lấy bảng điểm của một bài kiểm tra: mỗi học sinh đang học lớp một dòng (UC02).
    /// </summary>
    Task<ServiceResult<ScoreSheetDto>> GetScoreSheetAsync(long assessmentId, long userId);

    /// <summary>
    /// Nhập điểm lần đầu cho học sinh. Kiểm tra toàn bộ dữ liệu trước khi lưu (UC02).
    /// </summary>
    Task<ServiceResult> EnterScoresAsync(EnterScoresDto dto, long userId);

    /// <summary>
    /// Sửa điểm đã nhập, bắt buộc có lý do và ghi nhật ký (UC03).
    /// </summary>
    Task<ServiceResult<StudentScoreDto>> UpdateScoreAsync(UpdateScoreDto dto, long userId, string? ipAddress);

    /// <summary>
    /// Khóa toàn bộ điểm của bài kiểm tra (UC04).
    /// </summary>
    Task<ServiceResult<LockResultDto>> LockScoresAsync(long assessmentId, long userId);

    /// <summary>
    /// Lấy nhật ký chỉnh sửa của một điểm số.
    /// </summary>
    Task<ServiceResult<IReadOnlyList<ScoreAuditLogDto>>> GetScoreHistoryAsync(long scoreId, long userId);

    /// <summary>
    /// Xem bảng điểm theo học kỳ của học sinh, có tính điểm trung bình có trọng số theo môn (UC05).
    /// Học sinh xem của chính mình; phụ huynh xem của con.
    /// </summary>
    Task<ServiceResult<StudentScoreReportDto>> GetStudentScoresAsync(long? studentCustomerId, long? semesterId, long userId);
}
