namespace HSMS.MVC.DTOs.Auth;

public record ChangePasswordDto(
    long UserId,
    string CurrentPassword,
    string NewPassword
);

