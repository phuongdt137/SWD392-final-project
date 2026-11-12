using System.ComponentModel.DataAnnotations;

namespace HSMS.MVC.DTOs.Timetable;

/// <summary>
/// DTO gửi lên để tạo phân công giảng dạy và xếp thời khóa biểu.
/// Phù hợp với Sequence Diagram (Actor -> View -> Controller -> Service -> Repository).
/// </summary>
public class CreateTeachingAssignmentRequest
{
    [Required(ErrorMessage = "Vui lòng chọn giáo viên.")]
    [Range(1, long.MaxValue, ErrorMessage = "Giáo viên không hợp lệ.")]
    public long TeacherId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn môn học.")]
    [Range(1, long.MaxValue, ErrorMessage = "Môn học không hợp lệ.")]
    public long SubjectId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn lớp học.")]
    [Range(1, long.MaxValue, ErrorMessage = "Lớp học không hợp lệ.")]
    public long ClassId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn học kỳ.")]
    [Range(1, long.MaxValue, ErrorMessage = "Học kỳ không hợp lệ.")]
    public long SemesterId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn phòng học.")]
    [Range(1, long.MaxValue, ErrorMessage = "Phòng học không hợp lệ.")]
    public long RoomId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn tiết học.")]
    [Range(1, long.MaxValue, ErrorMessage = "Tiết học không hợp lệ.")]
    public long TimeSlotId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn thứ trong tuần.")]
    [Range(1, 7, ErrorMessage = "Thứ trong tuần phải từ 1 (Thứ Hai) đến 7 (Chủ Nhật).")]
    public byte DayOfWeek { get; set; } // 1: Thứ Hai, ..., 7: Chủ Nhật

    [DataType(DataType.Date)]
    public DateTime? EffectiveFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EffectiveTo { get; set; }
}

/// <summary>
/// Kết quả kiểm tra dữ liệu hợp lệ từ Repository.
/// </summary>
public class ValidationResultDto
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();

    public static ValidationResultDto Valid() => new() { IsValid = true };
    public static ValidationResultDto Invalid(params string[] errors) => new() { IsValid = false, Errors = errors.ToList() };
    public static ValidationResultDto Invalid(IEnumerable<string> errors) => new() { IsValid = false, Errors = errors.ToList() };
}

/// <summary>
/// Kết quả sau khi tạo TeachingAssignment và TimetableEntry thành công.
/// </summary>
public class CreateAssignmentResult
{
    public long AssignmentId { get; set; }
    public long TimetableId { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;

    public CreateAssignmentResult() { }

    public CreateAssignmentResult(long assignmentId, long timetableId, bool isSuccess = true, string message = "Tạo phân công và lịch dạy thành công.")
    {
        AssignmentId = assignmentId;
        TimetableId = timetableId;
        IsSuccess = isSuccess;
        Message = message;
    }
}

/// <summary>
/// DTO trả về cho Client theo Sequence Diagram (Step 20: mapToResponseDto).
/// </summary>
public class TeachingAssignmentResponseDto
{
    public long AssignmentId { get; set; }
    public long TimetableId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string SlotCode { get; set; } = string.Empty;
    public byte DayOfWeek { get; set; }
    public string DayOfWeekName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Status { get; set; } = "ACTIVE";
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// DTO biểu diễn một ô/mục trên thời khóa biểu.
/// </summary>
public class TimetableItemDto
{
    public long Id { get; set; }
    public long TeachingAssignmentId { get; set; }
    public byte DayOfWeek { get; set; }
    public string DayOfWeekName => DayOfWeek switch
    {
        1 => "Thứ Hai",
        2 => "Thứ Ba",
        3 => "Thứ Tư",
        4 => "Thứ Năm",
        5 => "Thứ Sáu",
        6 => "Thứ Bảy",
        7 => "Chủ Nhật",
        _ => $"Thứ {DayOfWeek}"
    };
    public long TimeSlotId { get; set; }
    public string SlotCode { get; set; } = string.Empty;
    public int PeriodNo { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public long SubjectId { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public long TeacherId { get; set; }
    public string TeacherCode { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public long ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public long RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;
    public string? RoomName { get; set; }
    public long SemesterId { get; set; }
    public string SemesterCode { get; set; } = string.Empty;
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string Status { get; set; } = "ACTIVE";
}

/// <summary>
/// DTO lọc thời khóa biểu theo tiêu chí.
/// </summary>
public class TimetableFilterDto
{
    public long? SemesterId { get; set; }
    public long? ClassId { get; set; }
    public long? TeacherId { get; set; }
    public long? RoomId { get; set; }
    public long? StudentCustomerId { get; set; }
}

