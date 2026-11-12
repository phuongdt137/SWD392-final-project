using System.ComponentModel.DataAnnotations;

namespace HSMS.MVC.DTOs.Academic
{
    public class ClassDtos
    {
        // Một lựa chọn trong ô dropdown
        public class SelectOptionDto
        {
            public long Id { get; set; }
            public string Text { get; set; } = string.Empty;
        }

        // Dữ liệu cho 3 dropdown trong form lớp học
        public class ClassOptionsDto
        {
            public List<SelectOptionDto> Years { get; set; } = new();
            public List<SelectOptionDto> GradeLevels { get; set; } = new();
            public List<SelectOptionDto> Teachers { get; set; } = new();
        }

        // Một dòng trong danh sách lớp
        public class ClassListItemDto
        {
            public long Id { get; set; }
            public string ClassName { get; set; } = string.Empty;
            public string AcademicYearCode { get; set; } = string.Empty;
            public string GradeLevelName { get; set; } = string.Empty;
            public string? HomeroomTeacherName { get; set; }
            public int StudentCount { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        // Chi tiết một lớp (dùng cho trang sửa)
        public class ClassDetailDto
        {
            public long Id { get; set; }
            public string ClassName { get; set; } = string.Empty;
            public long AcademicYearId { get; set; }
            public string AcademicYearCode { get; set; } = string.Empty;
            public long GradeLevelId { get; set; }
            public string GradeLevelName { get; set; } = string.Empty;
            public long? HomeroomEmployeeId { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        // Form thêm/sửa lớp
        public class SaveClassDto
        {
            public long Id { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập tên lớp.")]
            [StringLength(100, ErrorMessage = "Tên lớp tối đa 100 ký tự.")]
            [Display(Name = "Tên lớp")]
            public string ClassName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng chọn năm học.")]
            [Display(Name = "Năm học")]
            public long? AcademicYearId { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn khối.")]
            [Display(Name = "Khối")]
            public long? GradeLevelId { get; set; }

            [Display(Name = "Giáo viên chủ nhiệm")]
            public long? HomeroomEmployeeId { get; set; }

            [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
            [Display(Name = "Trạng thái")]
            public string Status { get; set; } = "ACTIVE";
        }
    }
}
