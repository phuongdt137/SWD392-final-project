namespace HSMS.MVC.DTOs.Auth;

public record ResetPasswordDto(
    string Token,
    string Email,
    string NewPassword
);

