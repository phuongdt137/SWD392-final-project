namespace HSMS.Model.Model;

public class Employee
{
    public long Id { get; set; }
    public string EmployeeCode { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Department { get; set; }
    public string? Position { get; set; }
    public string? Specialization { get; set; }
    public int? MaxPeriodsPerWeek { get; set; }
    public string? Qualification { get; set; }
    public DateTime? HireDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<EmployeeRole> EmployeeRoles { get; set; } = new List<EmployeeRole>();
    public virtual ICollection<Class> HomeroomClasses { get; set; } = new List<Class>();
    public virtual ICollection<TeachingAssignment> TeachingAssignments { get; set; } = new List<TeachingAssignment>();
    public virtual ICollection<LeaveRequest> ReviewedLeaveRequests { get; set; } = new List<LeaveRequest>();
    public virtual ICollection<AttendanceRecord> RecordedAttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public virtual ICollection<ScoreAuditLog> ScoreAuditLogs { get; set; } = new List<ScoreAuditLog>();
    public virtual ICollection<ConductEvaluation> ConductEvaluations { get; set; } = new List<ConductEvaluation>();
    public virtual ICollection<SemesterReport> FinalizedSemesterReports { get; set; } = new List<SemesterReport>();
    public virtual ICollection<Notification> SentNotifications { get; set; } = new List<Notification>();
    public virtual ICollection<Notification> ReceivedNotifications { get; set; } = new List<Notification>();
}
