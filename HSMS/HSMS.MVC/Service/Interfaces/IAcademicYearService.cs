using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Academic;

namespace HSMS.MVC.Service.Interfaces
{
    public interface IAcademicYearService
    {
        Task<ServiceResult<List<AcademicYearListItemDto>>> GetAllAsync();
        Task<ServiceResult<AcademicYearDetailDto>> GetByIdAsync(long id);
        Task<ServiceResult<long>> CreateAsync(SaveAcademicYearDto dto);
        Task<ServiceResult> UpdateAsync(SaveAcademicYearDto dto);
        Task<ServiceResult<long>> CreateSemesterAsync(SaveSemesterDto dto);
    }
}
