namespace HSMS.MVC.Service;

public sealed record CurrentUser(long UserId, string Name, long? EmployeeId, long? CustomerId, IReadOnlyList<string> Roles)
{
    public bool Has(string role) => Roles.Contains(role);
    public bool SchoolReader => Has("ADMIN") || Has("STAFF") || Has("PRINCIPAL");
}
public interface ICurrentUserService
{
    Task<CurrentUser?> GetAsync(CancellationToken cancellationToken = default);
}
