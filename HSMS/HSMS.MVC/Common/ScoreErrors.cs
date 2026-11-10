namespace HSMS.MVC.Common;

/// <summary>
/// Lỗi nhập điểm có nhiều dòng sai cùng lúc; Details chứa lỗi chi tiết từng học sinh.
/// </summary>
public class ScoreEntryValidationError : AppError
{
    public IReadOnlyList<string> Details { get; }

    public ScoreEntryValidationError(IEnumerable<string> details)
        : base(ScoreErrors.InvalidScoreEntries.Description, ScoreErrors.InvalidScoreEntries.Code, ErrorType.Validation)
    {
        Details = details.Distinct().ToList();
    }
}

/// <summary>
/// Danh mục các lỗi nghiệp vụ định nghĩa sẵn cho phân hệ Đánh giá & Điểm số (Assessment & Score).
/// </summary>
public static class ScoreErrors
{
    public static readonly AppError NotTeacher =
        new("Chỉ giáo viên bộ môn mới được thực hiện thao tác này.", "SCORE_NOT_TEACHER", ErrorType.Forbidden);

    public static readonly AppError NotYourClass =
        new("Bạn không phải là giáo viên phụ trách lớp/môn này.", "SCORE_NOT_YOUR_CLASS", ErrorType.Forbidden);

    public static readonly AppError NotStudentOrParent =
        new("Chỉ học sinh hoặc phụ huynh của học sinh mới được xem bảng điểm này.", "SCORE_ACCESS_DENIED", ErrorType.Forbidden);

    public static readonly AppError AssignmentNotFound =
        new("Không tìm thấy phân công giảng dạy.", "SCORE_ASSIGNMENT_NOT_FOUND", ErrorType.NotFound);

    public static readonly AppError AssessmentNotFound =
        new("Không tìm thấy bài kiểm tra.", "SCORE_ASSESSMENT_NOT_FOUND", ErrorType.NotFound);

    public static readonly AppError AssessmentTypeNotFound =
        new("Không tìm thấy loại bài kiểm tra.", "SCORE_ASSESSMENT_TYPE_NOT_FOUND", ErrorType.NotFound);

    public static readonly AppError ScoreNotFound =
        new("Không tìm thấy điểm của học sinh.", "SCORE_NOT_FOUND", ErrorType.NotFound);

    public static readonly AppError StudentNotFound =
        new("Không tìm thấy học sinh.", "SCORE_STUDENT_NOT_FOUND", ErrorType.NotFound);

    public static readonly AppError MaxEntriesExceeded =
        new("Đã đạt số lượng đầu điểm tối đa cho loại bài kiểm tra này.", "SCORE_MAX_ENTRIES_EXCEEDED", ErrorType.Validation);

    public static readonly AppError ScoreLocked =
        new("Bảng điểm đã bị khóa, không thể chỉnh sửa.", "SCORE_LOCKED", ErrorType.Conflict);

    public static readonly AppError InvalidScore =
        new("Điểm số phải nằm trong khoảng từ 0 đến 10.", "SCORE_INVALID_VALUE", ErrorType.Validation);

    public static readonly AppError ReasonRequired =
        new("Vui lòng nhập lý do sửa điểm.", "SCORE_REASON_REQUIRED", ErrorType.Validation);

    public static readonly AppError UseUpdateForExistingScore =
        new("Điểm đã được nhập. Vui lòng dùng chức năng sửa điểm để thay đổi (có ghi nhật ký).", "SCORE_USE_UPDATE", ErrorType.Validation);

    public static readonly AppError NoStudentsInClass =
        new("Lớp chưa có học sinh đang hoạt động.", "SCORE_NO_STUDENTS", ErrorType.Validation);

    public static readonly AppError StudentNotInClass =
        new("Học sinh không thuộc lớp của phân công này.", "SCORE_STUDENT_NOT_IN_CLASS", ErrorType.Validation);

    public static readonly AppError InvalidScoreEntries =
        new("Dữ liệu nhập điểm chưa hợp lệ, vui lòng kiểm tra lại:", "SCORE_INVALID_ENTRIES", ErrorType.Validation);

    public static readonly AppError NothingToSave =
        new("Chưa có điểm hoặc nhận xét mới nào để lưu.", "SCORE_NOTHING_TO_SAVE", ErrorType.Validation);

    public static readonly AppError ScoreUnchanged =
        new("Điểm mới trùng với điểm hiện tại, không có gì để cập nhật.", "SCORE_UNCHANGED", ErrorType.Validation);

    public static readonly AppError ConcurrentUpdate =
        new("Bảng điểm vừa được cập nhật bởi một thao tác khác. Vui lòng tải lại trang rồi thử lại.", "SCORE_CONCURRENT_UPDATE", ErrorType.Conflict);

    public static readonly AppError ConductedAtOutOfSemester =
        new("Ngày tổ chức phải nằm trong khoảng thời gian của học kỳ.", "SCORE_CONDUCTED_AT_OUT_OF_SEMESTER", ErrorType.Validation);

    public static readonly AppError NoLinkedStudent =
        new("Tài khoản phụ huynh chưa liên kết với học sinh nào.", "SCORE_NO_LINKED_STUDENT", ErrorType.Validation);

    public static readonly AppError SubjectNotScored =
        new("Môn học này được đánh giá bằng nhận xét, không có đầu điểm số.", "SCORE_SUBJECT_NOT_SCORED", ErrorType.Validation);

    public static readonly AppError SystemError =
        new("Hệ thống gặp sự cố khi xử lý điểm số. Vui lòng thử lại sau.", "SYSTEM_ERROR", ErrorType.Failure);
}
