using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Auth;

namespace HSMS.MVC.Service;

/// <summary>
/// Giao diện xử lý nghiệp vụ xác thực người dùng.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Xác thực thông tin đăng nhập của người dùng (Customer hoặc Employee).
    /// </summary>
    /// <param name="dto">Dữ liệu yêu cầu đăng nhập.</param>
    /// <returns>Kết quả thực thi kèm theo thông tin định danh và danh sách quyền.</returns>
    Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginDto dto);

    /// <summary>
    /// Đổi mật khẩu cho người dùng đang đăng nhập.
    /// </summary>
    Task<ServiceResult> ChangePasswordAsync(ChangePasswordDto dto);

    /// <summary>
    /// Tiếp nhận yêu cầu quên mật khẩu, sinh token và gửi email liên kết đặt lại.
    /// </summary>
    Task<ServiceResult> ForgotPasswordAsync(ForgotPasswordDto dto, string resetLinkBaseUrl);

    /// <summary>
    /// Xác thực token và đặt lại mật khẩu mới cho người dùng.
    /// </summary>
    Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto);
}
