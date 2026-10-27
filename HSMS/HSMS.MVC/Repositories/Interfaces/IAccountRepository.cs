using HSMS.MVC.Models;
namespace HSMS.MVC.Repositories.Interfaces;
public interface IAccountRepository
{
    Task<IAccount?> FindByLoginAsync(string identifier);
    Task<IAccount?> FindByEmailAsync(string email);
    Task<IAccount?> GetAsync(long id, string accountType);
    Task SaveChangesAsync();
}
