using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

public class Customer : IAccount
{
    public DateTime? LastLoginAt { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiry { get; private set; }
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
    public long? ParentId { get; private set; }
    public string? RelationshipType { get; private set; }
    public virtual Customer? Parent { get; private set; }
    public virtual ICollection<Customer> Children { get; private set; } = new List<Customer>();
    public virtual ICollection<StudentClass> StudentClasses { get; private set; } = new List<StudentClass>();
    public virtual ICollection<LeaveRequest> StudentLeaveRequests { get; private set; } = new List<LeaveRequest>();
    public virtual ICollection<LeaveRequest> RequestedLeaveRequests { get; private set; } = new List<LeaveRequest>();
    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; private set; } = new List<AttendanceRecord>();
    public virtual ICollection<StudentScore> StudentScores { get; private set; } = new List<StudentScore>();
    public virtual ICollection<ConductEvaluation> ConductEvaluations { get; private set; } = new List<ConductEvaluation>();
    public virtual ICollection<SemesterReport> SemesterReports { get; private set; } = new List<SemesterReport>();
    public virtual ICollection<TeacherFeedback> TeacherFeedbacks { get; private set; } = new List<TeacherFeedback>();
    public virtual ICollection<StudentTransfer> StudentTransfers { get; private set; } = new List<StudentTransfer>();
    public virtual ICollection<StudentAnnualReport> StudentAnnualReports { get; private set; } = new List<StudentAnnualReport>();
    public virtual ICollection<Notification> SentNotifications { get; private set; } = new List<Notification>();
    public virtual ICollection<Notification> ReceivedNotifications { get; private set; } = new List<Notification>();
    public virtual ICollection<MessageLog> ReceivedMessageLogs { get; private set; } = new List<MessageLog>();

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
        string? emergencyContact = null,
        long? parentId = null,
        string? relationshipType = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new AppError("Tên đăng nhập không được để trống.", "USERNAME_REQUIRED");

        if (string.IsNullOrWhiteSpace(email))
            throw new AppError("Email không được để trống.", "EMAIL_REQUIRED");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new AppError("Mật khẩu băm không được để trống.", "PASSWORD_REQUIRED");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new AppError("Họ và tên không được để trống.", "FULLNAME_REQUIRED");

        if (parentId.HasValue && parentId.Value <= 0)
            throw new AppError("Id phụ huynh không hợp lệ.", "INVALID_PARENT_ID");

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
        ParentId = parentId;
        RelationshipType = relationshipType?.Trim();
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
            throw new AppError("Họ và tên không được để trống.", "INVALID_FULLNAME");

        FullName = fullName.Trim();
        PhoneNumber = phoneNumber;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AssignParent(long parentId, string? relationshipType = null)
    {
        if (parentId <= 0)
            throw new AppError("Id phụ huynh không hợp lệ.", "INVALID_PARENT_ID");

        if (Id != 0 && parentId == Id)
            throw new AppError("Học sinh và phụ huynh không thể là cùng một người.", "SAME_CUSTOMER_RELATIONSHIP");

        ParentId = parentId;
        RelationshipType = relationshipType?.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveParent()
    {
        ParentId = null;
        RelationshipType = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsGuardianOf(Customer student) => student.ParentId == Id;

    public Customer GetProfile() => this;
    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new AppError("Mật khẩu mới không được để trống.", "PASSWORD_REQUIRED");

        PasswordHash = newPasswordHash;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public IReadOnlyList<string> GetActiveRoleCodes()
    {
        return CustomerRoles
            .Where(ur => ur.IsActive && ur.Role != null && ur.Role.IsActive)
            .Select(ur => ur.Role!.RoleCode)
            .Where(code => code is "STUDENT" or "PARENT")
            .Distinct()
            .ToList()
            .AsReadOnly();
    }

    public void SetPasswordResetToken(string token, DateTime expiry)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new AppError("Mã xác nhận đặt lại mật khẩu không hợp lệ.", "RESET_TOKEN_REQUIRED");

        PasswordResetToken = token;
        PasswordResetTokenExpiry = expiry;
    }

    public void ClearPasswordResetToken()
    {
        PasswordResetToken = null;
        PasswordResetTokenExpiry = null;
    }

    public bool IsPasswordResetTokenValid(string token)
    {
        return !string.IsNullOrWhiteSpace(PasswordResetToken)
            && PasswordResetToken == token
            && PasswordResetTokenExpiry.HasValue
            && PasswordResetTokenExpiry.Value > DateTime.UtcNow;
    }

}

