namespace HSMS.MVC.Common;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5
}

/// <summary>
/// Đại diện cho lỗi hệ thống và lỗi nghiệp vụ dùng chung toàn ứng dụng HSMS.
/// Kế thừa System.Exception để có thể throw trực tiếp trong Model/Entity hoặc đóng gói trong ServiceResult.
/// </summary>
public class AppError : Exception
{
    public string Code { get; }
    public string Description => Message;
    public ErrorType Type { get; }

    public AppError(string message, string? code = null, ErrorType type = ErrorType.Validation)
        : base(message)
    {
        Code = code ?? (type == ErrorType.Validation ? "VALIDATION_ERROR" : "BUSINESS_ERROR");
        Type = type;
    }

    public AppError(string message, Exception innerException, string? code = null, ErrorType type = ErrorType.Failure)
        : base(message, innerException)
    {
        Code = code ?? "SYSTEM_ERROR";
        Type = type;
    }

    public static readonly AppError None = new(string.Empty, string.Empty, ErrorType.Failure);

    public override string ToString() => $"[{Code}] {Description}";
}

/// <summary>
/// Danh mục các lỗi nghiệp vụ định nghĩa sẵn cho phân hệ Authentication.
/// </summary>
public static class AuthErrors
{
    public static readonly AppError InvalidCredentials =
        new("Tên đăng nhập hoặc mật khẩu không chính xác.", "AUTH_INVALID_CREDENTIALS", ErrorType.Unauthorized);

    public static readonly AppError AccountInactive =
        new("Tài khoản của bạn đã bị vô hiệu hóa hoặc chưa được kích hoạt.", "AUTH_ACCOUNT_INACTIVE", ErrorType.Forbidden);

    public static readonly AppError UserNotFound =
        new("Không tìm thấy tài khoản người dùng tương ứng.", "AUTH_USER_NOT_FOUND", ErrorType.NotFound);

    public static readonly AppError RoleNotAssigned =
        new("Tài khoản chưa được phân quyền trong hệ thống. Vui lòng liên hệ quản trị viên.", "AUTH_ROLE_NOT_ASSIGNED", ErrorType.Forbidden);

    public static readonly AppError MissingCredentials =
        new("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu.", "AUTH_MISSING_CREDENTIALS", ErrorType.Validation);

    public static readonly AppError InvalidCurrentPassword =
        new("Mật khẩu hiện tại không chính xác.", "AUTH_INVALID_CURRENT_PASSWORD", ErrorType.Validation);

    public static readonly AppError PasswordSameAsOld =
        new("Mật khẩu mới không được trùng với mật khẩu hiện tại.", "AUTH_PASSWORD_SAME_AS_OLD", ErrorType.Validation);

    public static readonly AppError ResetTokenInvalidOrExpired =
        new("Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn. Vui lòng gửi lại yêu cầu.", "AUTH_RESET_TOKEN_INVALID", ErrorType.Validation);

    public static readonly AppError EmailNotFound =
        new("Không tìm thấy tài khoản người dùng với địa chỉ email này.", "AUTH_EMAIL_NOT_FOUND", ErrorType.NotFound);
}
