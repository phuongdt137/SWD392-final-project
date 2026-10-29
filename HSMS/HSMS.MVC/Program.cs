using HSMS.MVC.Common;
using HSMS.MVC.Data;
using HSMS.MVC.Repositories.Implementations;
using HSMS.MVC.Repositories.Interfaces;
using HSMS.MVC.Service;
using HSMS.MVC.Service.Implementations;
using HSMS.MVC.Service.Interfaces;
using HSMS.MVC.Service.Notifications;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddSessionStateTempDataProvider();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();

// Cấu hình đường dẫn chuyển hướng mặc định cho [Authorize] (Login & AccessDenied)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });
builder.Services.AddAuthorization();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<SchoolTime>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<NotificationDispatchSignal>();
builder.Services.AddHostedService<NotificationDispatcher>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session lưu dữ liệu userJson ở server; trình duyệt chỉ giữ cookie chứa SessionId.
builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".HSMS.SessionId";
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
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<ISubjectRepository, SubjectRepository>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();

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
// Middleware truy vấn Session theo SessionId từ cookie và nạp vào UserPrincipal (HttpContext.User)
app.UseMiddleware<SessionAuthMiddleware>();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed the existing accounts and score demo data.
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAdminAccountsAsync(context);
    await ScoreSeeder.SeedAsync(context);
}

app.Run();

