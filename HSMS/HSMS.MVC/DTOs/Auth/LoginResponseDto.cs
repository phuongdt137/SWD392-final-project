using System.Text.Json.Serialization;

namespace HSMS.MVC.DTOs.Auth;

/// <summary>
/// DTO chứa thông tin danh tính người dùng sau khi xác thực đăng nhập thành công.
/// </summary>
public class LoginResponseDto
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty; // "CUSTOMER" hoặc "EMPLOYEE"
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public string? AvatarUrl { get; set; }

    public LoginResponseDto() { }

    [JsonConstructor]
    public LoginResponseDto(
        long id,
        string username,
        string fullName,
        string email,
        string userType,
        IReadOnlyList<string> roles,
        string? avatarUrl = null)
    {
        Id = id;
        Username = username;
        FullName = fullName;
        Email = email;
        UserType = userType;
        Roles = roles ?? Array.Empty<string>();
        AvatarUrl = avatarUrl;
    }

    public LoginResponseDto(
        long id,
        string username,
        string fullName,
        string email,
        string userType,
        IEnumerable<string> roles,
        string? avatarUrl = null)
        : this(id, username, fullName, email, userType, (roles?.ToList() ?? new List<string>()).AsReadOnly(), avatarUrl)
    {
    }
}

