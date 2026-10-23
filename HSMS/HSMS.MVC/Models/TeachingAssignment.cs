using HSMS.MVC.Exceptions;

namespace HSMS.MVC.Models;

public class TeachingAssignment
{
    public long Id { get; private set; }
    public long EmployeeId { get; private set; }
    public long SubjectId { get; private set; }
    public long ClassId { get; private set; }
    public long SemesterId { get; private set; }
    public string Status { get; private set; } = "ACTIVE";

    public virtual Employee Employee { get; private set; } = null!;
    public virtual Subject Subject { get; private set; } = null!;
    public virtual Class Class { get; private set; } = null!;
    public virtual Semester Semester { get; private set; } = null!;
    public virtual ICollection<TimetableEntry> TimetableEntries { get; private set; } = new List<TimetableEntry>();
    public virtual ICollection<Assessment> Assessments { get; private set; } = new List<Assessment>();
    public virtual ICollection<TeacherFeedback> TeacherFeedbacks { get; private set; } = new List<TeacherFeedback>();

    protected TeachingAssignment() { }

    public TeachingAssignment(long employeeId, long subjectId, long classId, long semesterId, string status = "ACTIVE")
    {
        if (employeeId <= 0)
            throw new BusinessException("Id giáo viên không hợp lệ.", "INVALID_EMPLOYEE_ID");

        if (subjectId <= 0)
            throw new BusinessException("Id môn học không hợp lệ.", "INVALID_SUBJECT_ID");

        if (classId <= 0)
            throw new BusinessException("Id lớp học không hợp lệ.", "INVALID_CLASS_ID");

        if (semesterId <= 0)
            throw new BusinessException("Id học kỳ không hợp lệ.", "INVALID_SEMESTER_ID");

        EmployeeId = employeeId;
        SubjectId = subjectId;
        ClassId = classId;
        SemesterId = semesterId;
        Status = string.IsNullOrWhiteSpace(status) ? "ACTIVE" : status.Trim().ToUpperInvariant();
    }

    public void Activate() => Status = "ACTIVE";
    public void Cancel() => Status = "CANCELLED";
    public int TotalPeriodsPerWeek() => TimetableEntries.Count(te => te.Status == "ACTIVE");
    public TeachingAssignment GetAssignments() => this;
}
