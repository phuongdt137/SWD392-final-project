using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Academic;

namespace HSMS.MVC.Repositories.Interfaces
{
    public interface ISubjectService
    {
        Task<ServiceResult<List<SubjectListItemDto>>> GetAllAsync();
        Task<ServiceResult<SaveSubjectDto>> GetByIdAsync(long id);
        Task<ServiceResult<long>> CreateAsync(SaveSubjectDto dto);
        Task<ServiceResult> UpdateAsync(SaveSubjectDto dto);
        Task<ServiceResult> DeleteAsync(long id);
    }
}
