using HSMS.MVC.Exceptions;

namespace HSMS.MVC.Models;

public class SemesterReport
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    public long SemesterId { get; private set; }
    public string Status { get; private set; } = "DRAFT";
    public long? FinalizedByEmployeeId { get; private set; }
    public DateTime? FinalizedAt { get; private set; }
    public decimal? GPA { get; private set; }
    public string? AcademicRank { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual Customer Customer { get; private set; } = null!;
    public virtual Semester Semester { get; private set; } = null!;
    public virtual Employee? FinalizedByEmployee { get; private set; }
    public virtual ICollection<SemesterSubjectResult> SemesterSubjectResults { get; private set; } = new List<SemesterSubjectResult>();

    protected SemesterReport() { }

    public SemesterReport(long customerId, long semesterId, string status = "DRAFT")
    {
        if (customerId <= 0)
            throw new BusinessException("Id học sinh không hợp lệ.", "INVALID_CUSTOMER_ID");

        if (semesterId <= 0)
            throw new BusinessException("Id học kỳ không hợp lệ.", "INVALID_SEMESTER_ID");

        CustomerId = customerId;
        SemesterId = semesterId;
        Status = string.IsNullOrWhiteSpace(status) ? "DRAFT" : status.Trim().ToUpperInvariant();
        CreatedAt = DateTime.UtcNow;
    }

    public decimal CalculateGPA()
    {
        var validResults = SemesterSubjectResults.Where(r => r.AverageScore.HasValue).Select(r => r.AverageScore!.Value).ToList();
        if (validResults.Count == 0)
        {
            GPA = 0m;
            return 0m;
        }

        GPA = Math.Round(validResults.Average(), 2);
        return GPA.Value;
    }

    public bool FinalizeReport(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee);
        CalculateGPA();
        Status = "FINALIZED";
        FinalizedByEmployeeId = employee.Id;
        FinalizedByEmployee = employee;
        FinalizedAt = DateTime.UtcNow;
        return true;
    }
}
