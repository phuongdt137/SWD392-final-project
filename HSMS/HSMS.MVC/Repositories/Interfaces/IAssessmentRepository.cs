using HSMS.MVC.Models;

namespace HSMS.MVC.Repositories.Interfaces;

/// <summary>
/// Giao diện Repository chuyên biệt cho thực thể Assessment (Bài kiểm tra).
/// </summary>
public interface IAssessmentRepository : IGenericRepository<Assessment>
{
    /// <summary>
    /// Tìm bài kiểm tra theo Id, kèm phân công giảng dạy (Lớp, Môn), loại bài và bảng điểm hiện có.
    /// </summary>
    Task<Assessment?> GetWithDetailsAsync(long id);
}
