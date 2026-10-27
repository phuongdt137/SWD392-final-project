using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

/// <summary>
/// Thực thể Khách hàng / Học sinh / Phụ huynh (Hồ sơ nghiệp vụ liên kết 1:1 với User).
/// </summary>
public class Customer
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public string? StudentCode { get; private set; }
    public string? Gender { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Address { get; private set; }
    public DateTime? AdmissionDate { get; private set; }
    public string? Occupation { get; private set; }
    public string? EmergencyContact { get; private set; }
    public long? ParentId { get; private set; }
    public string? RelationshipType { get; private set; }
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

    // Quan hệ cha - con (Parent - Student)
    public virtual Customer? Parent { get; private set; }
    public virtual ICollection<Customer> Children { get; private set; } = new List<Customer>();

    // Academic & School Relations
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

    protected Customer() { }

    public Customer(
        long userId,
        string? studentCode = null,
        DateTime? admissionDate = null,
        string? occupation = null,
        string? emergencyContact = null,
        long? parentId = null,
        string? relationshipType = null,
        string? gender = null,
        DateTime? dateOfBirth = null,
        string? address = null)
    {
        if (userId <= 0)
            throw new AppError("Id người dùng không hợp lệ.", "INVALID_USER_ID");

        if (parentId.HasValue && parentId.Value <= 0)
            throw new AppError("Id phụ huynh không hợp lệ.", "INVALID_PARENT_ID");

        UserId = userId;
        StudentCode = studentCode?.Trim();
        AdmissionDate = admissionDate;
        Occupation = occupation?.Trim();
        EmergencyContact = emergencyContact?.Trim();
        ParentId = parentId;
        RelationshipType = relationshipType?.Trim();
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Address = address;
    }

    public void UpdateStudentInfo(
        string? studentCode,
        DateTime? admissionDate,
        string? occupation,
        string? emergencyContact,
        string? gender,
        DateTime? dateOfBirth,
        string? address)
    {
        StudentCode = studentCode?.Trim();
        AdmissionDate = admissionDate;
        Occupation = occupation?.Trim();
        EmergencyContact = emergencyContact?.Trim();
        Gender = gender;
        DateOfBirth = dateOfBirth;
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

    /// <summary>
    /// Kiểm tra hồ sơ hiện tại có phải là phụ huynh (người giám hộ) của học sinh được truyền vào hay không.
    /// </summary>
    public bool IsGuardianOf(Customer student) => student != null && student.ParentId == Id;

    public void RemoveParent()
    {
        ParentId = null;
        RelationshipType = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
