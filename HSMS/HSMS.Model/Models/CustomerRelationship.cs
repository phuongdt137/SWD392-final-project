namespace HSMS.Model.Model;

public class CustomerRelationship
{
    public long Id { get; set; }
    public long ParentCustomerId { get; set; }
    public long StudentCustomerId { get; set; }
    public string? RelationshipType { get; set; }
    public bool IsPrimaryGuardian { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Customer ParentCustomer { get; set; } = null!;
    public virtual Customer StudentCustomer { get; set; } = null!;
}
