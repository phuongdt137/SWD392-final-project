using HSMS.MVC.Exceptions;

namespace HSMS.MVC.Models;

public class AcademicYear
{
    public long Id { get; private set; }
    public string YearCode { get; private set; } = null!;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public virtual ICollection<Semester> Semesters { get; private set; } = new List<Semester>();
    public virtual ICollection<Class> Classes { get; private set; } = new List<Class>();
    public virtual ICollection<StudentAnnualReport> StudentAnnualReports { get; private set; } = new List<StudentAnnualReport>();

    protected AcademicYear() { }

    public AcademicYear(string yearCode, DateTime startDate, DateTime endDate)
    {
        if (string.IsNullOrWhiteSpace(yearCode))
            throw new BusinessException("Mã năm học không được để trống.", "YEAR_CODE_REQUIRED");

        if (endDate < startDate)
            throw new BusinessException("Ngày kết thúc năm học phải lớn hơn hoặc bằng ngày bắt đầu.", "INVALID_DATE_RANGE");

        YearCode = yearCode.Trim();
        StartDate = startDate;
        EndDate = endDate;
    }

    public bool IsCurrent()
    {
        var today = DateTime.Today;
        return StartDate <= today && today <= EndDate;
    }
}
