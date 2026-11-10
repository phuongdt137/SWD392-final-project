namespace HSMS.MVC.Models;

/// <summary>
/// Một dòng nhập điểm của giáo viên cho một học sinh trong bài kiểm tra.
/// </summary>
public sealed record ScoreEntry(long CustomerId, decimal? Score, string? Comment);
