using System.ComponentModel.DataAnnotations;

namespace HSMS.MVC.DTOs.Academic
{
    // Dùng cho form thêm/sửa môn học (vừa là dữ liệu form, vừa gửi vào Service)
    public class SaveSubjectDto
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã môn học.")]
        [StringLength(50, ErrorMessage = "Mã môn học tối đa 50 ký tự.")]
        [Display(Name = "Mã môn học")]
        public string SubjectCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên môn học.")]
        [StringLength(150, ErrorMessage = "Tên môn học tối đa 150 ký tự.")]
        [Display(Name = "Tên môn học")]
        public string SubjectName { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Hình thức đánh giá tối đa 50 ký tự.")]
        [Display(Name = "Hình thức đánh giá")]
        public string? EvaluationType { get; set; }
    }
}
