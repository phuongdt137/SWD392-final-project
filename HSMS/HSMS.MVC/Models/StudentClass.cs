using HSMS.MVC.Exceptions;

namespace HSMS.MVC.Models;

public class StudentClass
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    public long ClassId { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }
    public string Status { get; private set; } = "ACTIVE";

    public virtual Customer Customer { get; private set; } = null!;
    public virtual Class Class { get; private set; } = null!;

    protected StudentClass() { }

    public StudentClass(long customerId, long classId, DateTime? joinedAt = null, string status = "ACTIVE")
    {
        if (customerId <= 0)
            throw new BusinessException("Id học sinh không hợp lệ.", "INVALID_CUSTOMER_ID");

        if (classId <= 0)
            throw new BusinessException("Id lớp học không hợp lệ.", "INVALID_CLASS_ID");

        CustomerId = customerId;
        ClassId = classId;
        JoinedAt = (joinedAt ?? DateTime.Today).Date;
        Status = string.IsNullOrWhiteSpace(status) ? "ACTIVE" : status.Trim().ToUpperInvariant();
    }

    public void Transfer(Class newClass)
    {
        ArgumentNullException.ThrowIfNull(newClass);
        if (Status != "ACTIVE")
            throw new BusinessException("Học sinh không ở trạng thái hoạt động trong lớp này để chuyển lớp.", "INVALID_STATUS");

        Status = "TRANSFERRED";
        LeftAt = DateTime.Today;
    }

    public void Withdraw()
    {
        if (Status != "ACTIVE")
            throw new BusinessException("Học sinh không ở trạng thái hoạt động để rút khỏi lớp.", "INVALID_STATUS");

        Status = "WITHDRAWN";
        LeftAt = DateTime.Today;
    }
}
