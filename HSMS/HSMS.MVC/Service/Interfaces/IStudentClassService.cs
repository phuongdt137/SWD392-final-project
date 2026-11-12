using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Academic;
using static HSMS.MVC.DTOs.Academic.ClassDtos;

namespace HSMS.MVC.Service.Interfaces
{
    public interface IStudentClassService
    {
        // Danh sách lớp cho ô chọn ở trang danh sách
        Task<ServiceResult<List<SelectOptionDto>>> GetClassOptionsAsync();

        // Học sinh đang học của một lớp
        Task<ServiceResult<ClassStudentsDto>> GetClassStudentsAsync(long classId);

        // Xếp học sinh vào lớp
        Task<ServiceResult<AssignFormDto>> GetAssignFormAsync(long classId);
        Task<ServiceResult> AssignAsync(AssignStudentDto dto);

        // Chuyển lớp (trả về Id lớp mới để chuyển trang tới lớp đó)
        Task<ServiceResult<TransferFormDto>> GetTransferFormAsync(long studentClassId);
        Task<ServiceResult<long>> TransferAsync(TransferStudentDto dto);
    }
}
