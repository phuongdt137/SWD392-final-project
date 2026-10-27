namespace HSMS.MVC.Service;

// Ảnh chụp danh tính hiện tại: User ID khác Employee ID và Customer ID; hồ sơ có thể chưa tồn tại.
public sealed record CurrentUser(long UserId, string Name, long? EmployeeId, long? CustomerId, IReadOnlyList<string> Roles)
{
    public bool Has(string role) => Roles.Contains(role);
    // Quyền đọc toàn trường, không đồng nghĩa với quyền ghi điểm danh.
    public bool SchoolReader => Has("ADMIN") || Has("STAFF") || Has("PRINCIPAL");
}
// Trả null nếu không xác định được tài khoản hợp lệ/đang hoạt động.
public interface ICurrentUserService
{
    Task<CurrentUser?> GetAsync(CancellationToken cancellationToken = default);
}
