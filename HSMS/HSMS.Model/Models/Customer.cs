using HSMS.Model.Exceptions;

namespace HSMS.Model.Model;

public class Customer
{
    public long Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FullName { get; private set; } = null!;
    public string? PhoneNumber { get; private set; }
    public string? Gender { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Address { get; private set; }
    public string? AvatarUrl { get; private set; }
    public string? StudentCode { get; private set; }
    public DateTime? AdmissionDate { get; private set; }
    public string? Occupation { get; private set; }
    public string? EmergencyContact { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public virtual ICollection<CustomerRole> CustomerRoles { get; private set; } = new List<CustomerRole>();
    public virtual ICollection<CustomerRelationship> ParentRelationships { get; private set; } = new List<CustomerRelationship>();
    public virtual ICollection<CustomerRelationship> StudentRelationships { get; private set; } = new List<CustomerRelationship>();
    public virtual ICollection<StudentClass> StudentClasses { get; private set; } = new List<StudentClass>();
    public virtual ICollection<LeaveRequest> StudentLeaveRequests { get; private set; } = new List<LeaveRequest>();
    public virtual ICollection<LeaveRequest> RequestedLeaveRequests { get; private set; } = new List<LeaveRequest>();
    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; private set; } = new List<AttendanceRecord>();
    public virtual ICollection<StudentScore> StudentScores { get; private set; } = new List<StudentScore>();
    public virtual ICollection<ConductEvaluation> ConductEvaluations { get; private set; } = new List<ConductEvaluation>();
    public virtual ICollection<SemesterReport> SemesterReports { get; private set; } = new List<SemesterReport>();
    public virtual ICollection<TeacherFeedback> TeacherFeedbacks { get; private set; } = new List<TeacherFeedback>();
    public virtual ICollection<Notification> SentNotifications { get; private set; } = new List<Notification>();
    public virtual ICollection<Notification> ReceivedNotifications { get; private set; } = new List<Notification>();

    protected Customer() { }

    public Customer(
        string username,
        string email,
        string passwordHash,
        string fullName,
        string? phoneNumber = null,
        string? gender = null,
        DateTime? dateOfBirth = null,
        string? address = null,
        string? avatarUrl = null,
        string? studentCode = null,
        DateTime? admissionDate = null,
        string? occupation = null,
        string? emergencyContact = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new BusinessException("Tên đăng nhập không được để trống.", "USERNAME_REQUIRED");

        if (string.IsNullOrWhiteSpace(email))
            throw new BusinessException("Email không được để trống.", "EMAIL_REQUIRED");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new BusinessException("Mật khẩu băm không được để trống.", "PASSWORD_REQUIRED");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new BusinessException("Họ và tên không được để trống.", "FULLNAME_REQUIRED");

        Username = username.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Address = address;
        AvatarUrl = avatarUrl;
        StudentCode = studentCode;
        AdmissionDate = admissionDate;
        Occupation = occupation;
        EmergencyContact = emergencyContact;
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

    public Customer GetProfile() => this;
}
