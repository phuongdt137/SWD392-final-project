using HSMS.Attendance.Checks;
using HSMS.MVC.Common;
using HSMS.MVC.Data;
using HSMS.MVC.DTOs.Attendance;
using HSMS.MVC.Models;
using HSMS.MVC.Repositories.Implementations;
using Microsoft.EntityFrameworkCore;

var passed = 0;
void Assert(bool value, string message = "Unexpected result")
{
    if (!value) throw new Exception(message);
}
async Task Run(string name, Func<Task> check)
{
    await check();
    passed++;
    Console.WriteLine("PASS " + name);
}

await Run("Teacher creates a result and audit", async () =>
{
    var f = new Fixture();
    var result = await f.Service.SaveAsync(f.Repo.User!, f.Input());
    Assert(result.IsSuccess && result.Data!.Changed == 1);
    Assert(f.Repo.Records.Single().RecordedByEmployeeId == 30 && f.Repo.Audits.Count == 1);
    Assert(f.Repo.Notifications.Count == 0);
});
await Run("Teacher can save at 23:59:59 Vietnam time", async () =>
{
    var f = new Fixture();
    f.Clock.Now = new(2026, 10, 28, 16, 59, 59, TimeSpan.Zero);
    Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input())).IsSuccess);
});
await Run("Teacher is locked at exactly midnight", async () =>
{
    var f = new Fixture();
    f.Clock.Now = new(2026, 10, 28, 17, 0, 0, TimeSpan.Zero);
    Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input())).StatusCode == 403);
    Assert(f.Repo.Commits == 0);
    Assert(!(await f.Service.SheetAsync(f.Repo.User!, 400, f.Day)).Data!.CanWrite);
});
await Run("Teacher cannot save before lesson starts", async () =>
{
    var f = new Fixture();
    f.Clock.Now = new(2026, 10, 27, 23, 59, 59, TimeSpan.Zero);
    Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input())).StatusCode == 403);
});
await Run("Staff can correct after the teacher deadline", async () =>
{
    var f = new Fixture("STAFF", recorded: true);
    f.Clock.Now = new(2026, 10, 29, 8, 0, 0, TimeSpan.Zero);
    Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input("LATE"))).IsSuccess);
    Assert(f.Repo.Records.Single().UpdatedByEmployeeId == 60);
    Assert(f.Repo.Records.Single().RecordedByEmployeeId == 30);
});
await Run("Staff can correct until the last second of the semester", async () =>
{
    var f = new Fixture("STAFF", recorded: true);
    f.Clock.Now = new(2026, 10, 31, 16, 59, 59, TimeSpan.Zero);
    Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input("LATE"))).IsSuccess);
});
await Run("Staff is locked at the exact semester boundary", async () =>
{
    var f = new Fixture("STAFF", recorded: true);
    f.Clock.Now = new(2026, 10, 31, 17, 0, 0, TimeSpan.Zero);
    Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input("LATE"))).StatusCode == 403);
    Assert(!(await f.Service.SheetAsync(f.Repo.User!, 400, f.Day)).Data!.CanWrite);
});
await Run("Starting another semester does not reopen old records", async () =>
{
    var f = new Fixture("STAFF", recorded: true);
    f.Clock.Now = new(2027, 1, 15, 8, 0, 0, TimeSpan.Zero);
    Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input("LATE"))).StatusCode == 403);
});
foreach (var role in new[] { "TEACHER", "STAFF" })
{
    await Run(role + " save crossing its deadline rolls back", async () =>
    {
        var f = new Fixture(role, recorded: true);
        var lastDay = role == "STAFF" ? 31 : 28;
        f.Clock.Now = new(2026, 10, lastDay, 16, 59, 59, TimeSpan.Zero);
        f.Repo.OnSave = () => f.Clock.Now = f.Clock.Now.AddSeconds(1);
        Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input("LATE"))).StatusCode == 403);
        Assert(f.Repo.Commits == 0 && f.Repo.Notifications.Count == 0 && f.Repo.Audits.Count == 0);
        Assert(f.Repo.Records.Single().Status == "PRESENT");
    });
}
await Run("Staff cannot create first results", async () =>
{
    var f = new Fixture("STAFF");
    Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input("ABSENT"))).StatusCode == 403);
    Assert(!(await f.Service.SheetAsync(f.Repo.User!, 400, f.Day)).Data!.CanWrite);
});
await Run("Staff corrects an existing student's result without a record ID in the form", async () =>
{
    var f = new Fixture("STAFF", recorded: true);
    Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input("LATE"))).IsSuccess);
    Assert(f.Repo.Records.Count == 1 && f.Repo.Records.Single().Id == 900);
});
await Run("Teacher schedule loads assigned lessons", async () =>
{
    var f = new Fixture();
    Assert((await f.Service.IndexAsync(f.Repo.User!, new())).Data!.Lessons.Count == 1);
    f.Repo.User!.EmployeeId = 31;
    Assert((await f.Service.IndexAsync(f.Repo.User!, new())).Data!.Lessons.Count == 0);
});
foreach (var role in new[] { "PRINCIPAL", "STUDENT", "PARENT" })
{
    await Run(role + " cannot save attendance", async () =>
    {
        var f = new Fixture(role, recorded: true);
        Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input("ABSENT"))).StatusCode == 403);
    });
}
await Run("Service uses the authenticated session without querying the account again", async () =>
{
    var f = new Fixture();
    var sessionUser = f.Repo.User!;
    f.Repo.User = null;
    Assert((await f.Service.SaveAsync(sessionUser, f.Input())).IsSuccess);
    Assert(f.Repo.Records.Single().RecordedByEmployeeId == sessionUser.EmployeeId);
});
await Run("Missing employee profile is not created automatically", async () =>
{
    var f = new Fixture("STAFF", recorded: true);
    f.Repo.User!.EmployeeId = null;
    Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input("LATE"))).StatusCode == 403);
});
await Run("Student and parent history cannot reveal another student", async () =>
{
    foreach (var role in new[] { "STUDENT", "PARENT" })
    {
        var f = new Fixture(role, recorded: true);
        Assert((await f.Service.HistoryAsync(f.Repo.User!, new())).Data!.Records.Count == 1);
        Assert((await f.Service.HistoryAsync(f.Repo.User!, new() { StudentId = 999 })).Data!.Filter.StudentId == 10);
        f.Repo.User!.CustomerId = 999;
        Assert((await f.Service.HistoryAsync(f.Repo.User!, new())).Data!.Records.Count == 0);
    }
});
await Run("Principal can view but not edit", async () =>
{
    var f = new Fixture("PRINCIPAL", recorded: true);
    Assert((await f.Service.SheetAsync(f.Repo.User!, 400, f.Day)).Data!.CanWrite == false);
    Assert((await f.Service.HistoryAsync(f.Repo.User!, new())).Data!.Records.Count == 1);
});
await Run("Empty results and students without a status are reported before saving", async () =>
{
    var f = new Fixture();
    var input = f.Input();
    input.Rows.Clear();
    Assert((await f.Service.SaveAsync(f.Repo.User!, input)).StatusCode == 400);
    input.Rows.Add(new() { CustomerId = 10, Status = "" });
    Assert((await f.Service.SaveAsync(f.Repo.User!, input)).ErrorCode == "INCOMPLETE_ATTENDANCE");
    Assert(f.Repo.Commits == 0 && f.Repo.Records.Count == 0);
});
await Run("A missing student row is reported before any database changes", async () =>
{
    var f = new Fixture();
    var second = new Customer(4, "HS02");
    Fixture.Set(second, "Id", 11L);
    Fixture.Set(second, "User", new User("child2", "child2@example.test", "unused", "Học sinh thứ hai"));
    f.Repo.Students.Add(second);
    var result = await f.Service.SaveAsync(f.Repo.User!, f.Input());
    Assert(result.ErrorCode == "INCOMPLETE_ATTENDANCE" && f.Repo.Commits == 0);
    Assert(f.Repo.Records.Count == 0 && f.Repo.Audits.Count == 0 && f.Repo.Notifications.Count == 0);
});
await Run("Excused absence needs the correct approved leave", async () =>
{
    var f = new Fixture();
    Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input("EXCUSED"))).StatusCode == 400);
    var leave = f.ApprovedLeave();
    var input = f.Input("EXCUSED");
    input.Rows[0].LeaveRequestId = leave.Id;
    Assert((await f.Service.SaveAsync(f.Repo.User!, input)).IsSuccess);
    Assert(f.Repo.Records.Single().LeaveRequestId == leave.Id && f.Repo.Notifications.Count == 0);
});
foreach (var status in new[] { "LATE", "ABSENT" })
{
    await Run(status + " queues notification and email to the linked parent", async () =>
    {
        var f = new Fixture();
        Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input(status))).IsSuccess);
        var notice = f.Repo.Notifications.Single();
        Assert(notice.RecipientId == 2 && notice.AttendanceAuditLog != null);
        Assert(notice.MessageLogs.Single().TemplateCode == "ATTENDANCE" && notice.MessageLogs.Single().Channel == "EMAIL");
        Assert(notice.MessageLogs.Single().Status == "QUEUED" && f.Repo.Commits == 1);
    });
}
await Run("Missing parent preserves result and reports undelivered alert", async () =>
{
    var f = new Fixture();
    Fixture.Set(f.Student, "Parent", null);
    var result = await f.Service.SaveAsync(f.Repo.User!, f.Input("ABSENT"));
    Assert(result.IsSuccess && result.Data!.MissingParents == 1 && f.Repo.Records.Count == 1);
    Assert(f.Repo.Notifications.Count == 0);
});
await Run("Invalid email preserves the in-app notification", async () =>
{
    var f = new Fixture();
    Fixture.Set(f.ParentUser, "Email", "parent@example.invalid");
    var result = await f.Service.SaveAsync(f.Repo.User!, f.Input("LATE"));
    Assert(result.IsSuccess && result.Data!.MissingEmails == 1);
    Assert(f.Repo.Notifications.Count == 1 && f.Repo.Notifications[0].MessageLogs.Count == 0);
});
foreach (var status in new[] { "PRESENT", "EXCUSED" })
{
    await Run("Staff correction to " + status + " sends no notification or email", async () =>
    {
        var f = new Fixture("STAFF", recorded: true);
        f.Repo.Records[0].RecordResult("ABSENT", null, null, 30, f.Clock.Now.UtcDateTime);
        var input = f.Input(status);
        if (status == "EXCUSED") input.Rows[0].LeaveRequestId = f.ApprovedLeave().Id;
        Assert((await f.Service.SaveAsync(f.Repo.User!, input)).IsSuccess);
        Assert(f.Repo.Notifications.Count == 0 && f.Repo.Audits.Count == 1);
    });
}
await Run("Editing only a comment does not resend an alert", async () =>
{
    var f = new Fixture("STAFF", recorded: true);
    f.Repo.Records[0].RecordResult("LATE", null, null, 30, f.Clock.Now.UtcDateTime);
    var input = f.Input("LATE");
    input.Rows[0].Comment = "Bổ sung ghi chú";
    Assert((await f.Service.SaveAsync(f.Repo.User!, input)).IsSuccess);
    Assert(f.Repo.Notifications.Count == 0 && f.Repo.Audits.Count == 1);
});
await Run("An unchanged result creates no new audit", async () =>
{
    var f = new Fixture("STAFF", recorded: true);
    var result = await f.Service.SaveAsync(f.Repo.User!, f.Input());
    Assert(result.IsSuccess && result.Data!.Changed == 0 && f.Repo.Audits.Count == 0);
});
await Run("A changed existing result requires a reason", async () =>
{
    var f = new Fixture("STAFF", recorded: true);
    var input = f.Input("LATE");
    input.ChangeReason = " ";
    Assert((await f.Service.SaveAsync(f.Repo.User!, input)).ErrorCode == "CHANGE_REASON_REQUIRED");
    Assert(f.Repo.Commits == 0);
});
await Run("A failed save publishes neither results nor notifications", async () =>
{
    var f = new Fixture();
    f.Repo.FailSave = true;
    Assert(!(await f.Service.SaveAsync(f.Repo.User!, f.Input("ABSENT"))).IsSuccess);
    Assert(f.Repo.Records.Count == 0 && f.Repo.Audits.Count == 0 && f.Repo.Notifications.Count == 0);
});
await Run("Existing database constraints remain mapped; Version is not a concurrency token", () =>
{
    using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
        .UseSqlServer("Server=127.0.0.1,1;Database=Offline;User Id=unused;Password=unused;TrustServerCertificate=True").Options);
    var model = db.Model.FindEntityType(typeof(AttendanceRecord))!;
    Assert(model.GetIndexes().Any(i => i.IsUnique));
    Assert(model.FindProperty("Version") is { IsConcurrencyToken: false });
    // Kiểm tra các query chính có thể được EF tạo SQL, không mở kết nối.
    var repo = new AttendanceRepository(db);
    Assert(db.AttendanceRecords.Where(r => r.Customer.ParentId == 20).ToQueryString().Contains("SELECT"));
    return Task.CompletedTask;
});

