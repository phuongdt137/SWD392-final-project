using HSMS.MVC.Models;

namespace HSMS.MVC.Repositories.Interfaces;

/// <summary>
/// Giao diện Repository chuyên biệt cho thực thể TeachingAssignment (Phân công giảng dạy).
/// </summary>
public interface ITeachingAssignmentRepository : IGenericRepository<TeachingAssignment>
{
    /// <summary>
    /// Tìm phân công giảng dạy theo Id, kèm Lớp, Môn, Học kỳ và danh sách bài kiểm tra.
    /// </summary>
    Task<TeachingAssignment?> GetWithDetailsAsync(long id);

    /// <summary>
    /// Lấy các phân công giảng dạy đang hoạt động của một giáo viên.
    /// </summary>
    Task<IReadOnlyList<TeachingAssignment>> GetActiveByEmployeeAsync(long employeeId);
}
