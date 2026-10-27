namespace HSMS.MVC.DTOs.Auth;

public record ChangePasswordDto(
    long AccountId,
    string AccountType,
    string CurrentPassword,
    string NewPassword
);

