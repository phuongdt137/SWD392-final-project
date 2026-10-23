using HSMS.MVC.Exceptions;

namespace HSMS.MVC.Models;

public class TeacherFeedback
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    public long TeachingAssignmentId { get; private set; }
    public string? FeedbackType { get; private set; }
    public string Comment { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public virtual Customer Customer { get; private set; } = null!;
    public virtual TeachingAssignment TeachingAssignment { get; private set; } = null!;

    protected TeacherFeedback() { }

    public TeacherFeedback(long customerId, long teachingAssignmentId, string comment, string? feedbackType = null)
    {
        if (customerId <= 0)
            throw new BusinessException("Id học sinh không hợp lệ.", "INVALID_CUSTOMER_ID");

        if (teachingAssignmentId <= 0)
            throw new BusinessException("Id phân công giảng dạy không hợp lệ.", "INVALID_TEACHING_ASSIGNMENT_ID");

        if (string.IsNullOrWhiteSpace(comment))
            throw new BusinessException("Nội dung nhận xét không được để trống.", "COMMENT_REQUIRED");

        CustomerId = customerId;
        TeachingAssignmentId = teachingAssignmentId;
        Comment = comment.Trim();
        FeedbackType = feedbackType;
        CreatedAt = DateTime.UtcNow;
    }

    public bool AddFeedback(string comment, string? type = null)
    {
        if (string.IsNullOrWhiteSpace(comment))
            throw new BusinessException("Nội dung nhận xét không được để trống.", "COMMENT_REQUIRED");

        Comment = comment.Trim();
        FeedbackType = type;
        CreatedAt = DateTime.UtcNow;
        return true;
    }

    public bool EditFeedback(string newComment)
    {
        if (string.IsNullOrWhiteSpace(newComment))
            throw new BusinessException("Nội dung nhận xét không được để trống.", "COMMENT_REQUIRED");

        Comment = newComment.Trim();
        UpdatedAt = DateTime.UtcNow;
        return true;
    }
}
