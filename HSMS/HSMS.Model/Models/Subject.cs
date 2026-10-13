using HSMS.Model.Exceptions;

namespace HSMS.Model.Model;

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
            throw new BusinessException("Mã môn học không được để trống.", "SUBJECT_CODE_REQUIRED");

        if (string.IsNullOrWhiteSpace(subjectName))
            throw new BusinessException("Tên môn học không được để trống.", "SUBJECT_NAME_REQUIRED");

        SubjectCode = subjectCode.Trim().ToUpperInvariant();
        SubjectName = subjectName.Trim();
        EvaluationType = evaluationType?.Trim();
    }

    public Subject GetDetails() => this;
}