await Run("Teacher can mark but the views hide history and audit actions", async () =>
{
    var f = new Fixture(recorded: true);
    Assert(!(await f.Service.IndexAsync(f.Repo.User!, new())).Data!.CanViewHistory);
    var sheet = (await f.Service.SheetAsync(f.Repo.User!, 400, f.Day)).Data!;
    Assert(sheet.CanWrite && !sheet.CanViewAudit);
});
await Run("Student sees the selected semester without a hidden 30-day restriction", async () =>
{
    var f = new Fixture("STUDENT", recorded: true);
    f.Clock.Now = new(2027, 1, 20, 8, 0, 0, TimeSpan.Zero);
    var result = await f.Service.HistoryAsync(f.Repo.User!, new() { ClassId = 999 });
    Assert(result.IsSuccess && result.Data!.Records.Count == 1);
    Assert(result.Data!.Filter.SemesterId == 300 && result.Data.Filter.ClassId == null);
    Assert(result.Data.Filter.From == null && result.Data.Filter.To == null);
});
await Run("Current enrolled semester is preferred over a future semester", async () =>
{
    var f = new Fixture("STUDENT", recorded: true);
    f.Repo.Semesters[10].Insert(0, new(301, "Học kỳ sau", new(2027, 1, 1), new(2027, 5, 31)));
    Assert((await f.Service.HistoryAsync(f.Repo.User!, new())).Data!.Filter.SemesterId == 300);
});
await Run("Student semester and subject options exist before any attendance result", async () =>
{
    var f = new Fixture("STUDENT");
    var result = (await f.Service.HistoryAsync(f.Repo.User!, new())).Data!;
    Assert(result.Records.Count == 0 && result.Semesters.Count == 1 && result.Subjects.Count == 1);
});
await Run("Semester, subject, exact date and status all filter student results", async () =>
{
    var f = new Fixture("STUDENT", recorded: true);
    var filter = new AttendanceHistoryFilter { SemesterId = 300, SubjectId = 200, Date = f.Day, Status = "PRESENT" };
    Assert((await f.Service.HistoryAsync(f.Repo.User!, filter)).Data!.Records.Count == 1);
    filter.Date = f.Day.AddDays(-1);
    Assert((await f.Service.HistoryAsync(f.Repo.User!, filter)).Data!.Records.Count == 0);
    filter.Date = f.Day;
    filter.Status = "LATE";
    Assert((await f.Service.HistoryAsync(f.Repo.User!, filter)).Data!.Records.Count == 0);
});
await Run("Study filters use available options without rejecting the request", async () =>
{
    var f = new Fixture("STUDENT", recorded: true);
    var semester = await f.Service.HistoryAsync(f.Repo.User!, new() { SemesterId = 999 });
    Assert(semester.IsSuccess && semester.Data!.Filter.SemesterId == 300);
    var subject = await f.Service.HistoryAsync(f.Repo.User!, new() { SemesterId = 300, SubjectId = 999 });
    Assert(subject.IsSuccess && subject.Data!.Filter.SubjectId == null);
    var date = await f.Service.HistoryAsync(f.Repo.User!, new() { SemesterId = 300, Date = new(2027, 1, 1) });
    Assert(date.IsSuccess && date.Data!.Records.Count == 0);
});
await Run("Old notification links still select the intended day", async () =>
{
    var f = new Fixture("PARENT", recorded: true);
    var result = await f.Service.HistoryAsync(f.Repo.User!, new() { StudentId = 10, From = f.Day, To = f.Day });
    Assert(result.IsSuccess && result.Data!.Filter.Date == f.Day && result.Data.Records.Count == 1);
});
await Run("Parent filters follow the selected child without leaking another child's options", async () =>
{
    var f = new Fixture("PARENT", recorded: true);
    var second = new Customer(4, "HS02", parentId: 20);
    Fixture.Set(second, "Id", 11L);
    Fixture.Set(second, "User", new User("child2", "child2@example.test", "unused", "Con thứ hai"));
    f.Repo.Students.Add(second);
    f.Repo.Semesters[11] = [new(301, "Học kỳ của con thứ hai", new(2026, 8, 1), new(2026, 12, 31))];
    f.Repo.Subjects[(11, 301)] = [new(201, "Vật lý")];
    var result = await f.Service.HistoryAsync(f.Repo.User!, new() { StudentId = 11 });
    Assert(result.IsSuccess && result.Data!.IsParent && result.Data.Students.Count == 2);
    Assert(result.Data!.Filter.SemesterId == 301 && result.Data.Subjects.Single().Id == 201 && result.Data.Records.Count == 0);
    Assert((await f.Service.HistoryAsync(f.Repo.User!, new() { StudentId = 11, SemesterId = 300 })).Data!.Filter.SemesterId == 301);
    Assert((await f.Service.HistoryAsync(f.Repo.User!, new() { StudentId = 999 })).Data!.Filter.StudentId == 10);
});
await Run("Parent with no linked children gets no attendance or study options", async () =>
{
    var f = new Fixture("PARENT", recorded: true);
    f.Repo.User!.CustomerId = 999;
    var result = (await f.Service.HistoryAsync(f.Repo.User!, new())).Data!;
    Assert(result.IsParent && result.Students.Count == 0 && result.Semesters.Count == 0 && result.Records.Count == 0);
});

