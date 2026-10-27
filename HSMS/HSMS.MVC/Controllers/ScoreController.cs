using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using HSMS.MVC.Common;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.DTOs.Score;
using HSMS.MVC.Service;
using HSMS.MVC.ViewModels.Score;

namespace HSMS.MVC.Controllers;

/// <summary>
/// Đánh giá & Điểm số: giáo viên tạo bài kiểm tra, nhập/sửa/khóa điểm;
/// học sinh và phụ huynh xem bảng điểm.
/// </summary>
public class ScoreController : Controller
{
    private readonly IScoreService _scoreService;

    public ScoreController(IScoreService scoreService)
    {
        _scoreService = scoreService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = GetCurrentUser();
        var denied = RequireRole(user, "TEACHER");
        if (denied != null) return denied;

        var result = await _scoreService.GetMyTeachingAssignmentsAsync(user!.Id);
        if (!result.IsSuccess)
            TempData["ErrorMessage"] = result.Message;

        return View(result.Data ?? Array.Empty<TeachingAssignmentSummaryDto>());
    }

    [HttpGet]
    public async Task<IActionResult> CreateAssessment(long? assignmentId)
    {
        var user = GetCurrentUser();
        var denied = RequireRole(user, "TEACHER");
        if (denied != null) return denied;

        await PopulateCreateOptionsAsync(user!.Id);
        return View(new CreateAssessmentViewModel { AssignmentId = assignmentId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAssessment(CreateAssessmentViewModel model)
    {
        var user = GetCurrentUser();
        var denied = RequireRole(user, "TEACHER");
        if (denied != null) return denied;

        if (!ModelState.IsValid)
        {
            await PopulateCreateOptionsAsync(user!.Id);
            return View(model);
        }

        var dto = new CreateAssessmentDto
        {
            AssignmentId = model.AssignmentId,
            AssessmentTypeId = model.AssessmentTypeId,
            AssessmentName = model.AssessmentName,
            ConductedAt = model.ConductedAt
        };

        var result = await _scoreService.CreateAssessmentAsync(dto, user!.Id);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Tạo bài kiểm tra thất bại.");
            await PopulateCreateOptionsAsync(user.Id);
            return View(model);
        }

        TempData["SuccessMessage"] = result.Errors.FirstOrDefault();
        return RedirectToAction(nameof(ScoreSheet), new { id = result.Data });
    }

    [HttpGet]
    public async Task<IActionResult> ScoreSheet(long id)
    {
        var user = GetCurrentUser();
        var denied = RequireRole(user, "TEACHER");
        if (denied != null) return denied;

        var result = await _scoreService.GetScoreSheetAsync(id, user!.Id);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Index));
        }

        return View(result.Data);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnterScores(EnterScoresViewModel model)
    {
        var user = GetCurrentUser();
        var denied = RequireRole(user, "TEACHER");
        if (denied != null) return denied;

        var dto = new EnterScoresDto
        {
            AssessmentId = model.AssessmentId,
            Items = model.Items.Select(i => new ScoreEntryItemDto
            {
                CustomerId = i.CustomerId,
                Score = i.Score,
                Comment = i.Comment
            }).ToList()
        };

        var result = await _scoreService.EnterScoresAsync(dto, user!.Id);
        SetResultMessages(result);

        return RedirectToAction(nameof(ScoreSheet), new { id = model.AssessmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateScore(UpdateScoreViewModel model)
    {
        var user = GetCurrentUser();
        var denied = RequireRole(user, "TEACHER");
        if (denied != null) return denied;

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage
                ?? "Dữ liệu sửa điểm không hợp lệ.";
            return RedirectToAction(nameof(ScoreSheet), new { id = model.AssessmentId });
        }

        var dto = new UpdateScoreDto
        {
            ScoreId = model.ScoreId,
            NewScore = model.NewScore,
            Reason = model.Reason
        };

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _scoreService.UpdateScoreAsync(dto, user!.Id, ipAddress);
        SetResultMessages(result);

        return RedirectToAction(nameof(ScoreSheet), new { id = model.AssessmentId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LockScores(long assessmentId)
    {
        var user = GetCurrentUser();
        var denied = RequireRole(user, "TEACHER");
        if (denied != null) return denied;

        var result = await _scoreService.LockScoresAsync(assessmentId, user!.Id);
        SetResultMessages(result);

        return RedirectToAction(nameof(ScoreSheet), new { id = assessmentId });
    }

    [HttpGet]
    public async Task<IActionResult> ScoreHistory(long scoreId)
    {
        var user = GetCurrentUser();
        var denied = RequireRole(user, "TEACHER");
        if (denied != null) return denied;

        var result = await _scoreService.GetScoreHistoryAsync(scoreId, user!.Id);
        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, result.Message);

        return PartialView("_ScoreHistory", result.Data ?? Array.Empty<ScoreAuditLogDto>());
    }

    [HttpGet]
    public async Task<IActionResult> MyScores(long? studentId, long? semesterId)
    {
        var user = GetCurrentUser();
        var denied = RequireRole(user, "STUDENT", "PARENT");
        if (denied != null) return denied;

        var result = await _scoreService.GetStudentScoresAsync(studentId, semesterId, user!.Id);
        if (result.ErrorCode == ScoreErrors.NotStudentOrParent.Code)
            return RedirectToAction("AccessDenied", "Auth");

        if (!result.IsSuccess)
            TempData["ErrorMessage"] = result.Message;

        return View(result.Data ?? new StudentScoreReportDto());
    }

    private async Task PopulateCreateOptionsAsync(long userId)
    {
        var assignments = await _scoreService.GetMyTeachingAssignmentsAsync(userId);
        var types = await _scoreService.GetAssessmentTypesAsync();

        ViewBag.Assignments = (assignments.Data ?? Array.Empty<TeachingAssignmentSummaryDto>())
            .Select(a => new SelectListItem
            {
                Value = a.AssignmentId.ToString(),
                Text = $"{a.SubjectName} - Lớp {a.ClassName} ({a.SemesterName})"
            })
            .ToList();

        ViewBag.AssessmentTypes = (types.Data ?? Array.Empty<AssessmentTypeDto>())
            .Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"{t.TypeName} ({t.Weight}%)"
            })
            .ToList();
    }

    private void SetResultMessages(ServiceResult result)
    {
        // ServiceResult.Success lưu thông báo trong Errors, còn Message chỉ có khi thất bại.
        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = result.Errors.FirstOrDefault();
            return;
        }

        TempData["ErrorMessage"] = result.Message;
        if (result.Errors.Count > 1)
            TempData["ErrorDetails"] = string.Join("\n", result.Errors.Skip(1));
    }

    private LoginResponseDto? GetCurrentUser()
    {
        if (User.Identity?.IsAuthenticated != true) return null;
        var userJson = HttpContext.Session.GetString(AuthController.SessionUserKey);
        if (string.IsNullOrEmpty(userJson))
            return null;

        try
        {
            return JsonSerializer.Deserialize<LoginResponseDto>(userJson);
        }
        catch
        {
            return null;
        }
    }

    private IActionResult? RequireRole(LoginResponseDto? user, params string[] roles)
    {
        if (user == null)
            return RedirectToAction("Login", "Auth", new { returnUrl = Request.Path.Value });

        if (!roles.Any(role => user.Roles.Contains(role, StringComparer.OrdinalIgnoreCase)))
            return RedirectToAction("AccessDenied", "Auth");

        return null;
    }
}

