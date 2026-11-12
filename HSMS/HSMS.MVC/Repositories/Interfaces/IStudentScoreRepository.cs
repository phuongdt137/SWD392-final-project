using HSMS.MVC.Models;

namespace HSMS.MVC.Repositories.Interfaces;

/// <summary>
/// Giao diện Repository chuyên biệt cho thực thể StudentScore (Điểm học sinh).
/// </summary>
public interface IStudentScoreRepository : IGenericRepository<StudentScore>
{
    /// <summary>
    /// Tìm điểm theo Id, kèm bài kiểm tra, học sinh, phụ huynh và nhật ký chỉnh sửa.
    /// </summary>
    Task<StudentScore?> GetWithDetailsAsync(long id);
}
