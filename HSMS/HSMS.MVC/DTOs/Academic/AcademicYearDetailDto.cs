namespace HSMS.MVC.DTOs.Academic
{
    // Chi tiết một năm học kèm các học kỳ
    public class AcademicYearDetailDto
    {
        public long Id { get; set; }
        public string YearCode { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<SemesterDto> Semesters { get; set; } = new();
    }
}
