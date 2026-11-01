using HSMS.MVC.Models;

namespace HSMS.MVC.Repositories.Interfaces;

/// <summary>
/// Giao diện Repository chuyên biệt cho thực thể Customer (Học sinh / Phụ huynh).
/// </summary>
public interface ICustomerRepository : IGenericRepository<Customer>
{
    /// <summary>
    /// Lấy danh sách học sinh đang hoạt động trong lớp, kèm thông tin User.
    /// </summary>
    Task<IReadOnlyList<Customer>> GetActiveStudentsByClassAsync(long classId);

    /// <summary>
    /// Tìm hồ sơ Customer theo UserId, kèm danh sách con (nếu là phụ huynh).
    /// </summary>
    Task<Customer?> GetByUserIdWithChildrenAsync(long userId);

    /// <summary>
    /// Tìm hồ sơ Customer theo Id, kèm User và phụ huynh.
    /// </summary>
    Task<Customer?> GetByIdWithUserAsync(long id);
}
