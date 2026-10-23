using HSMS.MVC.Exceptions;

namespace HSMS.MVC.Models;

public class Class
{
    public long Id { get; private set; }
    public string ClassName { get; private set; } = null!;
    public long? HomeroomEmployeeId { get; private set; }
    public long AcademicYearId { get; private set; }
    public long GradeLevelId { get; private set; }
    public string Status { get; private set; } = "ACTIVE";

    public virtual Employee? HomeroomEmployee { get; private set; }
    public virtual AcademicYear AcademicYear { get; private set; } = null!;
    public virtual GradeLevel GradeLevel { get; private set; } = null!;
    public virtual ICollection<StudentClass> StudentClasses { get; private set; } = new List<StudentClass>();
    public virtual ICollection<TeachingAssignment> TeachingAssignments { get; private set; } = new List<TeachingAssignment>();
    public virtual ICollection<StudentTransfer> TransfersFrom { get; private set; } = new List<StudentTransfer>();
    public virtual ICollection<StudentTransfer> TransfersTo { get; private set; } = new List<StudentTransfer>();

    protected Class() { }

    public Class(string className, long academicYearId, long gradeLevelId, long? homeroomEmployeeId = null, string status = "ACTIVE")
    {
        if (string.IsNullOrWhiteSpace(className))
            throw new BusinessException("Tên lớp học không được để trống.", "CLASS_NAME_REQUIRED");

        if (academicYearId <= 0)
            throw new BusinessException("Id năm học không hợp lệ.", "INVALID_ACADEMIC_YEAR_ID");

        if (gradeLevelId <= 0)
            throw new BusinessException("Id khối lớp không hợp lệ.", "INVALID_GRADE_LEVEL_ID");

        ClassName = className.Trim();
        AcademicYearId = academicYearId;
        GradeLevelId = gradeLevelId;
        HomeroomEmployeeId = homeroomEmployeeId;
        Status = string.IsNullOrWhiteSpace(status) ? "ACTIVE" : status.Trim().ToUpperInvariant();
    }

    public void AssignHomeroomEmployee(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);
        HomeroomEmployeeId = employee.Id;
        HomeroomEmployee = employee;
    }

    public int TotalStudents()
    {
        return StudentClasses.Count(sc => sc.Status == "ACTIVE");
    }
}
