using Microsoft.EntityFrameworkCore;
using HSMS.MVC.Data;
using HSMS.MVC.Repositories.Interfaces;
using HSMS.MVC.Repositories.Implementations;
using HSMS.MVC.Service;
using HSMS.MVC.Common;
using HSMS.MVC.Service.Notifications;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Thông báo qua Redirect (TempData) cũng dùng Session thay vì cookie TempData riêng.
builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
// Xác thực duy nhất từ Session, không tạo cookie đăng nhập riêng.
builder.Services.AddAuthentication(UserPrincipal.SessionScheme)
    .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(UserPrincipal.SessionScheme, _ => { });
builder.Services.AddAuthorization();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<SchoolTime>();
// Đăng ký theo request: Controller nhận interface, DI cấp đúng Service/Repository Attendance.
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
// Tác vụ nền gửi email từ thông báo/MessageLog đã commit, không đẩy dữ liệu real-time.
builder.Services.AddSingleton<NotificationDispatchSignal>();
builder.Services.AddHostedService<NotificationDispatcher>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session có sẵn lưu dữ liệu ở server; cookie dưới đây chỉ chứa mã phiên, không phải vé đăng nhập.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Dependency Injection (Repositories & Services)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAssessmentTypeRepository, AssessmentTypeRepository>();
builder.Services.AddScoped<ITeachingAssignmentRepository, TeachingAssignmentRepository>();
builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IStudentScoreRepository, StudentScoreRepository>();
builder.Services.AddScoped<IScoreService, ScoreService>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISubjectService, SubjectService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
// Session phải sẵn sàng trước khi SessionAuthenticationHandler dựng User cho request.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Reuse the existing account seed; academic demo data is limited to Development.
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAdminAccountsAsync(context);
    await ScoreSeeder.SeedAsync(context);
    if (app.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("DemoData:AttendanceEnabled"))
        await DbSeeder.SeedAttendanceDemoAsync(context, scope.ServiceProvider.GetRequiredService<SchoolTime>());
}

app.Run();
