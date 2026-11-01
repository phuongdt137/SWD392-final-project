using System.ComponentModel.DataAnnotations;

namespace HSMS.MVC.ViewModels.Auth;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập địa chỉ email.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [Display(Name = "Địa chỉ email")]
    public string Email { get; set; } = string.Empty;
}

