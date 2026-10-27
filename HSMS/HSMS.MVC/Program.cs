using Microsoft.EntityFrameworkCore;
using HSMS.MVC.Data;
using HSMS.MVC.Repositories.Interfaces;
using HSMS.MVC.Repositories.Implementations;
using HSMS.MVC.Service;
using HSMS.MVC.Common;
using HSMS.MVC.Hubs;
using HSMS.MVC.Service.Notifications;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.EventsType = typeof(SchoolCookieEvents);
});
builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, AccountUserIdProvider>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<SchoolTime>();
builder.Services.AddScoped<SchoolCookieEvents>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddSingleton<NotificationDispatchSignal>();
builder.Services.AddHostedService<NotificationDispatcher>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Dependency Injection (Repositories & Services)
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAssessmentTypeRepository, AssessmentTypeRepository>();
builder.Services.AddScoped<ITeachingAssignmentRepository, TeachingAssignmentRepository>();
builder.Services.AddScoped<IAssessmentRepository, AssessmentRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IStudentScoreRepository, StudentScoreRepository>();
builder.Services.AddScoped<IScoreService, ScoreService>();

builder.Services.AddScoped<ITimetableRepository, TimetableRepository>();
builder.Services.AddScoped<ITimetableService, TimetableService>();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<NotificationHub>("/hubs/notifications", options => options.CloseOnAuthenticationExpiration = true);

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


