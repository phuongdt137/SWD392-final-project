namespace HSMS.MVC.Common;

/// <summary>
/// Đại diện cho kết quả thực thi của một tác vụ trong hệ thống HSMS (không kèm dữ liệu trả về).
/// </summary>
public class ServiceResult
{
    public bool IsSuccess { get; protected set; }
    public AppError? Error { get; protected set; }
    public string? Message => Error?.Description;
    public string? ErrorCode => Error?.Code;
    public int StatusCode { get; protected set; }
    public List<string> Errors { get; protected set; } = new();

    protected ServiceResult() { }

    public static ServiceResult Success(string? message = null, int statusCode = 200)
    {
        return new ServiceResult
        {
            IsSuccess = true,
            Error = null,
            StatusCode = statusCode,
            Errors = string.IsNullOrWhiteSpace(message) ? new() : new() { message }
        };
    }

    public static ServiceResult Failed(
        AppError error,
        int? statusCode = null,
        IEnumerable<string>? additionalErrors = null)
    {
        var code = statusCode ?? error.Type switch
        {
            ErrorType.Validation => 400,
            ErrorType.Unauthorized => 401,
            ErrorType.Forbidden => 403,
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            _ => 500
        };

        var errorsList = new List<string> { error.Description };
        if (additionalErrors != null)
            errorsList.AddRange(additionalErrors);

        return new ServiceResult
        {
            IsSuccess = false,
            Error = error,
            StatusCode = code,
            Errors = errorsList
        };
    }

    public static ServiceResult Failed(
        string message,
        string? errorCode = null,
        int statusCode = 400,
        IEnumerable<string>? errors = null)
    {
        var error = new AppError(message, errorCode ?? "GENERAL_ERROR", ErrorType.Failure);
        return Failed(error, statusCode, errors);
    }

    public static ServiceResult BadRequest(string message, string? errorCode = "BAD_REQUEST", IEnumerable<string>? errors = null)
        => Failed(new AppError(message, errorCode ?? "BAD_REQUEST", ErrorType.Validation), 400, errors);

    public static ServiceResult Unauthorized(string message = "Tài khoản chưa đăng nhập hoặc phiên làm việc đã hết hạn.")
        => Failed(new AppError(message, "UNAUTHORIZED", ErrorType.Unauthorized), 401);

    public static ServiceResult Forbidden(string message = "Bạn không có quyền thực hiện thao tác này.")
        => Failed(new AppError(message, "FORBIDDEN", ErrorType.Forbidden), 403);

    public static ServiceResult NotFound(string message = "Không tìm thấy dữ liệu yêu cầu.")
        => Failed(new AppError(message, "NOT_FOUND", ErrorType.NotFound), 404);
}

/// <summary>
/// Đại diện cho kết quả thực thi của một tác vụ trong hệ thống HSMS có trả về dữ liệu kiểu T.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu trả về.</typeparam>
public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; private set; }

    protected ServiceResult() { }

    public static ServiceResult<T> Success(T data, string? message = null, int statusCode = 200)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            Error = null,
            StatusCode = statusCode,
            Errors = string.IsNullOrWhiteSpace(message) ? new() : new() { message }
        };
    }

    public new static ServiceResult<T> Failed(
        AppError error,
        int? statusCode = null,
        IEnumerable<string>? additionalErrors = null)
    {
        var code = statusCode ?? error.Type switch
        {
            ErrorType.Validation => 400,
            ErrorType.Unauthorized => 401,
            ErrorType.Forbidden => 403,
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            _ => 500
        };

        var errorsList = new List<string> { error.Description };
        if (additionalErrors != null)
            errorsList.AddRange(additionalErrors);

        return new ServiceResult<T>
        {
            IsSuccess = false,
            Data = default,
            Error = error,
            StatusCode = code,
            Errors = errorsList
        };
    }

    public new static ServiceResult<T> Failed(
        string message,
        string? errorCode = null,
        int statusCode = 400,
        IEnumerable<string>? errors = null)
    {
        var error = new AppError(message, errorCode ?? "GENERAL_ERROR", ErrorType.Failure);
        return Failed(error, statusCode, errors);
    }

    public new static ServiceResult<T> BadRequest(string message, string? errorCode = "BAD_REQUEST", IEnumerable<string>? errors = null)
        => Failed(new AppError(message, errorCode ?? "BAD_REQUEST", ErrorType.Validation), 400, errors);

    public new static ServiceResult<T> Unauthorized(string message = "Tài khoản chưa đăng nhập hoặc phiên làm việc đã hết hạn.")
        => Failed(new AppError(message, "UNAUTHORIZED", ErrorType.Unauthorized), 401);

    public new static ServiceResult<T> Forbidden(string message = "Bạn không có quyền thực hiện thao tác này.")
        => Failed(new AppError(message, "FORBIDDEN", ErrorType.Forbidden), 403);

    public new static ServiceResult<T> NotFound(string message = "Không tìm thấy dữ liệu yêu cầu.")
        => Failed(new AppError(message, "NOT_FOUND", ErrorType.NotFound), 404);
}
