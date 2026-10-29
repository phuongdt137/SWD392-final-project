using HSMS.MVC.DTOs.Timetable;
using HSMS.MVC.Models;

namespace HSMS.MVC.Repositories.Interfaces;

/// <summary>
/// Repository xử lý dữ liệu cho phân hệ Thời khóa biểu (Timetable).
/// Tuân thủ theo Sequence Diagram (TimetableRepo <-> Database).
/// </summary>
public interface ITimetableRepository
{
    /// <summary>
    /// Kiểm tra tính hợp lệ của các trường và tham chiếu khóa ngoại trong CSDL (Step 4 & 5).
    /// </summary>
    Task<ValidationResultDto> ValidateRequestAsync(CreateTeachingAssignmentRequest request);

    /// <summary>
    /// Kiểm tra xung đột lịch dạy (Step 7 & 8: findScheduleConflict).
    /// Kiểm tra: giáo viên đã có tiết chưa, lớp đã có tiết chưa, phòng học đã bị chiếm chưa.
    /// </summary>
    Task<bool> FindScheduleConflictAsync(
        long teacherId,
        long classId,
        long roomId,
        byte dayOfWeek,
        long timeSlotId,
        DateTime? effectiveFrom,
        DateTime? effectiveTo,
        long? excludeTimetableId = null);

    /// <summary>
    /// Lưu phân công giảng dạy TeachingAssignment (Step 11 & 12).
    /// Nếu phân công đã tồn tại cho bộ (giáo viên, môn học, lớp, học kỳ) thì tái sử dụng ID đó.
    /// </summary>
    Task<long> SaveAssignmentAsync(TeachingAssignment assignment);

    /// <summary>
    /// Lưu một mục thời khóa biểu TimetableEntry (Step 15 & 16).
    /// </summary>
    Task<long> SaveTimetableEntryAsync(TimetableEntry entry);

    /// <summary>
    /// Lấy danh sách thời khóa biểu theo bộ lọc.
    /// </summary>
    Task<IReadOnlyList<TimetableItemDto>> GetTimetableItemsAsync(TimetableFilterDto filter);

    /// <summary>
    /// Lấy thời khóa biểu theo giáo viên.
    /// </summary>
    Task<IReadOnlyList<TimetableItemDto>> GetByTeacherAsync(long employeeId, long? semesterId = null);

    /// <summary>
    /// Lấy thời khóa biểu theo lớp học.
    /// </summary>
    Task<IReadOnlyList<TimetableItemDto>> GetByClassAsync(long classId, long? semesterId = null);

    /// <summary>
    /// Lấy thời khóa biểu của học sinh theo CustomerId.
    /// </summary>
    Task<IReadOnlyList<TimetableItemDto>> GetByStudentAsync(long customerId, long? semesterId = null);

    /// <summary>
    /// Lấy danh sách các lớp học đang hoạt động.
    /// </summary>
    Task<IReadOnlyList<Class>> GetActiveClassesAsync();

    /// <summary>
    /// Lấy danh sách giáo viên đang hoạt động.
    /// </summary>
    Task<IReadOnlyList<Employee>> GetActiveTeachersAsync();

    /// <summary>
    /// Lấy danh sách môn học.
    /// </summary>
    Task<IReadOnlyList<Subject>> GetSubjectsAsync();

    /// <summary>
    /// Lấy danh sách học kỳ.
    /// </summary>
    Task<IReadOnlyList<Semester>> GetSemestersAsync();

    /// <summary>
    /// Lấy học kỳ hiện tại hoặc gần nhất.
    /// </summary>
    Task<Semester?> GetCurrentSemesterAsync();

    /// <summary>
    /// Lấy danh sách phòng học đang hoạt động.
    /// </summary>
    Task<IReadOnlyList<Room>> GetActiveRoomsAsync();

    /// <summary>
    /// Lấy danh sách tiết học, sắp xếp theo PeriodNo.
    /// </summary>
    Task<IReadOnlyList<TimeSlot>> GetTimeSlotsAsync();

    /// <summary>
    /// Tìm phân công giảng dạy theo ID.
    /// </summary>
    Task<TeachingAssignment?> GetAssignmentByIdAsync(long id);

    /// <summary>
    /// Tìm mục thời khóa biểu theo ID.
    /// </summary>
    Task<TimetableEntry?> GetTimetableEntryByIdAsync(long id);

    /// <summary>
    /// Lấy danh sách học sinh (con) của phụ huynh theo Parent CustomerId.
    /// </summary>
    Task<IReadOnlyList<Customer>> GetChildrenByParentCustomerIdAsync(long parentCustomerId);

    /// <summary>
    /// Lấy lớp học hiện tại của học sinh theo CustomerId.
    /// </summary>
    Task<Class?> GetCurrentClassOfStudentAsync(long customerId);
}

