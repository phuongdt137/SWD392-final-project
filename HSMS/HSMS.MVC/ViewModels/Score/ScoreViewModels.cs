using System.ComponentModel.DataAnnotations;

namespace HSMS.MVC.ViewModels.Score;

public class CreateAssessmentViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn lớp/môn phân công.")]
    [Display(Name = "Phân công giảng dạy")]
    public long AssignmentId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn loại bài kiểm tra.")]
    [Display(Name = "Loại bài kiểm tra")]
    public long AssessmentTypeId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tên bài kiểm tra.")]
    [StringLength(200, ErrorMessage = "Tên bài kiểm tra không được vượt quá 200 ký tự.")]
    [Display(Name = "Tên bài kiểm tra")]
    public string AssessmentName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    [Display(Name = "Ngày tổ chức")]
    public DateTime? ConductedAt { get; set; }
}

public class ScoreEntryViewModel
{
    public long CustomerId { get; set; }

    [Range(0, 10, ErrorMessage = "Điểm số phải nằm trong khoảng từ 0 đến 10.")]
    public decimal? Score { get; set; }

    [StringLength(1000, ErrorMessage = "Nhận xét không được vượt quá 1000 ký tự.")]
    public string? Comment { get; set; }
}

public class EnterScoresViewModel
{
    public long AssessmentId { get; set; }

    public List<ScoreEntryViewModel> Items { get; set; } = new();
}

public class UpdateScoreViewModel
{
    [Required]
    public long ScoreId { get; set; }

    [Required]
    public long AssessmentId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập điểm mới.")]
    [Range(0, 10, ErrorMessage = "Điểm số phải nằm trong khoảng từ 0 đến 10.")]
    [Display(Name = "Điểm mới")]
    public decimal NewScore { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập lý do sửa điểm.")]
    [StringLength(1000, ErrorMessage = "Lý do không được vượt quá 1000 ký tự.")]
    [Display(Name = "Lý do sửa điểm")]
    public string Reason { get; set; } = string.Empty;
}
