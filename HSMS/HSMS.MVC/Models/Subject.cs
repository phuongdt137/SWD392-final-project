using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

public class Subject
{
    public long Id { get; private set; }
    public string SubjectCode { get; private set; } = null!;
    public string SubjectName { get; private set; } = null!;
    public string? EvaluationType { get; private set; }

    public virtual ICollection<TeachingAssignment> TeachingAssignments { get; private set; } = new List<TeachingAssignment>();
    public virtual ICollection<SemesterSubjectResult> SemesterSubjectResults { get; private set; } = new List<SemesterSubjectResult>();

    protected Subject() { }

    public Subject(string subjectCode, string subjectName, string? evaluationType = null)
    {
        if (string.IsNullOrWhiteSpace(subjectCode))
            throw new AppError("Mã môn học không được để trống.", "SUBJECT_CODE_REQUIRED");

        if (string.IsNullOrWhiteSpace(subjectName))
            throw new AppError("Tên môn học không được để trống.", "SUBJECT_NAME_REQUIRED");

        SubjectCode = subjectCode.Trim().ToUpperInvariant();
        SubjectName = subjectName.Trim();
        EvaluationType = evaluationType?.Trim();
    }

    /// <summary>
    /// Môn có đánh giá bằng điểm số hay không. Môn để trống hình thức đánh giá được coi là chấm điểm;
    /// môn đánh giá bằng nhận xét / đạt - chưa đạt thì không nhập điểm số.
    /// </summary>
    public bool IsScoreEvaluated()
    {
        if (string.IsNullOrWhiteSpace(EvaluationType))
            return true;

        var type = EvaluationType.Trim();
        return !(type.Contains("nhận xét", StringComparison.OrdinalIgnoreCase)
                 || type.Contains("comment", StringComparison.OrdinalIgnoreCase)
                 || type.Contains("đạt", StringComparison.OrdinalIgnoreCase));
    }

    public Subject GetSubjectDetails() => this;
}
