using System.ComponentModel.DataAnnotations;
using static HSMS.MVC.DTOs.Academic.ClassDtos;

namespace HSMS.MVC.DTOs.Academic
{
    // Một học sinh trong bảng danh sách lớp
    public class StudentInClassDto
    {
        public long StudentClassId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }

    // Dữ liệu trang danh sách: thông tin lớp + học sinh đang học
    public class ClassStudentsDto
    {
        public long ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string AcademicYearCode { get; set; } = string.Empty;
        public string ClassStatus { get; set; } = string.Empty;
        public List<StudentInClassDto> Students { get; set; } = new();
    }

    // Dữ liệu để dựng form xếp lớp (không phải dữ liệu người dùng gửi lên)
    public class AssignFormDto
    {
        public long ClassId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string AcademicYearCode { get; set; } = string.Empty;
        public List<SelectOptionDto> Students { get; set; } = new();
    }

    // Dữ liệu người dùng gửi lên khi xếp học sinh vào lớp
    public class AssignStudentDto
    {
        public long ClassId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn học sinh.")]
        [Display(Name = "Học sinh")]
        public long? CustomerId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày vào lớp.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Ngày vào lớp")]
        public DateTime? JoinedAt { get; set; }
    }

    // Dữ liệu để dựng form chuyển lớp
    public class TransferFormDto
    {
        public long StudentClassId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string FromClassName { get; set; } = string.Empty;
        public string AcademicYearCode { get; set; } = string.Empty;
        public List<SelectOptionDto> Classes { get; set; } = new();
    }

    // Dữ liệu người dùng gửi lên khi chuyển lớp
    public class TransferStudentDto
    {
        public long StudentClassId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lớp mới.")]
        [Display(Name = "Lớp mới")]
        public long? ToClassId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do chuyển lớp.")]
        [StringLength(1000, ErrorMessage = "Lý do tối đa 1000 ký tự.")]
        [Display(Name = "Lý do chuyển lớp")]
        public string Reason { get; set; } = string.Empty;
    }
}
