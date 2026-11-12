using System.Text.Json;
using HSMS.MVC.Common;
using HSMS.MVC.Data;
using HSMS.MVC.DTOs.Auth;
using HSMS.MVC.DTOs.Timetable;
using HSMS.MVC.Repositories.Implementations;
using HSMS.MVC.Service.Implementations;
using HSMS.MVC.Service.Interfaces;
using HSMS.MVC.ViewModels.Timetable;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HSMS.MVC.Controllers;

/// <summary>
/// Controller xử lý phân hệ Thời khóa biểu (TimetableController).
/// Tuân thủ quy trình gọi: Controller -> Service -> Repository.
/// </summary>
[Authorize]
public class TimetableController : Controller
{
    private readonly ITimetableService _timetableService;

    /// <summary>
    /// Constructor hỗ trợ cả DI tiêu chuẩn lẫn fallback nếu chưa khai báo trong Program.cs.
    /// Giúp code chạy ngay mà không bắt buộc phải sửa Program.cs.
    /// </summary>
    [ActivatorUtilitiesConstructor]
    public TimetableController(AppDbContext db, IServiceProvider serviceProvider)
    {
        _timetableService = serviceProvider.GetService<ITimetableService>()
            ?? new TimetableService(new TimetableRepository(db));
    }

    public TimetableController(ITimetableService timetableService)
    {
        _timetableService = timetableService;
    }

    /// <summary>
    /// Hiển thị thời khóa biểu tuần theo vai trò:
    /// - Staff/Principal: xem toàn trường hoặc lọc theo lớp/giáo viên/phòng.
    /// - Teacher: chỉ xem lịch dạy của mình.
    /// - Student: chỉ xem lịch học của lớp mình.
    /// - Parent: chỉ xem lịch học của con.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(TimetableFilterDto filter)
    {
        var currentUser = GetCurrentUser();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _timetableService.GetTimetableIndexViewModelAsync(filter, currentUser);
        if (!result.IsSuccess)
        {
            TempData["ErrorMessage"] = result.Message;
        }

        return View(result.Data);
    }

    /// <summary>
    /// Giao diện form đặt lịch dạy / xếp thời khóa biểu dành cho Cán bộ học vụ (Staff).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "STAFF,PRINCIPAL,ADMIN")]
    public async Task<IActionResult> Create(long? classId, long? semesterId)
    {
        var initialRequest = new CreateTeachingAssignmentRequest
        {
            ClassId = classId ?? 0,
            SemesterId = semesterId ?? 0,
            DayOfWeek = 1
        };

        var vm = await _timetableService.GetCreateTimetableViewModelAsync(initialRequest);
        return View(vm);
    }

    /// <summary>
    /// Xử lý lưu form tạo thời khóa biểu từ Web UI (Staff).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "STAFF,PRINCIPAL,ADMIN")]
    public async Task<IActionResult> Create(CreateTimetableViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var repopulatedVm = await _timetableService.GetCreateTimetableViewModelAsync(model.Request);
            return View(repopulatedVm);
        }

        // Gọi Service tạo phân công và thời khóa biểu (Step 3)
        var result = await _timetableService.CreateAssignmentAndTimetableAsync(model.Request);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.Message ?? "Không thể đặt lịch dạy.");
            var repopulatedVm = await _timetableService.GetCreateTimetableViewModelAsync(model.Request);
            return View(repopulatedVm);
        }

        // Thành công: hiển thị thông báo "Assignment created successfully" (Step 22)
        TempData["SuccessMessage"] = "Đặt lịch dạy thành công!";
        return RedirectToAction(nameof(Index), new { classId = model.Request.ClassId, semesterId = model.Request.SemesterId });
    }

    /// <summary>
    /// API Endpoint tạo lịch dạy: POST /api/teaching-assignment(request).
    /// Tuân thủ chính xác đường dẫn và mã trả về HTTP 201 trong Sequence Diagram.
    /// </summary>
    [HttpPost("/api/teaching-assignment")]
    [Authorize(Roles = "STAFF,PRINCIPAL,ADMIN")]
    public async Task<IActionResult> CreateAssignmentAndTimetableApi([FromBody] CreateTeachingAssignmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Step 3: createAssignmentAndTimetable(request)
        var result = await _timetableService.CreateAssignmentAndTimetableAsync(request);
        if (!result.IsSuccess)
        {
            return StatusCode(result.StatusCode, new
            {
                success = false,
                errorCode = result.ErrorCode,
                message = result.Message,
                errors = result.Errors
            });
        }

        // Step 20: mapToResponseDto(result)
        var responseDto = await _timetableService.MapToResponseDtoAsync(result.Data!);

        // Step 21: HTTP 201 Created(response)
        return StatusCode(StatusCodes.Status201Created, responseDto);
    }

    private LoginResponseDto? GetCurrentUser()
    {
        try
        {
            var userJson = HttpContext.Session.GetString(SessionKeys.CurrentUser);
            if (string.IsNullOrWhiteSpace(userJson)) return null;
            return JsonSerializer.Deserialize<LoginResponseDto>(userJson);
        }
        catch
        {
            return null;
        }
    }
}

