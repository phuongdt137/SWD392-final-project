using HSMS.MVC.Exceptions;

namespace HSMS.MVC.Models;

public class Semester
{
    public long Id { get; private set; }
    public string SemesterCode { get; private set; } = null!;
    public string SemesterName { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public long AcademicYearId { get; private set; }

    public virtual AcademicYear AcademicYear { get; private set; } = null!;
    public virtual ICollection<TeachingAssignment> TeachingAssignments { get; private set; } = new List<TeachingAssignment>();
    public virtual ICollection<ConductEvaluation> ConductEvaluations { get; private set; } = new List<ConductEvaluation>();
    public virtual ICollection<SemesterReport> SemesterReports { get; private set; } = new List<SemesterReport>();

    protected Semester() { }

    public Semester(string semesterCode, string semesterName, DateTime startDate, DateTime endDate, long academicYearId)
    {
        if (string.IsNullOrWhiteSpace(semesterCode))
            throw new BusinessException("Mã học kỳ không được để trống.", "SEMESTER_CODE_REQUIRED");

        if (string.IsNullOrWhiteSpace(semesterName))
            throw new BusinessException("Tên học kỳ không được để trống.", "SEMESTER_NAME_REQUIRED");

        if (academicYearId <= 0)
            throw new BusinessException("Id năm học không hợp lệ.", "INVALID_ACADEMIC_YEAR_ID");

        if (endDate < startDate)
            throw new BusinessException("Ngày kết thúc học kỳ phải lớn hơn hoặc bằng ngày bắt đầu.", "INVALID_DATE_RANGE");

        SemesterCode = semesterCode.Trim().ToUpperInvariant();
        SemesterName = semesterName.Trim();
        StartDate = startDate.Date;
        EndDate = endDate.Date;
        AcademicYearId = academicYearId;
    }

    public bool IsOngoing()
    {
        var today = DateTime.Today;
        return StartDate <= today && today <= EndDate;
    }

    public Semester GetDetails() => this;
}
