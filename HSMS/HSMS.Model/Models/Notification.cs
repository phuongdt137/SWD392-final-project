namespace HSMS.Model.Model;

public class Notification
{
    public long Id { get; set; }
    public long? SenderCustomerId { get; set; }
    public long? SenderEmployeeId { get; set; }
    public long? RecipientCustomerId { get; set; }
    public long? RecipientEmployeeId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? ReadAt { get; set; }

    public virtual Customer? SenderCustomer { get; set; }
    public virtual Employee? SenderEmployee { get; set; }
    public virtual Customer? RecipientCustomer { get; set; }
    public virtual Employee? RecipientEmployee { get; set; }
}