await Run("A mixed class save counts changes and parent warnings across students", async () =>
{
    var f = new Fixture(recorded: true);
    var second = new Customer(4, "HS02", parentId: 20);
    Fixture.Set(second, "Id", 11L);
    Fixture.Set(second, "User", new User("child2", "child2@example.test", "unused", "Học sinh thứ hai"));
    Fixture.Set(second, "Parent", f.Student.Parent);
    var third = new Customer(5, "HS03");
    Fixture.Set(third, "Id", 12L);
    Fixture.Set(third, "User", new User("child3", "child3@example.test", "unused", "Học sinh thứ ba"));
    f.Repo.Students.AddRange([second, third]);
    var input = f.Input(); // Học sinh đầu tiên giữ nguyên kết quả.
    input.Rows.Add(new() { CustomerId = 11, Status = "LATE" });
    input.Rows.Add(new() { CustomerId = 12, Status = "ABSENT" });

    var result = await f.Service.SaveAsync(f.Repo.User!, input);
    Assert(result.IsSuccess && result.Data!.Changed == 2 && result.Data.MissingParents == 1 && result.Data.MissingEmails == 0);
    Assert(f.Repo.Records.Count == 3 && f.Repo.Audits.Count == 2 && f.Repo.Commits == 1);
    Assert(f.Repo.Notifications.Single().MessageLogs.Count == 1);
});
await Run("An invalid later student rolls back earlier results and alerts in the same class", async () =>
{
    var f = new Fixture();
    var second = new Customer(4, "HS02");
    Fixture.Set(second, "Id", 11L);
    Fixture.Set(second, "User", new User("child2", "child2@example.test", "unused", "Học sinh thứ hai"));
    f.Repo.Students.Add(second);
    var input = f.Input("LATE");
    input.Rows.Add(new() { CustomerId = 11, Status = "EXCUSED" }); // Thiếu đơn nghỉ đã duyệt.

    var result = await f.Service.SaveAsync(f.Repo.User!, input);
    Assert(result.StatusCode == 400 && result.ErrorCode == "LEAVE_REQUIRED");
    Assert(f.Repo.Records.Count == 0 && f.Repo.Audits.Count == 0 && f.Repo.Notifications.Count == 0 && f.Repo.Commits == 0);
});

