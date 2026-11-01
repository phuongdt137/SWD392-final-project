using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

public class AssessmentType
{
    public long Id { get; private set; }
    public string TypeCode { get; private set; } = null!;
    public string TypeName { get; private set; } = null!;
    public decimal Weight { get; private set; }
    public int MinEntries { get; private set; } = 1;
    public int? MaxEntries { get; private set; }

    public virtual ICollection<Assessment> Assessments { get; private set; } = new List<Assessment>();

    protected AssessmentType() { }

    public AssessmentType(string typeCode, string typeName, decimal weight, int minEntries = 1, int? maxEntries = null)
    {
        if (string.IsNullOrWhiteSpace(typeCode))
            throw new AppError("Mã loại đánh giá không được để trống.", "TYPE_CODE_REQUIRED");

        if (string.IsNullOrWhiteSpace(typeName))
            throw new AppError("Tên loại đánh giá không được để trống.", "TYPE_NAME_REQUIRED");

        if (weight < 0 || weight > 100)
            throw new AppError("Trọng số bài kiểm tra phải từ 0 đến 100%.", "INVALID_WEIGHT");

        if (minEntries < 0)
            throw new AppError("Số lượng đầu điểm tối thiểu phải >= 0.", "INVALID_MIN_ENTRIES");

        if (maxEntries.HasValue && maxEntries.Value < minEntries)
            throw new AppError("Số lượng đầu điểm tối đa không thể nhỏ hơn số tối thiểu.", "INVALID_MAX_ENTRIES");

        TypeCode = typeCode.Trim();
        TypeName = typeName.Trim();
        Weight = weight;
        MinEntries = minEntries;
        MaxEntries = maxEntries;
    }

    public decimal GetWeight() => Weight;

    /// <summary>
    /// Kiểm tra số đầu điểm có vượt quá giới hạn tối đa của loại bài kiểm tra hay không.
    /// </summary>
    public bool IsValidEntryCount(int count) => count >= 0 && (!MaxEntries.HasValue || count <= MaxEntries.Value);
}
