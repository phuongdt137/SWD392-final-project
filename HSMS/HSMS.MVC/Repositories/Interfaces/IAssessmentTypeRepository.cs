using HSMS.MVC.Models;

namespace HSMS.MVC.Repositories.Interfaces;

/// <summary>
/// Giao diện Repository chuyên biệt cho thực thể AssessmentType (Loại bài kiểm tra).
/// </summary>
public interface IAssessmentTypeRepository : IGenericRepository<AssessmentType>
{
    /// <summary>
    /// Lấy toàn bộ loại bài kiểm tra, sắp xếp theo mã loại.
    /// </summary>
    Task<IReadOnlyList<AssessmentType>> GetAllOrderedAsync();
}