await Run("Teacher schedule always uses today and ignores submitted date and class filters", async () =>
{
    var f = new Fixture();
    foreach (var requestedDate in new[] { f.Day.AddDays(-7), f.Day.AddDays(7) })
    {
        var result = await f.Service.IndexAsync(f.Repo.User!, new() { Date = requestedDate, ClassId = 999 });
        Assert(result.IsSuccess && result.Data!.Filter.Date == f.Day && result.Data.Filter.ClassId == null);
        Assert(!result.Data!.CanFilter && result.Data.Classes.Count == 0);
    }
});
await Run("Other lesson dates open normally and follow the existing editing deadlines", async () =>
{
    foreach (var daysFromLesson in new[] { -1, 1 })
    {
        var f = new Fixture(recorded: true);
        f.Clock.Now = f.Clock.Now.AddDays(daysFromLesson);
        var sheet = await f.Service.SheetAsync(f.Repo.User!, 400, f.Day);
        Assert(sheet.IsSuccess && !sheet.Data!.CanWrite);
        Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input("ABSENT"))).StatusCode == 403);
        Assert(f.Repo.Commits == 0 && f.Repo.Records.Single().Status == "PRESENT" && f.Repo.Notifications.Count == 0);
    }
});
await Run("Teacher can still view and correct today's existing result", async () =>
{
    var f = new Fixture(recorded: true);
    Assert((await f.Service.SheetAsync(f.Repo.User!, 400, f.Day)).Data!.CanWrite);
    Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input("LATE"))).IsSuccess);
    Assert(f.Repo.Records.Single().Status == "LATE" && f.Repo.Records.Single().UpdatedByEmployeeId == 30);
});
await Run("Teacher current day follows Vietnam time when the UTC date is still yesterday", async () =>
{
    var f = new Fixture();
    f.Clock.Now = new(2026, 10, 27, 18, 0, 0, TimeSpan.Zero);
    Assert((await f.Service.IndexAsync(f.Repo.User!, new())).Data!.Filter.Date == f.Day);
    var sheet = await f.Service.SheetAsync(f.Repo.User!, 400, f.Day);
    Assert(sheet.IsSuccess && !sheet.Data!.CanWrite); // Hôm nay nhưng chưa đến giờ bắt đầu tiết.
});
await Run("Staff and Principal keep their date/class filters and can view other days", async () =>
{
    foreach (var role in new[] { "STAFF", "PRINCIPAL" })
    {
        var f = new Fixture(role, recorded: true);
        var result = await f.Service.IndexAsync(f.Repo.User!, new() { Date = f.Day.AddDays(-7), ClassId = 100 });
        Assert(result.IsSuccess && result.Data!.CanFilter && result.Data.Filter.Date == f.Day.AddDays(-7));
        Assert(result.Data!.Filter.ClassId == 100 && result.Data.Classes.Count > 0);
        foreach (var daysFromLesson in new[] { -1, 1 })
        {
            f.Clock.Now = new DateTimeOffset(f.Day.AddDays(daysFromLesson).AddHours(8), TimeSpan.Zero);
            Assert((await f.Service.SheetAsync(f.Repo.User!, 400, f.Day)).IsSuccess);
        }
    }
});

