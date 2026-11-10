using HSMS.MVC.Common;

namespace HSMS.MVC.Models;

/// <summary>
/// Thực thể Nhân viên / Cán bộ / Giáo viên (Hồ sơ nghiệp vụ liên kết 1:1 với User).
/// </summary>
public class Employee
{
    public long Id { get; private set; }
    public long UserId { get; private set; }
    public string EmployeeCode { get; private set; } = null!;
    public string? Gender { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Address { get; private set; }
    public string? Department { get; private set; }
    public string? Position { get; private set; }
    public string? Specialization { get; private set; }
    public int? MaxPeriodsPerWeek { get; private set; }
    public string? Qualification { get; private set; }
    public DateTime? HireDate { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation tới User (1:1)
    public virtual User User { get; private set; } = null!;

    // Helper properties ánh xạ sang User để giữ tương thích ngược
    public string FullName => User?.FullName ?? string.Empty;
    public string Username => User?.Username ?? string.Empty;
    public string Email => User?.Email ?? string.Empty;
    public string? PhoneNumber => User?.PhoneNumber;
    public string? AvatarUrl => User?.AvatarUrl;
    public bool IsActive => User?.IsActive ?? true;

    // Academic & School Relations
    public virtual ICollection<Class> HomeroomClasses { get; private set; } = new List<Class>();
    public virtual ICollection<TeachingAssignment> TeachingAssignments { get; private set; } = new List<TeachingAssignment>();
    public virtual ICollection<LeaveRequest> ReviewedLeaveRequests { get; private set; } = new List<LeaveRequest>();
    public virtual ICollection<AttendanceRecord> RecordedAttendanceRecords { get; private set; } = new List<AttendanceRecord>();
    public virtual ICollection<ScoreAuditLog> ScoreAuditLogs { get; private set; } = new List<ScoreAuditLog>();
    public virtual ICollection<ConductEvaluation> ConductEvaluations { get; private set; } = new List<ConductEvaluation>();
    public virtual ICollection<SemesterReport> FinalizedSemesterReports { get; private set; } = new List<SemesterReport>();
    public virtual ICollection<StudentTransfer> RecordedStudentTransfers { get; private set; } = new List<StudentTransfer>();
    public virtual ICollection<StudentAnnualReport> FinalizedStudentAnnualReports { get; private set; } = new List<StudentAnnualReport>();

    protected Employee() { }

    public Employee(
        long userId,
        string employeeCode,
        string? department = null,
        string? position = null,
        string? specialization = null,
        string? qualification = null,
        DateTime? hireDate = null,
        int? maxPeriodsPerWeek = null,
        string? gender = null,
        DateTime? dateOfBirth = null,
        string? address = null)
    {
        if (userId <= 0)
            throw new AppError("Id người dùng không hợp lệ.", "INVALID_USER_ID");

        if (string.IsNullOrWhiteSpace(employeeCode))
            throw new AppError("Mã nhân viên không được để trống.", "EMPLOYEE_CODE_REQUIRED");

        if (maxPeriodsPerWeek.HasValue && maxPeriodsPerWeek.Value < 0)
            throw new AppError("Số tiết tối đa trong tuần phải >= 0.", "INVALID_MAX_PERIODS");

        UserId = userId;
        EmployeeCode = employeeCode.Trim();
        Department = department?.Trim();
        Position = position?.Trim();
        Specialization = specialization?.Trim();
        Qualification = qualification?.Trim();
        HireDate = hireDate;
        MaxPeriodsPerWeek = maxPeriodsPerWeek;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Address = address;
    }

    public void UpdateEmployeeInfo(
        string? department,
        string? position,
        string? specialization,
        string? qualification,
        int? maxPeriodsPerWeek,
        string? gender,
        DateTime? dateOfBirth,
        string? address)
    {
        if (maxPeriodsPerWeek.HasValue && maxPeriodsPerWeek.Value < 0)
            throw new AppError("Số tiết tối đa trong tuần phải >= 0.", "INVALID_MAX_PERIODS");

        Department = department?.Trim();
        Position = position?.Trim();
        Specialization = specialization?.Trim();
        Qualification = qualification?.Trim();
        MaxPeriodsPerWeek = maxPeriodsPerWeek;
        Gender = gender;
        DateOfBirth = dateOfBirth;
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    // ===== Đánh giá & Điểm số (giáo viên) =====

    /// <summary>
    /// Giáo viên phải được phân công ACTIVE cho lớp/môn thì mới thao tác được trên bài kiểm tra và điểm của lớp/môn đó.
    /// </summary>
    public void EnsureTeaches(TeachingAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        if (!assignment.IsTaughtBy(Id))
            throw ScoreErrors.NotYourClass;
    }

    /// <summary>
    /// Tạo bài kiểm tra mới trong một phân công của chính giáo viên. Kiểm tra quyền, môn có chấm điểm,
    /// ngày tổ chức nằm trong học kỳ và giới hạn số đầu điểm.
    /// sameScopeAssessments: mọi bài kiểm tra đã có của cùng môn + lớp + học kỳ (qua tất cả phân công, kể cả của
    /// giáo viên khác), vì giới hạn đầu điểm áp dụng cho học sinh chứ không riêng cho một phân công.
    /// </summary>
    public Assessment CreateAssessment(
        TeachingAssignment assignment,
        AssessmentType assessmentType,
        string assessmentName,
        DateTime? conductedAt,
        IEnumerable<Assessment> sameScopeAssessments)
    {
        ArgumentNullException.ThrowIfNull(assessmentType);
        ArgumentNullException.ThrowIfNull(sameScopeAssessments);
        EnsureTeaches(assignment);

        if (!assignment.Subject.IsScoreEvaluated())
            throw ScoreErrors.SubjectNotScored;

        if (conductedAt.HasValue
            && (conductedAt.Value.Date < assignment.Semester.StartDate.Date || conductedAt.Value.Date > assignment.Semester.EndDate.Date))
            throw ScoreErrors.ConductedAtOutOfSemester;

        var currentCount = sameScopeAssessments.Count(a => a.AssessmentTypeId == assessmentType.Id);
        if (!assessmentType.IsValidEntryCount(currentCount + 1))
            throw ScoreErrors.MaxEntriesExceeded;

        var assessment = new Assessment(assignment.Id, assessmentType.Id, assessmentName, conductedAt);
        assignment.AddAssessment(assessment);
        return assessment;
    }

    /// <summary>
    /// Nhập điểm lần đầu cho các học sinh của lớp. Kiểm tra toàn bộ trước khi ghi nên lỗi ở bất kỳ dòng nào
    /// cũng không để lại dữ liệu nhập dở. roster: Id học sinh đang học lớp → họ tên.
    /// Học sinh đã có dòng điểm nhưng chưa có điểm (chỉ có nhận xét) vẫn nhập tiếp được điểm ở đây.
    /// Trả về số dòng thực sự được thêm hoặc thay đổi (0 nghĩa là không có gì để lưu).
    /// </summary>
    public int EnterScores(Assessment assessment, IReadOnlyDictionary<long, string> roster, IEnumerable<ScoreEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(assessment);
        ArgumentNullException.ThrowIfNull(roster);
        ArgumentNullException.ThrowIfNull(entries);

        EnsureTeaches(assessment.TeachingAssignment);

        if (assessment.HasLockedScores())
            throw ScoreErrors.ScoreLocked;

        // Giữ lại dòng trống nếu học sinh đang có dòng chỉ-nhận-xét: đó là yêu cầu xóa nhận xét cũ.
        bool IsClearingComment(ScoreEntry e)
        {
            var current = assessment.FindStudentScore(e.CustomerId);
            return current != null && current.Score == null && !string.IsNullOrEmpty(current.Comment);
        }

        var items = entries
            .Where(e => e.Score.HasValue || !string.IsNullOrWhiteSpace(e.Comment) || IsClearingComment(e))
            .ToList();

        var errors = new List<string>();
        foreach (var item in items)
        {
            if (!roster.TryGetValue(item.CustomerId, out var name))
            {
                errors.Add("Học sinh không xác định: không thuộc lớp của phân công này.");
                continue;
            }

            if (item.Score.HasValue)
            {
                var scoreError = StudentScore.GetScoreError(item.Score.Value);
                if (scoreError != null)
                {
                    errors.Add($"{name}: {char.ToLowerInvariant(scoreError[0])}{scoreError[1..]}");
                    continue;
                }
            }

            var existing = assessment.FindStudentScore(item.CustomerId);
            if (existing?.Score != null && item.Score.HasValue && item.Score.Value != existing.Score.Value)
                errors.Add($"{name}: đã có điểm, vui lòng dùng chức năng sửa điểm để thay đổi.");
        }

        if (errors.Count > 0)
            throw new ScoreEntryValidationError(errors);

        var applied = 0;
        foreach (var item in items)
        {
            var existing = assessment.FindStudentScore(item.CustomerId);
            if (existing == null)
            {
                assessment.AddStudentScore(item.CustomerId, item.Score, item.Comment);
                applied++;
            }
            else if (existing.Score == null && item.Score.HasValue)
            {
                existing.EnterScore(item.Score.Value, item.Comment);
                applied++;
            }
            else if (existing.Score == null
                     && (existing.Comment ?? string.Empty) != (item.Comment ?? string.Empty).Trim())
            {
                existing.SetComment(item.Comment);
                applied++;
            }
        }

        return applied;
    }

    /// <summary>
    /// Sửa điểm đã nhập: bắt buộc có lý do, ghi ScoreAuditLog do chính giáo viên này thực hiện.
    /// </summary>
    public ScoreAuditLog UpdateScore(StudentScore score, decimal newScore, string reason, string? ipAddress = null)
    {
        ArgumentNullException.ThrowIfNull(score);
        EnsureTeaches(score.Assessment.TeachingAssignment);

        if (score.IsLocked)
            throw ScoreErrors.ScoreLocked;

        return score.UpdateScore(newScore, reason, this, ipAddress);
    }

    /// <summary>
    /// Khóa toàn bộ điểm của một bài kiểm tra thuộc lớp/môn mình dạy.
    /// </summary>
    public void LockScores(Assessment assessment, IEnumerable<long> rosterCustomerIds)
    {
        ArgumentNullException.ThrowIfNull(assessment);
        EnsureTeaches(assessment.TeachingAssignment);
        assessment.LockAllScores(rosterCustomerIds);
    }

    public int CurrentWorkload(long semesterId)
    {
        return TeachingAssignments
            .Where(ta => ta.SemesterId == semesterId && ta.Status == "ACTIVE")
            .SelectMany(ta => ta.TimetableEntries)
            .Count(te => te.Status == "ACTIVE");
    }
}
