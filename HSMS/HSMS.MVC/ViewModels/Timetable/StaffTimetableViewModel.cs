using HSMS.MVC.DTOs;

namespace HSMS.MVC.ViewModels;

// Model cho trang Staff xem lịch (có bộ lọc theo lớp / giáo viên)
public class StaffTimetableViewModel
{
    public FormOptionsDto Options { get; set; } = new();
    public long? ClassId { get; set; }
    public long? EmployeeId { get; set; }
    public WeeklyTimetableDto Timetable { get; set; } = new();
}
