using HSMS.MVC.DTOs.Timetable;
using HSMS.MVC.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HSMS.MVC.ViewModels.Timetable;

/// <summary>
/// ViewModel chính cho trang xem thời khóa biểu (Staff, Teacher, Student, Parent).
/// </summary>
public class TimetableIndexViewModel
{
    public string UserRole { get; set; } = "STAFF"; // "STAFF", "TEACHER", "STUDENT", "PARENT"
    public string DisplayTitle { get; set; } = "Thời khóa biểu";
    public string? Subtitle { get; set; }

    public bool CanCreate { get; set; } // Chỉ STAFF, PRINCIPAL, ADMIN mới có quyền tạo/xếp lịch

    // Bộ lọc đang chọn
    public long? SelectedSemesterId { get; set; }
    public long? SelectedClassId { get; set; }
    public long? SelectedTeacherId { get; set; }
    public long? SelectedRoomId { get; set; }
    public long? SelectedStudentCustomerId { get; set; }

    // Danh sách cho Dropdown lọc
    public List<SelectListItem> Semesters { get; set; } = new();
    public List<SelectListItem> Classes { get; set; } = new();
    public List<SelectListItem> Teachers { get; set; } = new();
    public List<SelectListItem> Rooms { get; set; } = new();
    public List<SelectListItem> Children { get; set; } = new(); // Dành cho PHHS có nhiều con

    // Cấu trúc lưới TKB
    public List<TimeSlot> TimeSlots { get; set; } = new();
    public List<byte> DaysOfWeek { get; set; } = new() { 1, 2, 3, 4, 5, 6, 7 }; // 1: Thứ Hai -> 7: Chủ Nhật

    // Dữ liệu thời khóa biểu
    public List<TimetableItemDto> TimetableItems { get; set; } = new();

    /// <summary>
    /// Lấy các mục thời khóa biểu rơi vào thứ và tiết học cụ thể.
    /// </summary>
    public List<TimetableItemDto> GetItemsForCell(byte dayOfWeek, long timeSlotId)
    {
        return TimetableItems
            .Where(item => item.DayOfWeek == dayOfWeek && item.TimeSlotId == timeSlotId)
            .ToList();
    }

    public string GetDayName(byte day) => day switch
    {
        1 => "Thứ Hai",
        2 => "Thứ Ba",
        3 => "Thứ Tư",
        4 => "Thứ Năm",
        5 => "Thứ Sáu",
        6 => "Thứ Bảy",
        7 => "Chủ Nhật",
        _ => $"Thứ {day}"
    };
}

/// <summary>
/// ViewModel cho form tạo/xếp thời khóa biểu mới dành cho Staff.
/// </summary>
public class CreateTimetableViewModel
{
    public CreateTeachingAssignmentRequest Request { get; set; } = new();

    public List<SelectListItem> Teachers { get; set; } = new();
    public List<SelectListItem> Subjects { get; set; } = new();
    public List<SelectListItem> Classes { get; set; } = new();
    public List<SelectListItem> Semesters { get; set; } = new();
    public List<SelectListItem> Rooms { get; set; } = new();
    public List<SelectListItem> TimeSlots { get; set; } = new();
    public List<SelectListItem> DaysOfWeek { get; set; } = new()
    {
        new SelectListItem { Value = "1", Text = "Thứ Hai" },
        new SelectListItem { Value = "2", Text = "Thứ Ba" },
        new SelectListItem { Value = "3", Text = "Thứ Tư" },
        new SelectListItem { Value = "4", Text = "Thứ Năm" },
        new SelectListItem { Value = "5", Text = "Thứ Sáu" },
        new SelectListItem { Value = "6", Text = "Thứ Bảy" },
        new SelectListItem { Value = "7", Text = "Chủ Nhật" }
    };
}

