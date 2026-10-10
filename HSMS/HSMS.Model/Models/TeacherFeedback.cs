namespace HSMS.Model.Model;

public class TeacherFeedback
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public long TeachingAssignmentId { get; set; }
    public string? FeedbackType { get; set; }
    public string Comment { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual TeachingAssignment TeachingAssignment { get; set; } = null!;
}
