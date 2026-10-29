using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.Models;
using HSMS.MVC.Service;
using HSMS.MVC.Service.Notifications;
using Microsoft.Extensions.Logging.Abstractions;

namespace HSMS.Attendance.Checks;

internal class TestClock : TimeProvider
{
    public DateTimeOffset Now { get; set; } = new(2026, 10, 28, 8, 0, 0, TimeSpan.Zero);
    public override DateTimeOffset GetUtcNow() => Now;
}

internal class Fixture
{
    public DateTime Day { get; } = new(2026, 10, 28);
    public TestClock Clock { get; } = new();
    public FakeRepository Repo { get; }
    public AttendanceService Service { get; }
    public Customer Student { get; }
    public User ParentUser { get; }
    public TimetableEntry Entry { get; }

    public Fixture(string role = "TEACHER", bool recorded = false)
    {
        var year = new AcademicYear("2026-2027", new(2026, 8, 1), new(2027, 7, 31));
        var semester = new Semester("HK1", "Học kỳ 1", new(2026, 10, 1), new(2026, 10, 31), 1);
        Set(semester, "Id", 300L);
        Set(semester, "AcademicYear", year);
        var classroom = new Class("10A1", 1, 1);
        var assignment = new TeachingAssignment(30, 200, 100, 300);
        Set(assignment, "Class", classroom);
        Set(assignment, "Subject", new Subject("TOAN", "Toán"));
        Set(assignment, "Semester", semester);
        var teacher = new Employee(1, "GV01");
        Set(teacher, "User", new User("teacher", "teacher@example.test", "unused", "Giáo viên mẫu"));
        Set(assignment, "Employee", teacher);
        Entry = new TimetableEntry(600, 500, 700, 3, semester.StartDate, semester.EndDate);
        Set(Entry, "Id", 400L);
        Set(Entry, "TeachingAssignment", assignment);
        Set(Entry, "TimeSlot", new TimeSlot("T1", 1, TimeSpan.FromHours(7), TimeSpan.FromHours(7.75)));
        Set(Entry, "Room", new Room("P1", "Phòng 1"));

        ParentUser = new User("parent", "parent@example.test", "unused", "Phụ huynh mẫu");
        Set(ParentUser, "Id", 2L);
        var parentRole = new Role("PARENT", "Phụ huynh");
        var userRole = new UserRole(2, 80);
        Set(userRole, "Role", parentRole);
        ParentUser.UserRoles.Add(userRole);
        var parent = new Customer(2);
        Set(parent, "Id", 20L);
        Set(parent, "User", ParentUser);
        Student = new Customer(3, studentCode: "HS01", parentId: 20);
        Set(Student, "Id", 10L);
        Set(Student, "User", new User("student", "student@example.test", "unused", "Học sinh mẫu"));
        Set(Student, "Parent", parent);

        Repo = new FakeRepository
        {
            User = new LoginResponseDto { Id = 1, Username = "test.user", FullName = "Người dùng mẫu", Roles = [role], EmployeeId = role == "TEACHER" ? 30 : 60, CustomerId = role == "PARENT" ? 20 : 10 },
            Entry = Entry,
            Students = [Student]
        };
        Repo.Semesters[10] = [new(300, "Học kỳ 1 · 2026-2027", semester.StartDate, semester.EndDate)];
        Repo.Subjects[(10, 300)] = [new(200, "Toán")];
        if (recorded)
        {
            var record = new AttendanceRecord(10, 400, 30, Day, "PRESENT");
            Set(record, "Id", 900L);
            Set(record, "Customer", Student);
            Set(record, "TimetableEntry", Entry);
            Repo.Records.Add(record);
        }
        Service = new AttendanceService(Repo, new SchoolTime(Clock), new NotificationDispatchSignal(), NullLogger<AttendanceService>.Instance);
    }

    public SaveAttendanceDto Input(string status = "PRESENT") => new()
    {
        TimetableEntryId = 400, Date = Day, ChangeReason = "Điều chỉnh theo xác nhận",
        Rows = [new() { CustomerId = 10, Status = status }]
    };

    public LeaveRequest ApprovedLeave()
    {
        var leave = new LeaveRequest(10, 20, "Nghỉ ốm", Day, Day);
        Set(leave, "Id", 50L);
        leave.Approve(new Employee(1, "GV01"));
        Repo.Leaves.Add(leave);
        return leave;
    }

    public static void Set(object target, string property, object? value) =>
        target.GetType().GetProperty(property)!.SetValue(target, value);

    public static AttendanceRecord Clone(AttendanceRecord source)
    {
        var result = (AttendanceRecord)Activator.CreateInstance(typeof(AttendanceRecord), nonPublic: true)!;
        foreach (var property in typeof(AttendanceRecord).GetProperties().Where(p => p.SetMethod != null))
            property.SetValue(result, property.GetValue(source));
        return result;
    }
}
