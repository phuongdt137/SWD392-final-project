using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Score;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HSMS.MVC.Service;

/// <summary>
/// Thực thi nghiệp vụ Đánh giá & Điểm số theo Repository Pattern và cơ chế AppError.
/// Thứ tự kiểm tra trong từng phương thức bám theo sequence diagram của module.
/// </summary>
public class ScoreService : IScoreService
{
    private readonly IUserRepository _userRepository;
    private readonly ITeachingAssignmentRepository _assignmentRepository;
    private readonly IAssessmentRepository _assessmentRepository;
    private readonly IAssessmentTypeRepository _assessmentTypeRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IStudentScoreRepository _studentScoreRepository;
    private readonly INotificationService _notificationService;
    private readonly IScoreRealtimeNotifier _realtimeNotifier;
    private readonly ILogger<ScoreService> _logger;

    public ScoreService(
        IUserRepository userRepository,
        ITeachingAssignmentRepository assignmentRepository,
        IAssessmentRepository assessmentRepository,
        IAssessmentTypeRepository assessmentTypeRepository,
        ICustomerRepository customerRepository,
        IStudentScoreRepository studentScoreRepository,
        INotificationService notificationService,
        IScoreRealtimeNotifier realtimeNotifier,
        ILogger<ScoreService> logger)
    {
        _userRepository = userRepository;
        _assignmentRepository = assignmentRepository;
        _assessmentRepository = assessmentRepository;
        _assessmentTypeRepository = assessmentTypeRepository;
        _customerRepository = customerRepository;
        _studentScoreRepository = studentScoreRepository;
        _notificationService = notificationService;
        _realtimeNotifier = realtimeNotifier;
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

            teacher.EnsureTeaches(assignment);

            var assessmentType = await _assessmentTypeRepository.GetByIdAsync(dto.AssessmentTypeId)
                ?? throw ScoreErrors.AssessmentTypeNotFound;

            var sameScopeAssessments = await _assignmentRepository.GetAssessmentsInScopeAsync(
                assignment.SubjectId, assignment.ClassId, assignment.SemesterId);

            var assessment = teacher.CreateAssessment(
                assignment, assessmentType, dto.AssessmentName, dto.ConductedAt, sameScopeAssessments);

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
            var assessment = await GetOwnedAssessmentAsync(assessmentId, teacher);

            var students = await _customerRepository.GetActiveStudentsByClassAsync(assessment.TeachingAssignment.ClassId);

            var rows = students.Select(student =>
            {
                var score = assessment.FindStudentScore(student.Id);
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
            var assessment = await GetOwnedAssessmentAsync(dto.AssessmentId, teacher);

            var students = await _customerRepository.GetActiveStudentsByClassAsync(assessment.TeachingAssignment.ClassId);
            var roster = students.ToDictionary(s => s.Id, s => s.FullName);

            var entries = dto.Items.Select(i => new ScoreEntry(i.CustomerId, i.Score, i.Comment));

            // Giáo viên kiểm tra toàn bộ trước khi ghi, để không lưu một phần khi có lỗi.
            var applied = teacher.EnterScores(assessment, roster, entries);
            if (applied == 0)
                throw ScoreErrors.NothingToSave;

            await _assessmentRepository.SaveChangesAsync();
            await _realtimeNotifier.ScoresUpdatedAsync(assessment.Id, dto.Items.Select(i => i.CustomerId));

            return ServiceResult.Success("Lưu điểm thành công.");
        }
        catch (ScoreEntryValidationError ex)
        {
            return ServiceResult.Failed(ex, 400, ex.Details);
        }
        catch (AppError ex)
        {
            return ServiceResult.Failed(ex);
        }
        catch (DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        {
            // Vi phạm unique (CustomerId, AssessmentId): hai thao tác lưu cùng lúc cho cùng học sinh.
            // Các lỗi lưu khác rơi xuống nhánh Exception bên dưới để được ghi LogError và báo lỗi hệ thống.
            _logger.LogWarning(ex, "Xung đột khi nhập điểm AssessmentId: {AssessmentId}", dto.AssessmentId);
            return ServiceResult.Failed(ScoreErrors.ConcurrentUpdate);
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

            var oldScore = score.Score;
            teacher.UpdateScore(score, dto.NewScore, dto.Reason, ipAddress);

            await _studentScoreRepository.SaveChangesAsync();
            await _notificationService.NotifyScoreChangedAsync(score, oldScore, userId);
            await _realtimeNotifier.ScoresUpdatedAsync(score.AssessmentId, new[] { score.CustomerId });

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
            var assessment = await GetOwnedAssessmentAsync(assessmentId, teacher);

            var students = await _customerRepository.GetActiveStudentsByClassAsync(assessment.TeachingAssignment.ClassId);

            teacher.LockScores(assessment, students.Select(s => s.Id));
            await _assessmentRepository.SaveChangesAsync();
            await _realtimeNotifier.ScoresUpdatedAsync(assessment.Id, students.Select(s => s.Id));

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

            teacher.EnsureTeaches(score.Assessment.TeachingAssignment);

            var history = score.GetAuditLogs()
                .OrderByDescending(l => l.ChangedAt)
                .Select(l => ToAuditLogDto(l, includeIpAddress: true))
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
            var (viewer, student) = await ResolveViewableStudentAsync(studentCustomerId, userId);

            var semesters = student.GetScoreSemesters();

            // semesterId không thuộc học sinh đang xem (ví dụ phụ huynh vừa đổi con) thì rơi về học kỳ mặc định.
            Semester? selectedSemester = (semesterId.HasValue ? semesters.FirstOrDefault(s => s.Id == semesterId.Value) : null)
                ?? semesters.FirstOrDefault(s => s.IsCurrentSemester())
                ?? semesters.FirstOrDefault();

            var scores = selectedSemester != null
                ? student.GetStudentScores(selectedSemester.Id)
                : new List<StudentScore>();

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
                            ScoreId = s.Id,
                            ChangeCount = s.ScoreAuditLogs.Count,
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

    public async Task<ServiceResult<IReadOnlyList<ScoreAuditLogDto>>> GetMyScoreHistoryAsync(long scoreId, long? studentCustomerId, long userId)
    {
        try
        {
            var (_, student) = await ResolveViewableStudentAsync(studentCustomerId, userId);

            // Học sinh / phụ huynh không xem được địa chỉ IP của người sửa điểm.
            var history = student.GetScoreHistory(scoreId)
                .Select(l => ToAuditLogDto(l, includeIpAddress: false))
                .ToList();

            return ServiceResult<IReadOnlyList<ScoreAuditLogDto>>.Success(history);
        }
        catch (AppError ex)
        {
            return ServiceResult<IReadOnlyList<ScoreAuditLogDto>>.Failed(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xem lịch sử điểm ScoreId: {ScoreId}, UserId: {UserId}", scoreId, userId);
            return ServiceResult<IReadOnlyList<ScoreAuditLogDto>>.Failed(ScoreErrors.SystemError);
        }
    }

    public async Task<bool> CanWatchAssessmentAsync(long assessmentId, long userId)
    {
        try
        {
            var teacher = await ResolveTeacherAsync(userId);
            await GetOwnedAssessmentAsync(assessmentId, teacher);
            return true;
        }
        catch (AppError)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi kiểm tra quyền theo dõi AssessmentId: {AssessmentId}", assessmentId);
            return false;
        }
    }

    public async Task<bool> CanWatchStudentScoresAsync(long studentCustomerId, long userId)
    {
        try
        {
            var viewer = await _customerRepository.GetByUserIdWithChildrenAsync(userId);
            return viewer != null && viewer.CanViewScoresOf(studentCustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi kiểm tra quyền theo dõi điểm StudentCustomerId: {StudentCustomerId}", studentCustomerId);
            return false;
        }
    }

    /// <summary>
    /// Xác định học sinh mà người đang đăng nhập được xem điểm theo vai trò: học sinh chỉ xem chính mình,
    /// phụ huynh xem con (mặc định là con đầu tiên, chưa có con thì báo chưa liên kết).
    /// Trả về hồ sơ người xem và hồ sơ học sinh đã nạp kèm điểm.
    /// </summary>
    private async Task<(Customer Viewer, Customer Student)> ResolveViewableStudentAsync(long? studentCustomerId, long userId)
    {
        var viewer = await _customerRepository.GetByUserIdWithChildrenAsync(userId)
            ?? throw ScoreErrors.NotStudentOrParent;

        var user = await _userRepository.GetByIdWithDetailsAsync(userId);
        var roles = user?.GetActiveRoleCodes() ?? Array.Empty<string>();
        var isParent = roles.Contains("PARENT");
        var isStudentOnly = roles.Contains("STUDENT") && !isParent;

        long targetId;
        if (studentCustomerId.HasValue)
            targetId = studentCustomerId.Value;
        else if (isStudentOnly)
            targetId = viewer.Id;
        else if (viewer.Children.Count > 0)
            targetId = viewer.Children.First().Id;
        else if (isParent && !roles.Contains("STUDENT"))
            throw ScoreErrors.NoLinkedStudent;
        else
            targetId = viewer.Id;

        if (isStudentOnly ? targetId != viewer.Id : !viewer.CanViewScoresOf(targetId))
            throw ScoreErrors.NotStudentOrParent;

        var student = await _customerRepository.GetWithScoresAsync(targetId)
            ?? throw ScoreErrors.StudentNotFound;

        return (viewer, student);
    }

    private static ScoreAuditLogDto ToAuditLogDto(ScoreAuditLog log, bool includeIpAddress)
    {
        return new ScoreAuditLogDto
        {
            ChangedAt = log.ChangedAt,
            OldScore = log.OldScore,
            NewScore = log.NewScore,
            Difference = log.GetScoreDifference(),
            ChangeReason = log.ChangeReason,
            ChangedByName = log.ChangedByEmployee?.FullName ?? string.Empty,
            IpAddress = includeIpAddress ? log.IpAddress : null
        };
    }

    /// <summary>
    /// Lấy hồ sơ giáo viên (Employee) của người đang đăng nhập; ném lỗi nếu không phải giáo viên.
    /// </summary>
    private async Task<Employee> ResolveTeacherAsync(long userId)
    {
        var user = await _userRepository.GetByIdWithDetailsAsync(userId);
        if (user?.EmployeeProfile == null || !user.GetActiveRoleCodes().Contains("TEACHER"))
            throw ScoreErrors.NotTeacher;

        return user.EmployeeProfile;
    }

    /// <summary>
    /// Lấy bài kiểm tra và kiểm tra giáo viên có được phân công lớp/môn đó hay không.
    /// </summary>
    private async Task<Assessment> GetOwnedAssessmentAsync(long assessmentId, Employee teacher)
    {
        var assessment = await _assessmentRepository.GetWithDetailsAsync(assessmentId)
            ?? throw ScoreErrors.AssessmentNotFound;

        teacher.EnsureTeaches(assessment.TeachingAssignment);

        return assessment;
    }

    /// <summary>
    /// Điểm trung bình môn: lấy trung bình cộng các đầu điểm trong từng loại (TX, GK, CK...), rồi tính
    /// Σ(TB loại × trọng số loại) / Σ(trọng số loại), chỉ xét các loại đã có điểm. Nhờ vậy trọng số của một
    /// loại luôn được tính một lần dù loại đó có bao nhiêu đầu điểm. Làm tròn 2 chữ số, .5 làm tròn lên.
    /// </summary>
    private static decimal? CalculateWeightedAverage(IEnumerable<StudentScore> scores)
    {
        var typeAverages = scores
            .Where(s => s.Score.HasValue)
            .GroupBy(s => s.Assessment.AssessmentTypeId)
            .Select(group => new
            {
                Weight = group.First().Assessment.AssessmentType.GetWeight(),
                Average = group.Average(s => s.Score!.Value)
            })
            .ToList();

        var totalWeight = typeAverages.Sum(t => t.Weight);
        if (totalWeight <= 0)
            return null;

        var weightedSum = typeAverages.Sum(t => t.Average * t.Weight);
        return Math.Round(weightedSum / totalWeight, 2, MidpointRounding.AwayFromZero);
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
