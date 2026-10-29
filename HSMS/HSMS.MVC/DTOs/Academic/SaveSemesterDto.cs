using System.ComponentModel.DataAnnotations;

namespace HSMS.MVC.DTOs.Academic
{
    // Form thêm học kỳ
    public class SaveSemesterDto
    {
        public long AcademicYearId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã học kỳ.")]
        [StringLength(30, ErrorMessage = "Mã học kỳ tối đa 30 ký tự.")]
        [Display(Name = "Mã học kỳ")]
        public string SemesterCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập tên học kỳ.")]
        [StringLength(100, ErrorMessage = "Tên học kỳ tối đa 100 ký tự.")]
        [Display(Name = "Tên học kỳ")]
        public string SemesterName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày kết thúc")]
        public DateTime? EndDate { get; set; }
    }
}
