using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Academic;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;

namespace HSMS.MVC.Repositories.Implementations
{
    public class SubjectService : ISubjectService
    {
        private readonly ISubjectRepository _repo;

        public SubjectService(ISubjectRepository repo)
        {
            _repo = repo;
        }

        public async Task<ServiceResult<List<SubjectListItemDto>>> GetAllAsync()
        {
            var subjects = await _repo.GetAllWithAssignmentsAsync();

            var list = subjects.Select(s => new SubjectListItemDto
            {
                Id = s.Id,
                SubjectCode = s.SubjectCode,
                SubjectName = s.SubjectName,
                EvaluationType = s.EvaluationType,
                AssignmentCount = s.TeachingAssignments.Count
            }).ToList();

            return ServiceResult<List<SubjectListItemDto>>.Success(list);
        }

        public async Task<ServiceResult<SaveSubjectDto>> GetByIdAsync(long id)
        {
            var subject = await _repo.GetByIdAsync(id);
            if (subject == null)
                return ServiceResult<SaveSubjectDto>.NotFound("Không tìm thấy môn học.");

            var dto = new SaveSubjectDto
            {
                Id = subject.Id,
                SubjectCode = subject.SubjectCode,
                SubjectName = subject.SubjectName,
                EvaluationType = subject.EvaluationType
            };
            return ServiceResult<SaveSubjectDto>.Success(dto);
        }

        public async Task<ServiceResult<long>> CreateAsync(SaveSubjectDto dto)
        {
            try
            {
                // Constructor của Subject tự kiểm tra mã/tên rỗng và ném AppError
                var subject = new Subject(dto.SubjectCode, dto.SubjectName, EmptyToNull(dto.EvaluationType));

                if (await _repo.CodeExistsAsync(subject.SubjectCode))
                    return ServiceResult<long>.Failed("Mã môn học đã tồn tại.", "SUBJECT_CODE_DUPLICATE", 409);

                await _repo.AddAsync(subject);
                await _repo.SaveChangesAsync();

                return ServiceResult<long>.Success(subject.Id, "Tạo môn học thành công.", 201);
            }
            catch (AppError ex)
            {
                return ServiceResult<long>.Failed(ex);
            }
        }

        public async Task<ServiceResult> UpdateAsync(SaveSubjectDto dto)
        {
            var subject = await _repo.GetByIdAsync(dto.Id);
            if (subject == null)
                return ServiceResult.NotFound("Không tìm thấy môn học.");

            var code = dto.SubjectCode.Trim().ToUpperInvariant();
            var name = dto.SubjectName.Trim();
            if (code == "" || name == "")
                return ServiceResult.BadRequest("Mã và tên môn học không được để trống.");

            if (await _repo.CodeExistsAsync(code, dto.Id))
                return ServiceResult.Failed("Mã môn học đã tồn tại.", "SUBJECT_CODE_DUPLICATE", 409);

            _repo.UpdateInfo(subject, code, name, EmptyToNull(dto.EvaluationType));
            await _repo.SaveChangesAsync();

            return ServiceResult.Success("Cập nhật môn học thành công.");
        }

        public async Task<ServiceResult> DeleteAsync(long id)
        {
            var subject = await _repo.GetByIdAsync(id);
            if (subject == null)
                return ServiceResult.NotFound("Không tìm thấy môn học.");

            if (await _repo.IsInUseAsync(id))
                return ServiceResult.Failed("Môn học đã được phân công giảng dạy nên không thể xóa.", "SUBJECT_IN_USE", 409);

            _repo.Delete(subject);
            await _repo.SaveChangesAsync();

            return ServiceResult.Success("Đã xóa môn học.");
        }

        private static string? EmptyToNull(string? text)
        {
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }
    }
}
