using HSMS.MVC.Models;

namespace HSMS.MVC.Repositories.Interfaces
{
    public interface IAcademicYearRepository : IGenericRepository<AcademicYear>
    {
        // Tất cả năm học (mới nhất trước), kèm học kỳ và lớp để đếm
        Task<List<AcademicYear>> GetAllWithDetailsAsync();

        // Một năm học kèm các học kỳ của nó
        Task<AcademicYear?> GetWithSemestersAsync(long id);

        // Mã năm học đã có chưa? (excludeId dùng khi sửa)
        Task<bool> CodeExistsAsync(string yearCode, long? excludeId = null);

        // Khoảng ngày này có chồng lên năm học khác không?
        Task<bool> OverlapsAsync(DateTime start, DateTime end, long? excludeId = null);

        // Mã học kỳ đã có chưa? (mã học kỳ là duy nhất toàn hệ thống)
        Task<bool> SemesterCodeExistsAsync(string semesterCode);

        Task AddSemesterAsync(Semester semester);

        // Đổi thông tin năm học (AcademicYear có private set nên phải đổi qua EF)
        void UpdateInfo(AcademicYear year, string yearCode, DateTime start, DateTime end);
    }
}