await Run("A 09:00-09:45 lesson is writable from 09:00 through 23:59:59 Vietnam time", async () =>
{
    foreach (var (utc, allowed) in new[]
    {
        (new DateTimeOffset(2026, 10, 28, 1, 59, 59, TimeSpan.Zero), false),
        (new DateTimeOffset(2026, 10, 28, 2, 0, 0, TimeSpan.Zero), true),
        (new DateTimeOffset(2026, 10, 28, 2, 45, 0, TimeSpan.Zero), true),
        (new DateTimeOffset(2026, 10, 28, 3, 30, 0, TimeSpan.Zero), true),
        (new DateTimeOffset(2026, 10, 28, 16, 59, 59, TimeSpan.Zero), true),
        (new DateTimeOffset(2026, 10, 28, 17, 0, 0, TimeSpan.Zero), false)
    })
    {
        var f = new Fixture();
        Fixture.Set(f.Entry, "TimeSlot", new TimeSlot("T2", 2, TimeSpan.FromHours(9), TimeSpan.FromHours(9.75)));
        f.Clock.Now = utc;
        var sheet = await f.Service.SheetAsync(f.Repo.User!, 400, f.Day);
        Assert(sheet.IsSuccess && sheet.Data!.CanWrite == allowed);
        var result = await f.Service.SaveAsync(f.Repo.User!, f.Input());
        Assert(result.IsSuccess == allowed && f.Repo.Commits == (allowed ? 1 : 0));
    }
});
await Run("An approved leave requires EXCUSED and a specified valid request for both Teacher and Staff", async () =>
{
    foreach (var role in new[] { "TEACHER", "STAFF" })
    {
        foreach (var status in new[] { "PRESENT", "LATE", "ABSENT", "EXCUSED" })
        {
            var f = new Fixture(role, recorded: true);
            f.ApprovedLeave();
            var input = f.Input(status); // Chưa chỉ định đơn, dù đã có đơn được duyệt.
            var rejected = await f.Service.SaveAsync(f.Repo.User!, input);
            Assert(rejected.ErrorCode == "APPROVED_LEAVE_REQUIRED" && f.Repo.Commits == 0);
            Assert(f.Repo.Records.Single().Status == "PRESENT" && f.Repo.Notifications.Count == 0);
            if (status == "EXCUSED")
            {
                input.Rows[0].LeaveRequestId = 999;
                Assert((await f.Service.SaveAsync(f.Repo.User!, input)).ErrorCode == "APPROVED_LEAVE_REQUIRED");
            }
        }
        var valid = new Fixture(role, recorded: true);
        var leave = valid.ApprovedLeave();
        var approved = valid.Input("EXCUSED");
        approved.Rows[0].LeaveRequestId = leave.Id;
        Assert((await valid.Service.SaveAsync(valid.Repo.User!, approved)).IsSuccess);
        Assert(valid.Repo.Records.Single().Status == "EXCUSED" && valid.Repo.Records.Single().LeaveRequestId == leave.Id);
    }
});
await Run("Pending, rejected, other-student and out-of-date leaves do not force excused absence", async () =>
{
    foreach (var kind in new[] { "PENDING", "REJECTED", "OTHER_STUDENT", "OTHER_DATE" })
    {
        var f = new Fixture();
        var leave = new LeaveRequest(kind == "OTHER_STUDENT" ? 11 : 10, 20, "Nghỉ ốm",
            kind == "OTHER_DATE" ? f.Day.AddDays(1) : f.Day, kind == "OTHER_DATE" ? f.Day.AddDays(1) : f.Day);
        Fixture.Set(leave, "Id", 50L);
        if (kind == "REJECTED") leave.Reject(new Employee(1, "GV01"), "Chưa đủ thông tin");
        else if (kind != "PENDING") leave.Approve(new Employee(1, "GV01"));
        f.Repo.Leaves.Add(leave);
        Assert((await f.Service.SheetAsync(f.Repo.User!, 400, f.Day)).Data!.Students.Single().Leaves.Count == 0);
        Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input())).IsSuccess);
    }
});
await Run("Approved leave date ranges include both the start date and end date", async () =>
{
    foreach (var boundary in new[] { "START", "END" })
    {
        var f = new Fixture();
        var leave = new LeaveRequest(10, 20, "Nghỉ ốm", boundary == "START" ? f.Day : f.Day.AddDays(-2),
            boundary == "END" ? f.Day : f.Day.AddDays(2));
        Fixture.Set(leave, "Id", 50L);
        leave.Approve(new Employee(1, "GV01"));
        f.Repo.Leaves.Add(leave);
        Assert((await f.Service.SaveAsync(f.Repo.User!, f.Input())).ErrorCode == "APPROVED_LEAVE_REQUIRED");
        var input = f.Input("EXCUSED");
        input.Rows[0].LeaveRequestId = 50;
        input.ChangeReason = null; // Lần đầu ghi không cần lý do sửa.
        Assert((await f.Service.SaveAsync(f.Repo.User!, input)).IsSuccess);
    }
});
await Run("Editing status, comment or leave request requires a nonblank reason", async () =>
{
    foreach (var role in new[] { "TEACHER", "STAFF" })
    {
        foreach (var change in new[] { "STATUS", "COMMENT", "LEAVE" })
        {
            var f = new Fixture(role, recorded: true);
            var input = f.Input(change == "STATUS" ? "LATE" : "PRESENT");
            if (change == "COMMENT") input.Rows[0].Comment = "Bổ sung ghi chú";
            if (change == "LEAVE")
            {
                var first = f.ApprovedLeave();
                var second = new LeaveRequest(10, 20, "Đơn bổ sung", f.Day, f.Day);
                Fixture.Set(second, "Id", 51L);
                second.Approve(new Employee(1, "GV01"));
                f.Repo.Leaves.Add(second);
                f.Repo.Records.Single().RecordResult("EXCUSED", null, first, 30, f.Clock.Now.UtcDateTime);
                input.Rows[0].Status = "EXCUSED";
                input.Rows[0].LeaveRequestId = second.Id;
            }
            input.ChangeReason = "  ";
            Assert((await f.Service.SaveAsync(f.Repo.User!, input)).ErrorCode == "CHANGE_REASON_REQUIRED");
            Assert(f.Repo.Commits == 0 && f.Repo.Audits.Count == 0 && f.Repo.Notifications.Count == 0);
            input.ChangeReason = "Xác nhận thông tin mới";
            Assert((await f.Service.SaveAsync(f.Repo.User!, input)).IsSuccess);
        }
    }
});

await OfflineSqlChecks.RunAsync(Assert);
await UiChecks.RunAsync(Assert);
Console.WriteLine($"Completed {passed} business checks plus HTTP/Razor checks. No real database, SMTP or production startup was used.");
