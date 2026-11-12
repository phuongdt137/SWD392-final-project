using HSMS.MVC.Models;


namespace HSMS.MVC.Repositories.Interfaces
{
    public interface IStudentClassRepository : IGenericRepository<StudentClass>
    {
        // Một lớp kèm năm học (có tracking vì sẽ truyền vào StudentClass.Transfer)
        Task<Class?> GetClassWithYearAsync(long classId);

        // Tất cả lớp kèm năm học, dùng cho ô chọn lớp ở trang danh sách
        Task<List<Class>> GetAllClassesAsync();

        // Các lớp ACTIVE khác trong cùng năm học (đích đến khi chuyển lớp)
        Task<List<Class>> GetOtherActiveClassesInYearAsync(long academicYearId, long excludeClassId);

        // Học sinh đang học (ACTIVE) của một lớp, kèm hồ sơ để lấy tên
        Task<List<StudentClass>> GetActiveStudentsOfClassAsync(long classId);

        // Một bản ghi học sinh-lớp kèm học sinh, lớp, năm học (có tracking để sửa rồi lưu)
        Task<StudentClass?> GetWithDetailsAsync(long studentClassId);

        // Hồ sơ này có phải học sinh đang hoạt động không?
        Task<bool> IsStudentAsync(long customerId);

        // Học sinh đã có lớp ACTIVE trong năm học này chưa?
        Task<bool> HasActiveInYearAsync(long customerId, long academicYearId);

        // Học sinh chưa có lớp ACTIVE nào trong năm học này (để chọn khi xếp lớp)
        Task<List<Customer>> GetAssignableStudentsAsync(long academicYearId);

        // Thêm bản ghi lịch sử chuyển lớp
        Task AddTransferAsync(StudentTransfer transfer);
    }
}
