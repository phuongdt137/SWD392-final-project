using HSMS.MVC.Exceptions;

namespace HSMS.MVC.Models;

/// <summary>Auditable class or school transfer event.</summary>
public class StudentTransfer
{
    public long Id { get; private set; }
    public long CustomerId { get; private set; }
    public string TransferType { get; private set; } = null!; // CLASS, SCHOOL_IN, SCHOOL_OUT
    public long? FromClassId { get; private set; }
    public long? ToClassId { get; private set; }
    public string? FromSchoolName { get; private set; }
    public string? ToSchoolName { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public string Reason { get; private set; } = null!;
    public string Status { get; private set; } = "COMPLETED";
    public long RecordedByEmployeeId { get; private set; }
    public DateTime RecordedAt { get; private set; }
    public virtual Customer Customer { get; private set; } = null!;
    public virtual Class? FromClass { get; private set; }
    public virtual Class? ToClass { get; private set; }
    public virtual Employee RecordedByEmployee { get; private set; } = null!;

    protected StudentTransfer() { }

    public StudentTransfer(long customerId, string transferType, DateTime effectiveDate, string reason, long recordedByEmployeeId,
        long? fromClassId = null, long? toClassId = null, string? fromSchoolName = null, string? toSchoolName = null)
    {
        if (customerId <= 0 || recordedByEmployeeId <= 0) throw new BusinessException("Id học sinh hoặc nhân viên không hợp lệ.", "INVALID_TRANSFER_PARTY");
        var type = transferType?.Trim().ToUpperInvariant();
        if (type is not ("CLASS" or "SCHOOL_IN" or "SCHOOL_OUT")) throw new BusinessException("Loại chuyển trường/lớp không hợp lệ.", "INVALID_TRANSFER_TYPE");
        if (string.IsNullOrWhiteSpace(reason)) throw new BusinessException("Lý do chuyển không được để trống.", "TRANSFER_REASON_REQUIRED");
        if (type == "CLASS" && (!fromClassId.HasValue || !toClassId.HasValue || fromClassId == toClassId)) throw new BusinessException("Chuyển lớp cần lớp đi và lớp đến khác nhau.", "INVALID_CLASS_TRANSFER");
        if (type == "SCHOOL_IN" && string.IsNullOrWhiteSpace(fromSchoolName)) throw new BusinessException("Cần ghi trường chuyển đến từ.", "FROM_SCHOOL_REQUIRED");
        if (type == "SCHOOL_OUT" && string.IsNullOrWhiteSpace(toSchoolName)) throw new BusinessException("Cần ghi trường chuyển đến.", "TO_SCHOOL_REQUIRED");
        CustomerId = customerId; TransferType = type!; EffectiveDate = effectiveDate.Date; Reason = reason.Trim();
        RecordedByEmployeeId = recordedByEmployeeId; FromClassId = fromClassId; ToClassId = toClassId;
        FromSchoolName = fromSchoolName?.Trim(); ToSchoolName = toSchoolName?.Trim(); RecordedAt = DateTime.UtcNow;
    }
}
