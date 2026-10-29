using System.ComponentModel.DataAnnotations;

namespace HSMS.MVC.DTOs.Academic
{
    // Form thêm/sửa năm học
    public class SaveAcademicYearDto
    {
        public long Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã năm học.")]
        [StringLength(30, ErrorMessage = "Mã năm học tối đa 30 ký tự.")]
        [Display(Name = "Mã năm học")]
        public string YearCode { get; set; } = string.Empty;

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
