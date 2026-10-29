using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using HSMS.MVC.Common;
using HSMS.MVC.Controllers;
using HSMS.MVC.DTOs.Notifications;
using HSMS.MVC.Models;
using HSMS.MVC.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;

namespace HSMS.Attendance.Checks;

internal static class UiChecks
{
    public static async Task RunAsync(Action<bool, string> assert)
    {
        var fixture = new Fixture();
        var server = new InProcessServer();
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = typeof(AttendanceController).Assembly.FullName,
            EnvironmentName = "Development"
        });
        builder.Logging.ClearProviders();
        builder.WebHost.UseServer(server);
        builder.Services.AddControllersWithViews().AddApplicationPart(typeof(AttendanceController).Assembly).AddSessionStateTempDataProvider();
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession();
        builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
        builder.Services.AddAuthentication("AttendanceTest").AddScheme<AuthenticationSchemeOptions, TestAuthentication>("AttendanceTest", _ => { });
        builder.Services.AddAuthorization();
        builder.Services.AddSingleton(fixture);
        builder.Services.AddSingleton<IAttendanceService>(fixture.Service);
        builder.Services.AddSingleton<INotificationService, SilentNotifications>();

        await using var app = builder.Build();
        app.UseRouting();
        app.UseSession();
        app.Use(async (context, next) =>
        {
            if (context.Request.Headers.ContainsKey("X-Test-Anonymous"))
                context.Session.Remove(SessionKeys.CurrentUser);
            else
                context.Session.SetString(SessionKeys.CurrentUser, JsonSerializer.Serialize(fixture.Repo.User));
            await next();
        });
        app.UseAuthentication();
        app.UseMiddleware<SessionAuthMiddleware>();
        app.UseAuthorization();
        app.MapControllerRoute("default", "{controller=Attendance}/{action=Index}/{id?}");
        await app.StartAsync();
        try
        {
            using var client = new HttpClient(server.CreateHandler())
            {
                BaseAddress = new Uri("http://attendance.test")
            };
            var schedule = await client.GetAsync("/Attendance?date=2026-10-28");
            assert(schedule.IsSuccessStatusCode, "Schedule HTTP status: " + await schedule.Content.ReadAsStringAsync());
            var scheduleHtml = WebUtility.HtmlDecode(await schedule.Content.ReadAsStringAsync());
            assert(scheduleHtml.Contains("Lịch điểm danh") && scheduleHtml.Contains("10A1"), "Schedule Razor output");
            assert(!scheduleHtml.Contains("Xem lịch sử"), "Teacher must not see a history link");
            assert(!scheduleHtml.Contains("name=\"Date\"") && !scheduleHtml.Contains("name=\"ClassId\"") && !scheduleHtml.Contains("am-filters"),
                "Teacher schedule must not render filters");
            var forgedSchedule = WebUtility.HtmlDecode(await client.GetStringAsync("/Attendance?Date=2026-10-21&ClassId=999"));
            assert(forgedSchedule.Contains("28/10/2026") && !forgedSchedule.Contains("21/10/2026") && forgedSchedule.Contains("date=2026-10-28"),
                "Teacher URL filters cannot change today's schedule");

            var sheet = await client.GetAsync("/Attendance/Mark?timetableEntryId=400&date=2026-10-28");
            var html = await sheet.Content.ReadAsStringAsync();
            assert(sheet.IsSuccessStatusCode && html.Contains("attendance-form"), "Sheet Razor output: " + html);
            var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;
            assert(token.Length > 0, "Antiforgery token must exist");

            fixture.ApprovedLeave();
            var firstLeaveSheet = await client.GetStringAsync("/Attendance/Mark?timetableEntryId=400&date=2026-10-28");
            var firstLeaveStatus = Regex.Match(firstLeaveSheet, @"<select[^>]*name=""Rows\[0\]\.Status""[^>]*>.*?</select>", RegexOptions.Singleline).Value;
            var firstLeaveRequest = Regex.Match(firstLeaveSheet, @"<select[^>]*name=""Rows\[0\]\.LeaveRequestId""[^>]*>.*?</select>", RegexOptions.Singleline).Value;
            assert(firstLeaveStatus.Contains("value=\"EXCUSED\" selected") && firstLeaveRequest.Contains("value=\"50\" selected"),
                "First-time attendance automatically selects EXCUSED and the approved request");
            assert(fixture.Repo.Records.Count == 0, "Automatic selections are only a preview until Save");
            fixture.Repo.Leaves.Clear();

            var withoutToken = await client.PostAsync("/Attendance/Save", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["TimetableEntryId"] = "400", ["Date"] = "2026-10-28"
            }));
            assert(withoutToken.StatusCode == HttpStatusCode.BadRequest, "Missing antiforgery token must be rejected");

            var incomplete = await client.PostAsync("/Attendance/Save", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token),
                ["TimetableEntryId"] = "400", ["Date"] = "2026-10-28",
                ["Rows[0].CustomerId"] = "10", ["Rows[0].Status"] = ""
            }));
            assert(incomplete.StatusCode == HttpStatusCode.BadRequest && fixture.Repo.Records.Count == 0 &&
                WebUtility.HtmlDecode(await incomplete.Content.ReadAsStringAsync()).Contains("Còn học sinh chưa được điểm danh"),
                "An unmarked student is reported on the form before saving");

            var saved = await client.PostAsync("/Attendance/Save", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token),
                ["TimetableEntryId"] = "400", ["Date"] = "2026-10-28",
                ["Rows[0].CustomerId"] = "10", ["Rows[0].Status"] = "LATE",
                ["userId"] = "999", ["user.EmployeeId"] = "999", ["user.Roles[0]"] = "STAFF"
            }));
            assert(saved.StatusCode == HttpStatusCode.Redirect, "Valid POST must redirect: " + await saved.Content.ReadAsStringAsync());
            assert(fixture.Repo.Records.Count == 1 && fixture.Repo.Records.Single().RecordedByEmployeeId == 30 && fixture.Repo.Notifications.Single().SenderId == 1, "Controller uses authenticated ID, not submitted ID");

            assert((await client.GetAsync("/Attendance/History")).StatusCode == HttpStatusCode.Forbidden, "Teacher history URL denied");
            assert((await client.GetAsync("/Attendance/Audit?id=1000")).StatusCode == HttpStatusCode.Forbidden, "Teacher audit URL denied");
            var teacherSheet = await client.GetStringAsync("/Attendance/Mark?timetableEntryId=400&date=2026-10-28");
            assert(!teacherSheet.Contains("/Attendance/Audit"), "Teacher sheet must not show audit links");

            var approvedLeave = fixture.ApprovedLeave();
            var autoSheet = await client.GetStringAsync("/Attendance/Mark?timetableEntryId=400&date=2026-10-28");
            var autoStatus = Regex.Match(autoSheet, @"<select[^>]*name=""Rows\[0\]\.Status""[^>]*>.*?</select>", RegexOptions.Singleline).Value;
            var autoLeave = Regex.Match(autoSheet, @"<select[^>]*name=""Rows\[0\]\.LeaveRequestId""[^>]*>.*?</select>", RegexOptions.Singleline).Value;
            assert(autoStatus.Contains("value=\"EXCUSED\" selected") && !autoStatus.Contains("value=\"PRESENT\""),
                "Approved leave preselects EXCUSED and does not offer other attendance statuses");
            assert(autoLeave.Contains($"value=\"{approvedLeave.Id}\" selected") && !autoLeave.Contains("value=\"\""),
                "Approved leave is selected automatically without an empty request option");
            assert(fixture.Repo.Records.Single().Status == "LATE", "Opening a sheet must not save the suggested leave result");

            var wrongLeaveStatus = await client.PostAsync("/Attendance/Save", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token),
                ["TimetableEntryId"] = "400", ["Date"] = "2026-10-28",
                ["Rows[0].CustomerId"] = "10", ["Rows[0].Status"] = "PRESENT", ["ChangeReason"] = "Kiểm tra đơn nghỉ"
            }));
            assert(wrongLeaveStatus.StatusCode == HttpStatusCode.BadRequest &&
                WebUtility.HtmlDecode(await wrongLeaveStatus.Content.ReadAsStringAsync()).Contains("Học sinh có đơn nghỉ đã duyệt"),
                "Save reports the mandatory approved-leave rule on the form");
            var missingReason = await client.PostAsync("/Attendance/Save", new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token),
                ["TimetableEntryId"] = "400", ["Date"] = "2026-10-28",
                ["Rows[0].CustomerId"] = "10", ["Rows[0].Status"] = "EXCUSED",
                ["Rows[0].LeaveRequestId"] = approvedLeave.Id.ToString(), ["ChangeReason"] = " "
            }));
            assert(missingReason.StatusCode == HttpStatusCode.BadRequest &&
                WebUtility.HtmlDecode(await missingReason.Content.ReadAsStringAsync()).Contains("Chỉnh sửa điểm danh phải có lý do"),
                "Changing an existing result to the suggested leave still requires a reason");
            assert(fixture.Repo.Records.Single().Status == "LATE", "A failed leave correction preserves the saved result");
            fixture.Repo.Leaves.Clear();

            var originalTime = fixture.Clock.Now;
            var commitsBefore = fixture.Repo.Commits;
            foreach (var daysFromLesson in new[] { -1, 1 })
            {
                fixture.Clock.Now = originalTime.AddDays(daysFromLesson);
                var otherDaySheet = await client.GetAsync("/Attendance/Mark?timetableEntryId=400&date=2026-10-28");
                var otherDayHtml = await otherDaySheet.Content.ReadAsStringAsync();
                assert(otherDaySheet.IsSuccessStatusCode && !otherDayHtml.Contains("id=\"attendance-save\""),
                    "Other dates open without a URL guard; editing follows lesson start and deadline rules");
                var rejectedSave = await client.PostAsync("/Attendance/Save", new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["__RequestVerificationToken"] = WebUtility.HtmlDecode(token),
                    ["TimetableEntryId"] = "400", ["Date"] = "2026-10-28",
                    ["Rows[0].CustomerId"] = "10", ["Rows[0].Status"] = "ABSENT", ["ChangeReason"] = "Kiểm tra ngày học"
                }));
                assert(rejectedSave.StatusCode == HttpStatusCode.Forbidden && fixture.Repo.Commits == commitsBefore,
                    "Existing lesson-start and midnight rules still apply when saving");
            }
            fixture.Clock.Now = originalTime;

            fixture.Repo.User!.Roles = ["STAFF"];
            var staffSchedule = await client.GetStringAsync("/Attendance?Date=2026-10-21&ClassId=100");
            assert(staffSchedule.Contains("name=\"Date\"") && staffSchedule.Contains("name=\"ClassId\"") && staffSchedule.Contains("value=\"2026-10-21\""),
                "Staff keeps date/class filters");

            var history = await client.GetAsync("/Attendance/History?from=2026-10-28&to=2026-10-28");
            var historyHtml = WebUtility.HtmlDecode(await history.Content.ReadAsStringAsync());
            assert(history.IsSuccessStatusCode && historyHtml.Contains("Đi muộn"), "History Razor output");
            var audit = await client.GetAsync("/Attendance/Audit?id=" + fixture.Repo.Records.Single().Id);
            assert(audit.IsSuccessStatusCode && WebUtility.HtmlDecode(await audit.Content.ReadAsStringAsync()).Contains("Nhật ký điểm danh"), "Audit Razor output");

            fixture.Repo.User.Roles = ["PRINCIPAL"];
            var principalSchedule = await client.GetStringAsync("/Attendance?Date=2026-10-21&ClassId=100");
            assert(principalSchedule.Contains("name=\"Date\"") && principalSchedule.Contains("name=\"ClassId\"") && principalSchedule.Contains("value=\"2026-10-21\""),
                "Principal keeps date/class filters");
            assert((await client.GetAsync("/Attendance/History")).IsSuccessStatusCode, "Principal can view history");
            assert((await client.GetAsync("/Attendance/Audit?id=" + fixture.Repo.Records.Single().Id)).IsSuccessStatusCode,
                "Principal can view audits");
            var principalSheet = await client.GetStringAsync("/Attendance/Mark?timetableEntryId=400&date=2026-10-28");
            assert(!principalSheet.Contains("id=\"attendance-save\"") && !principalSheet.Contains("name=\"Rows[0].Status\""),
                "Principal sheet has no save button or editable attendance status");

            fixture.Repo.User!.Roles = ["PARENT"];
            fixture.Repo.User.CustomerId = 20;
            assert((await client.GetAsync("/Attendance/Audit?id=" + fixture.Repo.Records.Single().Id)).StatusCode == HttpStatusCode.Forbidden,
                "Controller denies parent access to audits");
            assert((await client.GetAsync("/Attendance")).StatusCode == HttpStatusCode.Redirect, "Parent starts at history");
            assert((await client.GetAsync("/Attendance/Mark?timetableEntryId=400&date=2026-10-28")).StatusCode == HttpStatusCode.Forbidden,
                "Parent cannot open a management sheet");
            var parentHistory = await client.GetAsync("/Attendance/History");
            var parentHtml = await parentHistory.Content.ReadAsStringAsync();
            assert(parentHistory.IsSuccessStatusCode && !parentHtml.Contains("/Attendance/Audit"),
                "Parent history hides management actions");
            assert(!parentHtml.Contains("name=\"ClassId\"") && !parentHtml.Contains("name=\"From\"") && !parentHtml.Contains("name=\"To\""), "Parent filters have no class or date range");
            assert(parentHtml.Contains("name=\"StudentId\"") && parentHtml.Contains("name=\"SemesterId\"") && parentHtml.Contains("name=\"SubjectId\""), "Parent filters include child, semester and subject");

            fixture.Repo.User.Roles = ["STUDENT"];
            fixture.Repo.User.CustomerId = 10;
            assert((await client.GetAsync("/Attendance/Audit?id=" + fixture.Repo.Records.Single().Id)).StatusCode == HttpStatusCode.Forbidden,
                "Controller denies student access to audits");
            var studentHtml = await client.GetStringAsync("/Attendance/History");
            foreach (var name in new[] { "SemesterId", "SubjectId", "Date", "Status" })
                assert(studentHtml.Contains($"name=\"{name}\""), "Missing student filter: " + name);
            foreach (var name in new[] { "StudentId", "ClassId", "From", "To" })
                assert(!studentHtml.Contains($"name=\"{name}\""), "Unexpected student filter: " + name);

            var pagedHtml = WebUtility.HtmlDecode(await client.GetStringAsync("/Attendance/History?SemesterId=300&SubjectId=200&Date=2026-10-28&Status=LATE&Page=2"));
            assert(pagedHtml.Contains("SemesterId=300") && pagedHtml.Contains("SubjectId=200") && pagedHtml.Contains("Date=2026-10-28") && pagedHtml.Contains("Status=LATE"), "Pagination preserves student filters");

            var anonymous = new HttpRequestMessage(HttpMethod.Get, "/Attendance/History");
            anonymous.Headers.Add("X-Test-Anonymous", "true");
            assert((await client.SendAsync(anonymous)).StatusCode == HttpStatusCode.Unauthorized, "Anonymous access denied");
            Console.WriteLine("PASS HTTP/Razor: four views, antiforgery, authenticated identity, parent navigation and access control");
        }
        finally { await app.StopAsync(); }
    }

    private sealed class TestAuthentication(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, Fixture fixture) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (Request.Headers.ContainsKey("X-Test-Anonymous")) return Task.FromResult(AuthenticateResult.NoResult());
            var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, "1"), new(ClaimTypes.Name, "Test user") };
            claims.AddRange(fixture.Repo.User!.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
        }
    }

    private sealed class SilentNotifications : INotificationService
    {
        public Task<ServiceResult<NotificationFeed>> FeedAsync(CancellationToken ct = default) => Task.FromResult(ServiceResult<NotificationFeed>.Success(new(0, [])));
        public Task<int> UnreadCountAsync(CancellationToken ct = default) => Task.FromResult(0);
        public Task<ServiceResult> ReadAsync(long id, CancellationToken ct = default) => Task.FromResult(ServiceResult.Success());
        public Task NotifyScoreChangedAsync(StudentScore score, decimal? oldScore, long senderUserId) => Task.CompletedTask;
    }
}
