namespace HSMS.MVC.DTOs;

// ===== Dữ liệu Staff gửi lên (bước 2 trong diagram) =====
public class CreateAssignmentRequest
{
    public long EmployeeId { get; set; }   // giáo viên
    public long SubjectId { get; set; }    // môn
    public long ClassId { get; set; }      // lớp
    public long SemesterId { get; set; }   // học kỳ
    public long RoomId { get; set; }       // phòng
    public long TimeSlotId { get; set; }   // tiết học (đã có giờ bắt đầu/kết thúc)
    public int DayOfWeek { get; set; }     // 1..7 (Thứ Hai .. Chủ nhật), đồng bộ SchoolTime.Weekday
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

// ===== Kết quả nội bộ sau khi lưu xong (bước 19 "createAssignmentResult") =====
public class CreateAssignmentResult
{
    public long AssignmentId { get; set; }
    public long TimetableId { get; set; }
}

// ===== Dữ liệu trả về cho View (bước 19 "mapToResponseDto") =====
public class AssignmentResponseDto
{
    public bool Success { get; set; }
    public bool IsConflict { get; set; }
    public string Message { get; set; } = string.Empty;
    public long? AssignmentId { get; set; }
    public long? TimetableId { get; set; }
}

// ===== Dữ liệu cho dropdown ở form =====
public class OptionDto
{
    public long Id { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class FormOptionsDto
{
    public List<OptionDto> Employees { get; set; } = new();
    public List<OptionDto> Subjects { get; set; } = new();
    public List<OptionDto> Classes { get; set; } = new();
    public List<OptionDto> Semesters { get; set; } = new();
    public List<OptionDto> Rooms { get; set; } = new();
    public List<OptionDto> TimeSlots { get; set; } = new();
}

// ===== Dữ liệu để vẽ bảng thời khóa biểu =====
public class SlotDto
{
    public long Id { get; set; }
    public int PeriodNo { get; set; }
    public string TimeText { get; set; } = string.Empty; // "07:00 - 07:45"
}

public class TimetableItemDto
{
    public int DayOfWeek { get; set; }
    public long TimeSlotId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string RoomName { get; set; } = string.Empty;
}

public class WeeklyTimetableDto
{
    public DateTime WeekStart { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<SlotDto> Slots { get; set; } = new();
    public List<TimetableItemDto> Items { get; set; } = new();
}

