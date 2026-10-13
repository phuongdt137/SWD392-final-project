using HSMS.Model.Exceptions;

namespace HSMS.Model.Model;

public class CustomerRelationship
{
    public long Id { get; private set; }
    public long ParentCustomerId { get; private set; }
    public long StudentCustomerId { get; private set; }
    public string? RelationshipType { get; private set; }
    public bool IsPrimaryGuardian { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual Customer ParentCustomer { get; private set; } = null!;
    public virtual Customer StudentCustomer { get; private set; } = null!;

    protected CustomerRelationship() { }

    public CustomerRelationship(long parentCustomerId, long studentCustomerId, string? relationshipType = null, bool isPrimaryGuardian = false)
    {
        if (parentCustomerId <= 0)
            throw new BusinessException("Id phụ huynh không hợp lệ.", "INVALID_PARENT_ID");

        if (studentCustomerId <= 0)
            throw new BusinessException("Id học sinh không hợp lệ.", "INVALID_STUDENT_ID");

        if (parentCustomerId == studentCustomerId)
            throw new BusinessException("Phụ huynh và học sinh không thể là cùng một người.", "SAME_CUSTOMER_RELATIONSHIP");

        ParentCustomerId = parentCustomerId;
        StudentCustomerId = studentCustomerId;
        RelationshipType = relationshipType?.Trim();
        IsPrimaryGuardian = isPrimaryGuardian;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetPrimaryGuardian(bool isPrimary = true) => IsPrimaryGuardian = isPrimary;

    public void UpdateRelationshipType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new BusinessException("Loại quan hệ không được để trống.", "INVALID_RELATIONSHIP_TYPE");

        RelationshipType = type.Trim();
    }
}
