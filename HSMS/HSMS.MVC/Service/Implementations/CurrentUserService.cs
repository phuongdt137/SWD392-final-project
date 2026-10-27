using System.Text.Json;
using HSMS.MVC.Controllers;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.Repositories.Interfaces;

namespace HSMS.MVC.Service;

// Dùng chung danh tính server cho Attendance và thông báo; tách User/Employee/Customer ID.
public sealed class CurrentUserService(IHttpContextAccessor http, IUserRepository users) : ICurrentUserService
{
    private Task<CurrentUser?>? resolved;

    // AuthenticationHandler và AttendanceService dùng chung kết quả trong một request.
    public Task<CurrentUser?> GetAsync(CancellationToken cancellationToken = default) =>
        resolved ??= ResolveAsync(cancellationToken);

    private async Task<CurrentUser?> ResolveAsync(CancellationToken cancellationToken)
    {
        var context = http.HttpContext;
        if (context == null) return null;
        var session = context.Session;
        await session.LoadAsync(cancellationToken);
        var json = session.GetString(AuthController.SessionUserKey);
        if (string.IsNullOrEmpty(json)) return null;

        LoginResponseDto? login;
        try { login = JsonSerializer.Deserialize<LoginResponseDto>(json); }
        catch (JsonException)
        {
            session.Remove(AuthController.SessionUserKey);
            return null;
        }
        if (login == null || login.Id <= 0)
        {
            session.Remove(AuthController.SessionUserKey);
            return null;
        }

        // Chỉ tin ID nằm trong Session server; role/hồ sơ cũ phải đối chiếu lại database.
        // UserRepository nạp hồ sơ nhân viên, hồ sơ khách hàng và vai trò để lập CurrentUser.
        var user = await users.GetByIdWithDetailsAsync(login.Id);
        if (user is not { IsActive: true } || user.GetActiveRoleCodes().Count == 0)
        {
            session.Remove(AuthController.SessionUserKey);
            return null;
        }

        var roles = user.GetActiveRoleCodes();
        var snapshot = new LoginResponseDto(user.Id, user.Username, user.FullName, user.Email,
            user.EmployeeProfile != null ? "EMPLOYEE" : user.CustomerProfile != null ? "CUSTOMER" : "USER",
            roles, user.AvatarUrl);
        var updated = JsonSerializer.Serialize(snapshot);
        // Đồng bộ giao diện với quyền hiện hành, chỉ ghi Session nếu dữ liệu thực sự đổi.
        if (updated != json) session.SetString(AuthController.SessionUserKey, updated);
        return new(user.Id, user.FullName, user.EmployeeProfile?.Id, user.CustomerProfile?.Id, roles);
    }
}
