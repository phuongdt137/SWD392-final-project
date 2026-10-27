using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Score;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;

namespace HSMS.MVC.Service;

/// <summary>
/// Thực thi nghiệp vụ Đánh giá & Điểm số theo Repository Pattern và cơ chế AppError.
/// Thứ tự kiểm tra trong từng phương thức bám theo sequence diagram của module.
/// </summary>
public class ScoreService : IScoreService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITeachingAssignmentRepository _assignmentRepository;
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IAssessmentTypeRepository _assessmentTypeRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IStudentScoreRepository _studentScoreRepository;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ScoreService> _logger;

    public ScoreService(
        IAccountRepository accountRepository,
        ITeachingAssignmentRepository assignmentRepository,
        IAssessmentRepository assessmentRepository,
        IAssessmentTypeRepository assessmentTypeRepository,
        ICustomerRepository customerRepository,
        IStudentScoreRepository studentScoreRepository,
        INotificationService notificationService,
        ILogger<ScoreService> logger)
    {
        _accountRepository = accountRepository;
        _assignmentRepository = assignmentRepository;
        _assessmentRepository = assessmentRepository;
        _assessmentTypeRepository = assessmentTypeRepository;
        _customerRepository = customerRepository;
        _studentScoreRepository = studentScoreRepository;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<ServiceResult<IReadOnlyList<TeachingAssignmentSummaryDto>>> GetMyTeachingAssignmentsAsync(long userId)
    {
        try
        {
            var teacher = await ResolveTeacherAsync(userId);
            var assignments = await _assignmentRepository.GetActiveByEmployeeAsync(teacher.Id);

            var result = assignments.Select(ta => new TeachingAssignmentSummaryDto
            {
                AssignmentId = ta.Id,
                SubjectName = ta.Subject.SubjectName,
                ClassName = ta.Class.ClassName,
                SemesterName = ta.Semester.SemesterName,
                Assessments = ta.Assessments
                    .OrderByDescending(a => a.ConductedAt)
                    .Select(ToAssessmentSummary)
                    .ToList()
            }).ToList();

            return ServiceResult<IReadOnlyList<TeachingAssignmentSummaryDto>>.Success(result);
        }
        catch (AppError ex)
        {
            return ServiceResult<IReadOnlyList<TeachingAssignmentSummaryDto>>.Failed(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy phân công giảng dạy cho UserId: {UserId}", userId);
            return ServiceResult<IReadOnlyList<TeachingAssignmentSummaryDto>>.Failed(ScoreErrors.SystemError);
        }
    }

    public async Task<ServiceResult<IReadOnlyList<AssessmentTypeDto>>> GetAssessmentTypesAsync()
    {
        try
        {
            var types = await _assessmentTypeRepository.GetAllOrderedAsync();

            var result = types.Select(t => new AssessmentTypeDto
            {
                Id = t.Id,
                TypeCode = t.TypeCode,
                TypeName = t.TypeName,
                Weight = t.GetWeight(),
                MinEntries = t.MinEntries,
                MaxEntries = t.MaxEntries
            }).ToList();

            return ServiceResult<IReadOnlyList<AssessmentTypeDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy danh sách loại bài kiểm tra");
            return ServiceResult<IReadOnlyList<AssessmentTypeDto>>.Failed(ScoreErrors.SystemError);
        }
    }

    public async Task<ServiceResult<long>> CreateAssessmentAsync(CreateAssessmentDto dto, long userId)
    {
        try
        {
            var teacher = await ResolveTeacherAsync(userId);

            var assignment = await _assignmentRepository.GetWithDetailsAsync(dto.AssignmentId)
                ?? throw ScoreErrors.AssignmentNotFound;

            if (!assignment.IsTaughtBy(teacher.Id))
                throw ScoreErrors.NotYourClass;

            var assessmentType = await _assessmentTypeRepository.GetByIdAsync(dto.AssessmentTypeId)
                ?? throw ScoreErrors.AssessmentTypeNotFound;

            var currentCount = assignment.Assessments.Count(a => a.AssessmentTypeId == assessmentType.Id);
            if (!assessmentType.IsValidEntryCount(currentCount + 1))
                throw ScoreErrors.MaxEntriesExceeded;

            var assessment = new Assessment(assignment.Id, assessmentType.Id, dto.AssessmentName, dto.ConductedAt);
            assignment.AddAssessment(assessment);

            await _assignmentRepository.SaveChangesAsync();

            return ServiceResult<long>.Success(assessment.Id, "Tạo bài kiểm tra thành công.", 201);
        }
        catch (AppError ex)
        {
            return ServiceResult<long>.Failed(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi tạo bài kiểm tra cho AssignmentId: {AssignmentId}", dto.AssignmentId);
            return ServiceResult<long>.Failed(ScoreErrors.SystemError);
        }
    }

    public async Task<ServiceResult<ScoreSheetDto>> GetScoreSheetAsync(long assessmentId, long userId)
    {
        try
        {
            var teacher = await ResolveTeacherAsync(userId);
            var assessment = await GetOwnedAssessmentAsync(assessmentId, teacher.Id);

            var students = await _customerRepository.GetActiveStudentsByClassAsync(assessment.TeachingAssignment.ClassId);

            var rows = students.Select(student =>
            {
                var score = assessment.StudentScores.FirstOrDefault(s => s.CustomerId == student.Id);
                return new ScoreSheetRowDto
                {
                    CustomerId = student.Id,
                    StudentCode = student.StudentCode,
                    FullName = student.FullName,
                    ScoreId = score?.Id,
                    Score = score?.Score,
                    Comment = score?.Comment
                };
            }).ToList();

            var sheet = new ScoreSheetDto
            {
                AssessmentId = assessment.Id,
                AssessmentName = assessment.AssessmentName,
                SubjectName = assessment.TeachingAssignment.Subject.SubjectName,
                ClassName = assessment.TeachingAssignment.Class.ClassName,
                TypeName = assessment.AssessmentType.TypeName,
                IsLocked = assessment.IsFullyLocked(),
                Rows = rows
            };

            return ServiceResult<ScoreSheetDto>.Success(sheet);
        }
        catch (AppError ex)
        {
            return ServiceResult<ScoreSheetDto>.Failed(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy bảng điểm AssessmentId: {AssessmentId}", assessmentId);
            return ServiceResult<ScoreSheetDto>.Failed(ScoreErrors.SystemError);
        }
    }

    public async Task<ServiceResult> EnterScoresAsync(EnterScoresDto dto, long userId)
    {
        try
        {
            var teacher = await ResolveTeacherAsync(userId);
            var assessment = await GetOwnedAssessmentAsync(dto.AssessmentId, teacher.Id);

            if (assessment.StudentScores.Any(s => s.IsLocked))
                throw ScoreErrors.ScoreLocked;

            var roster = await _customerRepository.GetActiveStudentsByClassAsync(assessment.TeachingAssignment.ClassId);
            var rosterNames = roster.ToDictionary(s => s.Id, s => s.FullName);

            var entries = dto.Items
                .Where(i => i.Score.HasValue || !string.IsNullOrWhiteSpace(i.Comment))
                .ToList();

            // Kiểm tra toàn bộ trước khi ghi, để không lưu một phần khi có lỗi.
            var errors = new List<string>();
            foreach (var item in entries)
            {
                var name = rosterNames.GetValueOrDefault(item.CustomerId, "Học sinh không xác định");

                if (!rosterNames.ContainsKey(item.CustomerId))
                {
                    errors.Add($"{name}: không thuộc lớp của phân công này.");
                    continue;
                }

                if (item.Score.HasValue && (item.Score.Value < 0 || item.Score.Value > 10))
                {
                    errors.Add($"{name}: điểm phải nằm trong khoảng từ 0 đến 10.");
                    continue;
                }

                var existing = assessment.StudentScores.FirstOrDefault(s => s.CustomerId == item.CustomerId);
                if (existing?.Score != null && item.Score.HasValue && item.Score.Value != existing.Score.Value)
                {
                    errors.Add($"{name}: đã có điểm, vui lòng dùng chức năng sửa điểm để thay đổi.");
                }
            }

            if (errors.Count > 0)
                return ServiceResult.Failed(ScoreErrors.InvalidScore, 400, errors.Distinct());

            foreach (var item in entries)
            {
                var existing = assessment.StudentScores.FirstOrDefault(s => s.CustomerId == item.CustomerId);

                if (existing == null)
                {
                    assessment.AddStudentScore(item.CustomerId, item.Score, item.Comment);
                }
                else if (existing.Score == null && item.Score.HasValue)
                {
                    existing.EnterScore(item.Score.Value, item.Comment);
                }
            }

            await _assessmentRepository.SaveChangesAsync();

            return ServiceResult.Success("Lưu điểm thành công.");
        }
        catch (AppError ex)
        {
            return ServiceResult.Failed(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi nhập điểm AssessmentId: {AssessmentId}", dto.AssessmentId);
            return ServiceResult.Failed(ScoreErrors.SystemError);
        }
    }

    public async Task<ServiceResult<StudentScoreDto>> UpdateScoreAsync(UpdateScoreDto dto, long userId, string? ipAddress)
    {
        try
        {
            var teacher = await ResolveTeacherAsync(userId);

            var score = await _studentScoreRepository.GetWithDetailsAsync(dto.ScoreId)
                ?? throw ScoreErrors.ScoreNotFound;

            if (!score.Assessment.TeachingAssignment.IsTaughtBy(teacher.Id))
                throw ScoreErrors.NotYourClass;

            if (score.IsLocked)
                throw ScoreErrors.ScoreLocked;

            var oldScore = score.Score;
            score.UpdateScore(dto.NewScore, dto.Reason, teacher, ipAddress);

            await _studentScoreRepository.SaveChangesAsync();
            await _notificationService.NotifyScoreChangedAsync(score, oldScore, userId);

            return ServiceResult<StudentScoreDto>.Success(ToStudentScoreDto(score), "Cập nhật điểm thành công.");
        }
        catch (AppError ex)
        {
            return ServiceResult<StudentScoreDto>.Failed(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi sửa điểm ScoreId: {ScoreId}", dto.ScoreId);
            return ServiceResult<StudentScoreDto>.Failed(ScoreErrors.SystemError);
        }
    }

    public async Task<ServiceResult<LockResultDto>> LockScoresAsync(long assessmentId, long userId)
    {
        try
        {
            var teacher = await ResolveTeacherAsync(userId);
            var assessment = await GetOwnedAssessmentAsync(assessmentId, teacher.Id);

            assessment.LockAllScores();
            await _assessmentRepository.SaveChangesAsync();

            var result = new LockResultDto
            {
                AssessmentId = assessment.Id,
                LockedCount = assessment.StudentScores.Count(s => s.IsLocked)
            };

            return ServiceResult<LockResultDto>.Success(result, "Đã khóa bảng điểm.");
        }
        catch (AppError ex)
        {
            return ServiceResult<LockResultDto>.Failed(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi khóa điểm AssessmentId: {AssessmentId}", assessmentId);
            return ServiceResult<LockResultDto>.Failed(ScoreErrors.SystemError);
        }
    }

    public async Task<ServiceResult<IReadOnlyList<ScoreAuditLogDto>>> GetScoreHistoryAsync(long scoreId, long userId)
    {
        try
        {
            var teacher = await ResolveTeacherAsync(userId);

            var score = await _studentScoreRepository.GetWithDetailsAsync(scoreId)
                ?? throw ScoreErrors.ScoreNotFound;

            if (!score.Assessment.TeachingAssignment.IsTaughtBy(teacher.Id))
                throw ScoreErrors.NotYourClass;

            var history = score.GetAuditLogs()
                .OrderByDescending(l => l.ChangedAt)
                .Select(l => new ScoreAuditLogDto
                {
                    ChangedAt = l.ChangedAt,
                    OldScore = l.OldScore,
                    NewScore = l.NewScore,
                    Difference = l.GetScoreDifference(),
                    ChangeReason = l.ChangeReason,
                    ChangedByName = l.ChangedByEmployee?.FullName ?? string.Empty,
                    IpAddress = l.IpAddress
                })
                .ToList();

            return ServiceResult<IReadOnlyList<ScoreAuditLogDto>>.Success(history);
        }
        catch (AppError ex)
        {
            return ServiceResult<IReadOnlyList<ScoreAuditLogDto>>.Failed(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi lấy lịch sử điểm ScoreId: {ScoreId}", scoreId);
            return ServiceResult<IReadOnlyList<ScoreAuditLogDto>>.Failed(ScoreErrors.SystemError);
        }
    }

    public async Task<ServiceResult<StudentScoreReportDto>> GetStudentScoresAsync(long? studentCustomerId, long? semesterId, long userId)
    {
        try
        {
            var viewer = await _customerRepository.GetByIdWithChildrenAsync(userId)
                ?? throw ScoreErrors.NotStudentOrParent;

            var targetId = studentCustomerId
                ?? (viewer.Children.Count > 0 ? viewer.Children.First().Id : viewer.Id);

            var student = await _customerRepository.GetByIdWithParentAsync(targetId)
                ?? throw ScoreErrors.StudentNotFound;

            if (student.Id != viewer.Id && !viewer.IsGuardianOf(student))
                throw ScoreErrors.NotStudentOrParent;

            var semesters = await _studentScoreRepository.GetSemestersByStudentAsync(student.Id);

            Semester? selectedSemester = semesterId.HasValue
                ? semesters.FirstOrDefault(s => s.Id == semesterId.Value)
                : semesters.FirstOrDefault(s => s.IsOngoing()) ?? semesters.FirstOrDefault();

            IReadOnlyList<StudentScore> scores = new List<StudentScore>();
            if (selectedSemester != null)
                scores = await _studentScoreRepository.GetByStudentAndSemesterAsync(student.Id, selectedSemester.Id);

            var subjects = scores
                .GroupBy(s => s.Assessment.TeachingAssignment.SubjectId)
                .Select(group => new SubjectScoreDto
                {
                    SubjectName = group.First().Assessment.TeachingAssignment.Subject.SubjectName,
                    WeightedAverage = CalculateWeightedAverage(group),
                    Items = group
                        .OrderBy(s => s.Assessment.AssessmentName)
                        .Select(s => new AssessmentScoreItemDto
                        {
                            AssessmentName = s.Assessment.AssessmentName,
                            TypeName = s.Assessment.AssessmentType.TypeName,
                            Weight = s.Assessment.AssessmentType.GetWeight(),
                            Score = s.Score
                        })
                        .ToList()
                })
                .OrderBy(s => s.SubjectName)
                .ToList();

            var report = new StudentScoreReportDto
            {
                StudentCustomerId = student.Id,
                StudentName = student.FullName,
                SelectedSemesterId = selectedSemester?.Id,
                Semesters = semesters.Select(s => new OptionDto { Id = s.Id, Name = s.SemesterName }).ToList(),
                Children = viewer.Children.Select(c => new OptionDto { Id = c.Id, Name = c.FullName }).ToList(),
                Subjects = subjects
            };

            return ServiceResult<StudentScoreReportDto>.Success(report);
        }
        catch (AppError ex)
        {
            return ServiceResult<StudentScoreReportDto>.Failed(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xem bảng điểm UserId: {UserId}", userId);
            return ServiceResult<StudentScoreReportDto>.Failed(ScoreErrors.SystemError);
        }
    }

    /// <summary>
    /// Lấy hồ sơ giáo viên (Employee) của người đang đăng nhập; ném lỗi nếu không phải giáo viên.
    /// </summary>
    private async Task<Employee> ResolveTeacherAsync(long userId)
    {
        var user = await _accountRepository.GetAsync(userId, "EMPLOYEE");
        if (user is not Employee || !user.GetActiveRoleCodes().Contains("TEACHER"))
            throw ScoreErrors.NotTeacher;

        return (Employee)user;
    }

    /// <summary>
    /// Lấy bài kiểm tra và kiểm tra giáo viên có được phân công lớp/môn đó hay không.
    /// </summary>
    private async Task<Assessment> GetOwnedAssessmentAsync(long assessmentId, long employeeId)
    {
        var assessment = await _assessmentRepository.GetWithDetailsAsync(assessmentId)
            ?? throw ScoreErrors.AssessmentNotFound;

        if (!assessment.TeachingAssignment.IsTaughtBy(employeeId))
            throw ScoreErrors.NotYourClass;

        return assessment;
    }

    /// <summary>
    /// Điểm trung bình có trọng số = Σ(điểm × trọng số) / Σ(trọng số), chỉ tính các đầu điểm đã nhập.
    /// </summary>
    private static decimal? CalculateWeightedAverage(IEnumerable<StudentScore> scores)
    {
        var graded = scores.Where(s => s.Score.HasValue).ToList();
        var totalWeight = graded.Sum(s => s.Assessment.AssessmentType.GetWeight());
        if (totalWeight <= 0)
            return null;

        var weightedSum = graded.Sum(s => s.Score!.Value * s.Assessment.AssessmentType.GetWeight());
        return Math.Round(weightedSum / totalWeight, 2);
    }

    private static AssessmentSummaryDto ToAssessmentSummary(Assessment assessment)
    {
        return new AssessmentSummaryDto
        {
            Id = assessment.Id,
            AssessmentName = assessment.AssessmentName,
            TypeCode = assessment.AssessmentType.TypeCode,
            TypeName = assessment.AssessmentType.TypeName,
            ConductedAt = assessment.ConductedAt
        };
    }

    private static StudentScoreDto ToStudentScoreDto(StudentScore score)
    {
        return new StudentScoreDto
        {
            ScoreId = score.Id,
            CustomerId = score.CustomerId,
            AssessmentId = score.AssessmentId,
            Score = score.Score,
            Comment = score.Comment,
            IsLocked = score.IsLocked,
            UpdatedAt = score.UpdatedAt
        };
    }
}
