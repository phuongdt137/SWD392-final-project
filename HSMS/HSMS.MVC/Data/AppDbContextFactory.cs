using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace HSMS.MVC.Data;

// EF CLI tạo DbContext trực tiếp, không chạy seed/khởi động web khi tạo migration.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var directory = Directory.Exists("HSMS.MVC") ? Path.Combine(Directory.GetCurrentDirectory(), "HSMS.MVC") : Directory.GetCurrentDirectory();
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder().SetBasePath(directory)
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables().AddCommandLine(args).Build();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(configuration.GetConnectionString("DefaultConnection")).Options;
        return new AppDbContext(options);
    }
}
