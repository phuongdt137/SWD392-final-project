using HSMS.Model.Exceptions;

namespace HSMS.Model.Model;

/// <summary>Year-end transcript snapshot, finalized only after both semesters are reconciled.</summary>
public class StudentAnnualReport
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    public long AcademicYearId { get; private set; }
    public string Status { get; private set; } = "DRAFT";
    public decimal? AnnualGpa { get; private set; }
    public string? AcademicResult { get; private set; }
    public string? ConductGrade { get; private set; }
    public string? TeacherComment { get; private set; }
    public string? PrincipalComment { get; private set; }
    public long? FinalizedByEmployeeId { get; private set; }
    public DateTime? FinalizedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public virtual Customer Customer { get; private set; } = null!;
    public virtual AcademicYear AcademicYear { get; private set; } = null!;
    public virtual Employee? FinalizedByEmployee { get; private set; }
    protected StudentAnnualReport() { }

    public StudentAnnualReport(long customerId, long academicYearId)
    {
        if (customerId <= 0 || academicYearId <= 0) throw new BusinessException("Học sinh hoặc năm học không hợp lệ.", "INVALID_ANNUAL_REPORT");
        CustomerId = customerId; AcademicYearId = academicYearId; CreatedAt = DateTime.UtcNow;
    }

    public void UpdateResults(decimal annualGpa, string academicResult, string conductGrade, string? teacherComment = null, string? principalComment = null)
    {
        EnsureDraft();
        if (annualGpa is < 0 or > 10) throw new BusinessException("Điểm trung bình năm phải từ 0 đến 10.", "INVALID_SCORE");
        if (string.IsNullOrWhiteSpace(academicResult) || string.IsNullOrWhiteSpace(conductGrade)) throw new BusinessException("Kết quả học tập và hạnh kiểm là bắt buộc.", "ANNUAL_RESULT_REQUIRED");
        AnnualGpa = Math.Round(annualGpa, 2); AcademicResult = academicResult.Trim(); ConductGrade = conductGrade.Trim();
        TeacherComment = teacherComment?.Trim(); PrincipalComment = principalComment?.Trim();
    }

    public void Finalize(Employee employee)
    {
        ArgumentNullException.ThrowIfNull(employee); EnsureDraft();
        if (!AnnualGpa.HasValue || AcademicResult is null || ConductGrade is null) throw new BusinessException("Cần hoàn thiện kết quả năm học trước khi khóa học bạ.", "ANNUAL_REPORT_INCOMPLETE");
        Status = "FINALIZED"; FinalizedByEmployeeId = employee.Id; FinalizedByEmployee = employee; FinalizedAt = DateTime.UtcNow;
    }

    private void EnsureDraft()
    {
        if (Status != "DRAFT") throw new BusinessException("Học bạ đã khóa, không thể chỉnh sửa.", "ANNUAL_REPORT_LOCKED");
    }
}
