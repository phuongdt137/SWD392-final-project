using System.Security.Cryptography;
using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.Repositories.Interfaces;

namespace HSMS.MVC.Service;

/// <summary>
/// Thực thi nghiệp vụ xác thực người dùng sử dụng Repository Pattern và cơ chế AppError.
/// Tách biệt hoàn toàn, không phụ thuộc trực tiếp vào DbContext.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IEmailService emailService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.UsernameOrEmail) || string.IsNullOrWhiteSpace(dto.Password))
            {
                throw AuthErrors.MissingCredentials;
            }

            var identifier = dto.UsernameOrEmail.Trim().ToLower();

            var user = await _userRepository.GetByUsernameOrEmailWithDetailsAsync(identifier);

            if (user == null)
            {
                throw AuthErrors.InvalidCredentials;
            }

            if (!user.IsActive)
            {
                throw AuthErrors.AccountInactive;
            }

            // Xác thực mật khẩu băm
            bool passwordMatches = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!passwordMatches)
            {
                throw AuthErrors.InvalidCredentials;
            }

            // Lấy danh sách Roles đang hoạt động thông qua Domain Method
            var roles = user.GetActiveRoleCodes();

            if (roles.Count == 0)
            {
                throw AuthErrors.RoleNotAssigned;
            }

            // Xác định UserType
            string userType = user.EmployeeProfile != null ? "EMPLOYEE"
                            : (user.CustomerProfile != null ? "CUSTOMER" : "USER");

            // Cập nhật thời điểm đăng nhập qua Repository
            user.RecordLogin();
            await _userRepository.SaveChangesAsync();

            var responseDto = new LoginResponseDto(
                user.Id,
                user.Username,
                user.FullName,
                user.Email,
                userType,
                roles,
                user.AvatarUrl
            );

            return ServiceResult<LoginResponseDto>.Success(responseDto, "Đăng nhập thành công.");
        }
        catch (AppError ex)
        {
            return ServiceResult<LoginResponseDto>.Failed(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, dto.UsernameOrEmail);
            return ServiceResult<LoginResponseDto>.Failed(new AppError("Hệ thống gặp sự cố khi xử lý đăng nhập. Vui lòng thử lại sau.", "SYSTEM_ERROR", ErrorType.Failure));
        }
    }

    public async Task<ServiceResult> ChangePasswordAsync(ChangePasswordDto dto)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(dto.UserId);
            if (user == null)
            {
                throw AuthErrors.UserNotFound;
            }

            if (!user.IsActive)
            {
                throw AuthErrors.AccountInactive;
            }

            bool passwordMatches = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
            if (!passwordMatches)
            {
                throw AuthErrors.InvalidCurrentPassword;
            }

            bool isSamePassword = BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash);
            if (isSamePassword)
            {
                throw AuthErrors.PasswordSameAsOld;
            }

            string newHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ChangePassword(newHash);

            await _userRepository.SaveChangesAsync();

            return ServiceResult.Success("Đổi mật khẩu thành công.");
        }
        catch (AppError ex)
        {
            return ServiceResult.Failed(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi hệ thống ngoài dự kiến khi đổi mật khẩu cho UserId: {UserId}", dto.UserId);
            return ServiceResult.Failed(new AppError("Hệ thống gặp sự cố khi đổi mật khẩu. Vui lòng thử lại sau.", "SYSTEM_ERROR", ErrorType.Failure));
        }
    }

    public async Task<ServiceResult> ForgotPasswordAsync(ForgotPasswordDto dto, string resetLinkBaseUrl)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
            {
                throw new AppError("Vui lòng cung cấp địa chỉ email.", "EMAIL_REQUIRED");
            }

            var email = dto.Email.Trim().ToLower();
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null || !user.IsActive)
            {
                throw AuthErrors.EmailNotFound;
            }

            // Sinh token ngẫu nhiên bảo mật 32 bytes (64 hex characters)
            var tokenBytes = RandomNumberGenerator.GetBytes(32);
            var token = Convert.ToHexString(tokenBytes);
            var expiry = DateTime.UtcNow.AddMinutes(15);

            user.SetPasswordResetToken(token, expiry);
            await _userRepository.SaveChangesAsync();

            var resetUrl = $"{resetLinkBaseUrl}?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email)}";
            await _emailService.SendPasswordResetEmailAsync(user.Email, resetUrl);

            return ServiceResult.Success("Yêu cầu đặt lại mật khẩu đã được xử lý. Vui lòng kiểm tra email.");
        }
        catch (AppError ex)
        {
            return ServiceResult.Failed(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi hệ thống ngoài dự kiến khi xử lý quên mật khẩu cho Email: {Email}", dto.Email);
            return ServiceResult.Failed(new AppError("Hệ thống gặp sự cố khi xử lý yêu cầu quên mật khẩu. Vui lòng thử lại sau.", "SYSTEM_ERROR", ErrorType.Failure));
        }
    }

    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.Email))
            {
                throw AuthErrors.ResetTokenInvalidOrExpired;
            }

            var email = dto.Email.Trim().ToLower();
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null || !user.IsActive)
            {
                throw AuthErrors.ResetTokenInvalidOrExpired;
            }

            if (!user.IsPasswordResetTokenValid(dto.Token))
            {
                throw AuthErrors.ResetTokenInvalidOrExpired;
            }

            string newHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            user.ChangePassword(newHash);
            user.ClearPasswordResetToken();

            await _userRepository.SaveChangesAsync();

            return ServiceResult.Success("Đặt lại mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới.");
        }
        catch (AppError ex)
        {
            return ServiceResult.Failed(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi hệ thống ngoài dự kiến khi đặt lại mật khẩu cho Email: {Email}", dto.Email);
            return ServiceResult.Failed(new AppError("Hệ thống gặp sự cố khi đặt lại mật khẩu. Vui lòng thử lại sau.", "SYSTEM_ERROR", ErrorType.Failure));
        }
    }
}
