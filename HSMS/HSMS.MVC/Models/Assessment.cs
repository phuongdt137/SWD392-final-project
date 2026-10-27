using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

public class Assessment
{
    public long Id { get; private set; }
    public long TeachingAssignmentId { get; private set; }
    public long AssessmentTypeId { get; private set; }
    public string AssessmentName { get; private set; } = null!;
    public DateTime? ConductedAt { get; private set; }

    public virtual TeachingAssignment TeachingAssignment { get; private set; } = null!;
    public virtual AssessmentType AssessmentType { get; private set; } = null!;
    public virtual ICollection<StudentScore> StudentScores { get; private set; } = new List<StudentScore>();

    protected Assessment() { }

    public Assessment(long teachingAssignmentId, long assessmentTypeId, string assessmentName, DateTime? conductedAt = null)
    {
        if (teachingAssignmentId <= 0)
            throw new AppError("Id phân công giảng dạy không hợp lệ.", "INVALID_TEACHING_ASSIGNMENT_ID");

        if (assessmentTypeId <= 0)
            throw new AppError("Id loại bài kiểm tra không hợp lệ.", "INVALID_ASSESSMENT_TYPE_ID");

        if (string.IsNullOrWhiteSpace(assessmentName))
            throw new AppError("Tên bài kiểm tra không được để trống.", "ASSESSMENT_NAME_REQUIRED");

        TeachingAssignmentId = teachingAssignmentId;
        AssessmentTypeId = assessmentTypeId;
        AssessmentName = assessmentName.Trim();
        ConductedAt = conductedAt;
    }

    public StudentScore AddStudentScore(long customerId, decimal? score = null, string? comment = null)
    {
        if (StudentScores.Any(s => s.CustomerId == customerId))
            throw new AppError("Học sinh đã có điểm trong bài kiểm tra này.", "DUPLICATE_STUDENT_SCORE");

        var studentScore = new StudentScore(customerId, Id, score, comment);
        StudentScores.Add(studentScore);
        return studentScore;
    }

    public void LockAllScores()
    {
        foreach (var studentScore in StudentScores)
            studentScore.Lock();
    }

    public bool IsFullyLocked() => StudentScores.Count > 0 && StudentScores.All(s => s.IsLocked);

    public decimal CalculateAverage()
    {
        var validScores = StudentScores.Where(s => s.Score.HasValue).Select(s => s.Score!.Value).ToList();
        if (validScores.Count == 0)
            return 0m;

        return Math.Round(validScores.Average(), 2);
    }
}
