namespace HSMS.MVC.DTOs.Score;

public class AssessmentTypeDto
{
    public long Id { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public int MinEntries { get; set; }
    public int? MaxEntries { get; set; }
}

public class AssessmentSummaryDto
{
    public long Id { get; set; }
    public string AssessmentName { get; set; } = string.Empty;
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public DateTime? ConductedAt { get; set; }
}

public class TeachingAssignmentSummaryDto
{
    public long AssignmentId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SemesterName { get; set; } = string.Empty;
    public IReadOnlyList<AssessmentSummaryDto> Assessments { get; set; } = Array.Empty<AssessmentSummaryDto>();
}

public class CreateAssessmentDto
{
    public long AssignmentId { get; set; }
    public long AssessmentTypeId { get; set; }
    public string AssessmentName { get; set; } = string.Empty;
    public DateTime? ConductedAt { get; set; }
}

public class ScoreSheetRowDto
{
    public long CustomerId { get; set; }
    public string? StudentCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public long? ScoreId { get; set; }
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
}

public class ScoreSheetDto
{
    public long AssessmentId { get; set; }
    public string AssessmentName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public IReadOnlyList<ScoreSheetRowDto> Rows { get; set; } = Array.Empty<ScoreSheetRowDto>();
}

public class ScoreEntryItemDto
{
    public long CustomerId { get; set; }
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
}

public class EnterScoresDto
{
    public long AssessmentId { get; set; }
    public List<ScoreEntryItemDto> Items { get; set; } = new();
}

public class UpdateScoreDto
{
    public long ScoreId { get; set; }
    public decimal NewScore { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class StudentScoreDto
{
    public long ScoreId { get; set; }
    public long CustomerId { get; set; }
    public long AssessmentId { get; set; }
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class LockResultDto
{
    public long AssessmentId { get; set; }
    public int LockedCount { get; set; }
}

public class ScoreAuditLogDto
{
    public DateTime ChangedAt { get; set; }
    public decimal? OldScore { get; set; }
    public decimal? NewScore { get; set; }
    public decimal Difference { get; set; }
    public string? ChangeReason { get; set; }
    public string ChangedByName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
}

public class OptionDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AssessmentScoreItemDto
{
    public string AssessmentName { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public decimal? Score { get; set; }
}

public class SubjectScoreDto
{
    public string SubjectName { get; set; } = string.Empty;
    public decimal? WeightedAverage { get; set; }
    public IReadOnlyList<AssessmentScoreItemDto> Items { get; set; } = Array.Empty<AssessmentScoreItemDto>();
}

public class StudentScoreReportDto
{
    public long StudentCustomerId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public long? SelectedSemesterId { get; set; }
    public IReadOnlyList<OptionDto> Semesters { get; set; } = Array.Empty<OptionDto>();
    public IReadOnlyList<OptionDto> Children { get; set; } = Array.Empty<OptionDto>();
    public IReadOnlyList<SubjectScoreDto> Subjects { get; set; } = Array.Empty<SubjectScoreDto>();
}
