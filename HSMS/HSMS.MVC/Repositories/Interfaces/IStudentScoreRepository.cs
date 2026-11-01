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

    /// <summary>
    /// Lấy điểm của học sinh trong một học kỳ (hoặc toàn bộ nếu semesterId = null).
    /// </summary>
    Task<IReadOnlyList<StudentScore>> GetByStudentAndSemesterAsync(long customerId, long? semesterId);

    /// <summary>
    /// Lấy danh sách học kỳ mà học sinh đã có điểm.
    /// </summary>
    Task<IReadOnlyList<Semester>> GetSemestersByStudentAsync(long customerId);
}
