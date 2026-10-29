namespace HSMS.MVC.DTOs.Academic
{
    // Một dòng trong danh sách năm học
    public class AcademicYearListItemDto
    {
        public long Id { get; set; }
        public string YearCode { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int SemesterCount { get; set; }
        public int ClassCount { get; set; }
        public bool IsCurrent { get; set; }
    }
}
