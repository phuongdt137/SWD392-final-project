namespace HSMS.Model.Model;

public class Customer
{
    public long Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string? StudentCode { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public string? Occupation { get; set; }
    public string? EmergencyContact { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CustomerRole> CustomerRoles { get; set; } = new List<CustomerRole>();
    public virtual ICollection<CustomerRelationship> ParentRelationships { get; set; } = new List<CustomerRelationship>();
    public virtual ICollection<CustomerRelationship> StudentRelationships { get; set; } = new List<CustomerRelationship>();
    public virtual ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
    public virtual ICollection<LeaveRequest> StudentLeaveRequests { get; set; } = new List<LeaveRequest>();
    public virtual ICollection<LeaveRequest> RequestedLeaveRequests { get; set; } = new List<LeaveRequest>();
    public virtual ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public virtual ICollection<StudentScore> StudentScores { get; set; } = new List<StudentScore>();
    public virtual ICollection<ConductEvaluation> ConductEvaluations { get; set; } = new List<ConductEvaluation>();
    public virtual ICollection<SemesterReport> SemesterReports { get; set; } = new List<SemesterReport>();
    public virtual ICollection<TeacherFeedback> TeacherFeedbacks { get; set; } = new List<TeacherFeedback>();
    public virtual ICollection<Notification> SentNotifications { get; set; } = new List<Notification>();
    public virtual ICollection<Notification> ReceivedNotifications { get; set; } = new List<Notification>();
}
