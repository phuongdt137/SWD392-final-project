namespace HSMS.MVC.DTOs.Auth;

/// <summary>
/// DTO chứa thông tin yêu cầu đăng nhập gửi vào tầng Service.
/// </summary>
public class LoginDto
{
    public string UsernameOrEmail { get; }
    public string Password { get; }
    public bool RememberMe { get; }

    public LoginDto(string usernameOrEmail, string password, bool rememberMe = false)
    {
        UsernameOrEmail = usernameOrEmail?.Trim() ?? string.Empty;
        Password = password ?? string.Empty;
        RememberMe = rememberMe;
    }
}
