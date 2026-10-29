using HSMS.MVC.Models;

namespace HSMS.MVC.Repositories.Interfaces
{
    public interface ISubjectRepository : IGenericRepository<Subject>
    {
        // Lấy tất cả môn học, kèm danh sách phân công để đếm số phân công
        Task<List<Subject>> GetAllWithAssignmentsAsync();

        // Mã môn đã có chưa? excludeId dùng khi sửa (bỏ qua chính môn đang sửa)
        Task<bool> CodeExistsAsync(string subjectCode, long? excludeId = null);

        // Môn đã được phân công giảng dạy hoặc có kết quả học kỳ chưa?
        Task<bool> IsInUseAsync(long id);

        // Đổi thông tin môn học (Subject có private set nên phải đổi qua EF)
        void UpdateInfo(Subject subject, string subjectCode, string subjectName, string? evaluationType);
    }
}
