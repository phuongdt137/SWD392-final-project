using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

/// <summary>
/// Thực thể Nhân viên / Cán bộ / Giáo viên (Hồ sơ nghiệp vụ liên kết 1:1 với User).
/// </summary>
public class Employee
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public string EmployeeCode { get; private set; } = null!;
    public string? Gender { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Address { get; private set; }
    public string? Department { get; private set; }
    public string? Position { get; private set; }
    public string? Specialization { get; private set; }
    public int? MaxPeriodsPerWeek { get; private set; }
    public string? Qualification { get; private set; }
    public DateTime? HireDate { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation tới User (1:1)
    public virtual User User { get; private set; } = null!;

    // Helper properties ánh xạ sang User để giữ tương thích ngược
    public string FullName => User?.FullName ?? string.Empty;
    public string Username => User?.Username ?? string.Empty;
    public string Email => User?.Email ?? string.Empty;
    public string? PhoneNumber => User?.PhoneNumber;
    public string? AvatarUrl => User?.AvatarUrl;
    public bool IsActive => User?.IsActive ?? true;

    // Academic & School Relations
    public virtual ICollection<Class> HomeroomClasses { get; private set; } = new List<Class>();
    public virtual ICollection<TeachingAssignment> TeachingAssignments { get; private set; } = new List<TeachingAssignment>();
    public virtual ICollection<LeaveRequest> ReviewedLeaveRequests { get; private set; } = new List<LeaveRequest>();
    public virtual ICollection<AttendanceRecord> RecordedAttendanceRecords { get; private set; } = new List<AttendanceRecord>();
    public virtual ICollection<ScoreAuditLog> ScoreAuditLogs { get; private set; } = new List<ScoreAuditLog>();
    public virtual ICollection<ConductEvaluation> ConductEvaluations { get; private set; } = new List<ConductEvaluation>();
    public virtual ICollection<SemesterReport> FinalizedSemesterReports { get; private set; } = new List<SemesterReport>();
    public virtual ICollection<StudentTransfer> RecordedStudentTransfers { get; private set; } = new List<StudentTransfer>();
    public virtual ICollection<StudentAnnualReport> FinalizedStudentAnnualReports { get; private set; } = new List<StudentAnnualReport>();

    protected Employee() { }

    public Employee(
        long userId,
        string employeeCode,
        string? department = null,
        string? position = null,
        string? specialization = null,
        string? qualification = null,
        DateTime? hireDate = null,
        int? maxPeriodsPerWeek = null,
        string? gender = null,
        DateTime? dateOfBirth = null,
        string? address = null)
    {
        if (userId <= 0)
            throw new AppError("Id người dùng không hợp lệ.", "INVALID_USER_ID");

        if (string.IsNullOrWhiteSpace(employeeCode))
            throw new AppError("Mã nhân viên không được để trống.", "EMPLOYEE_CODE_REQUIRED");

        if (maxPeriodsPerWeek.HasValue && maxPeriodsPerWeek.Value < 0)
            throw new AppError("Số tiết tối đa trong tuần phải >= 0.", "INVALID_MAX_PERIODS");

        UserId = userId;
        EmployeeCode = employeeCode.Trim();
        Department = department?.Trim();
        Position = position?.Trim();
        Specialization = specialization?.Trim();
        Qualification = qualification?.Trim();
        HireDate = hireDate;
        MaxPeriodsPerWeek = maxPeriodsPerWeek;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Address = address;
    }

    public void UpdateEmployeeInfo(
        string? department,
        string? position,
        string? specialization,
        string? qualification,
        int? maxPeriodsPerWeek,
        string? gender,
        DateTime? dateOfBirth,
        string? address)
    {
        if (maxPeriodsPerWeek.HasValue && maxPeriodsPerWeek.Value < 0)
            throw new AppError("Số tiết tối đa trong tuần phải >= 0.", "INVALID_MAX_PERIODS");

        Department = department?.Trim();
        Position = position?.Trim();
        Specialization = specialization?.Trim();
        Qualification = qualification?.Trim();
        MaxPeriodsPerWeek = maxPeriodsPerWeek;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public int CurrentWorkload(long semesterId)
    {
        return TeachingAssignments
            .Where(ta => ta.SemesterId == semesterId && ta.Status == "ACTIVE")
            .SelectMany(ta => ta.TimetableEntries)
            .Count(te => te.Status == "ACTIVE");
    }
}
