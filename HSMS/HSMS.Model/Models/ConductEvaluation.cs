using HSMS.Model.Exceptions;

namespace HSMS.Model.Model;

public class ConductEvaluation
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    public long SemesterId { get; private set; }
    public long HomeroomEmployeeId { get; private set; }
    public string ConductGrade { get; private set; } = null!;
    public string? TeacherComment { get; private set; }
    public DateTime EvaluatedAt { get; private set; }

    public virtual Customer Customer { get; private set; } = null!;
    public virtual Semester Semester { get; private set; } = null!;
    public virtual Employee HomeroomEmployee { get; private set; } = null!;

    protected ConductEvaluation() { }

    public ConductEvaluation(long customerId, long semesterId, long homeroomEmployeeId, string conductGrade, string? teacherComment = null)
    {
        if (customerId <= 0)
            throw new BusinessException("Id học sinh không hợp lệ.", "INVALID_CUSTOMER_ID");

        if (semesterId <= 0)
            throw new BusinessException("Id học kỳ không hợp lệ.", "INVALID_SEMESTER_ID");

        if (homeroomEmployeeId <= 0)
            throw new BusinessException("Id giáo viên chủ nhiệm không hợp lệ.", "INVALID_EMPLOYEE_ID");

        if (string.IsNullOrWhiteSpace(conductGrade))
            throw new BusinessException("Xếp loại hạnh kiểm không được để trống.", "GRADE_REQUIRED");

        CustomerId = customerId;
        SemesterId = semesterId;
        HomeroomEmployeeId = homeroomEmployeeId;
        ConductGrade = conductGrade.Trim();
        TeacherComment = teacherComment;
        EvaluatedAt = DateTime.UtcNow;
    }

    public bool CreateEvaluation(string grade, string? comment, Employee teacher)
    {
        ArgumentNullException.ThrowIfNull(teacher);
        if (string.IsNullOrWhiteSpace(grade))
            throw new BusinessException("Xếp loại hạnh kiểm không được để trống.", "GRADE_REQUIRED");

        ConductGrade = grade.Trim();
        TeacherComment = comment;
        HomeroomEmployeeId = teacher.Id;
        HomeroomEmployee = teacher;
        EvaluatedAt = DateTime.UtcNow;
        return true;
    }

    public bool UpdateEvaluation(string newGrade, string? newComment = null)
    {
        if (string.IsNullOrWhiteSpace(newGrade))
            throw new BusinessException("Xếp loại hạnh kiểm không được để trống.", "GRADE_REQUIRED");

        ConductGrade = newGrade.Trim();
        TeacherComment = newComment;
        EvaluatedAt = DateTime.UtcNow;
        return true;
    }
}
