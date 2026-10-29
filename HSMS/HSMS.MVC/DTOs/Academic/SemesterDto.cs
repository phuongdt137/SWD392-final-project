namespace HSMS.MVC.DTOs.Academic
{
    // Một học kỳ (hiển thị trong trang sửa năm học)
    public class SemesterDto
    {
        public long Id { get; set; }
        public string SemesterCode { get; set; } = string.Empty;
        public string SemesterName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
