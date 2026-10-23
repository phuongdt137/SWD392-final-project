using HSMS.Model.Exceptions;

namespace HSMS.Model.Model;

public class Employee
{
    public long Id { get; private set; }
    public string EmployeeCode { get; private set; } = null!;
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public string? Gender { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Address { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? Department { get; private set; }
    public string? Position { get; private set; }
    public string? Specialization { get; private set; }
    public int? MaxPeriodsPerWeek { get; private set; }
    public string? Qualification { get; private set; }
    public DateTime? HireDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public virtual ICollection<EmployeeRole> EmployeeRoles { get; private set; } = new List<EmployeeRole>();
    public virtual ICollection<Class> HomeroomClasses { get; private set; } = new List<Class>();
    public virtual ICollection<TeachingAssignment> TeachingAssignments { get; private set; } = new List<TeachingAssignment>();
    public virtual ICollection<LeaveRequest> ReviewedLeaveRequests { get; private set; } = new List<LeaveRequest>();
    public virtual ICollection<AttendanceRecord> RecordedAttendanceRecords { get; private set; } = new List<AttendanceRecord>();
    public virtual ICollection<ScoreAuditLog> ScoreAuditLogs { get; private set; } = new List<ScoreAuditLog>();
    public virtual ICollection<ConductEvaluation> ConductEvaluations { get; private set; } = new List<ConductEvaluation>();
    public virtual ICollection<SemesterReport> FinalizedSemesterReports { get; private set; } = new List<SemesterReport>();
    public virtual ICollection<Notification> SentNotifications { get; private set; } = new List<Notification>();
    public virtual ICollection<Notification> ReceivedNotifications { get; private set; } = new List<Notification>();
    public virtual ICollection<MessageLog> ReceivedMessageLogs { get; private set; } = new List<MessageLog>();
    public virtual ICollection<MessageLog> SentMessageLogs { get; private set; } = new List<MessageLog>();
    public virtual ICollection<StudentTransfer> RecordedStudentTransfers { get; private set; } = new List<StudentTransfer>();
    public virtual ICollection<StudentAnnualReport> FinalizedStudentAnnualReports { get; private set; } = new List<StudentAnnualReport>();

    protected Employee() { }

    public Employee(
        string employeeCode,
        string username,
        string email,
        string passwordHash,
        string fullName,
        string? phoneNumber = null,
        string? gender = null,
        DateTime? dateOfBirth = null,
        string? address = null,
        string? avatarUrl = null,
        string? department = null,
        string? position = null,
        string? specialization = null,
        string? qualification = null,
        DateTime? hireDate = null,
        int? maxPeriodsPerWeek = null)
    {
        if (string.IsNullOrWhiteSpace(employeeCode))
            throw new BusinessException("Mã nhân viên không được để trống.", "EMPLOYEE_CODE_REQUIRED");

        if (string.IsNullOrWhiteSpace(username))
            throw new BusinessException("Tên đăng nhập không được để trống.", "USERNAME_REQUIRED");

        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessException("Email không được để trống.", "EMAIL_REQUIRED");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new BusinessException("Mật khẩu băm không được để trống.", "PASSWORD_REQUIRED");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessException("Họ và tên không được để trống.", "FULLNAME_REQUIRED");

        if (maxPeriodsPerWeek.HasValue && maxPeriodsPerWeek.Value < 0)
            throw new BusinessException("Số tiết tối đa trong tuần phải >= 0.", "INVALID_MAX_PERIODS");

        EmployeeCode = employeeCode.Trim();
        Username = username.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Address = address;
        AvatarUrl = avatarUrl;
        Department = department;
        Position = position;
        Specialization = specialization;
        Qualification = qualification;
        HireDate = hireDate;
        MaxPeriodsPerWeek = maxPeriodsPerWeek;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public bool VerifyPassword(string rawPassword)
    {
        if (string.IsNullOrEmpty(rawPassword) || string.IsNullOrEmpty(PasswordHash))
            return false;

        return BCrypt.Net.BCrypt.Verify(rawPassword, PasswordHash);
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string fullName, string? phoneNumber, string? address)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessException("Họ và tên không được để trống.", "INVALID_FULLNAME");

        FullName = fullName.Trim();
        PhoneNumber = phoneNumber;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public Employee GetProfile() => this;

    public int CurrentWorkload(long semesterId)
    {
        return TeachingAssignments
            .Where(ta => ta.SemesterId == semesterId && ta.Status == "ACTIVE")
            .SelectMany(ta => ta.TimetableEntries)
            .Count(te => te.Status == "ACTIVE");
    }
}
