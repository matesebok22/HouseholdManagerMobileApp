namespace HouseholdManagerMobileApp.Core.Services;

public interface IAuthService
{
    string CurrentUsername { get; }
    Task<bool> RegisterAsync(string username, string password);
    Task<bool> LoginAsync(string username, string password);
}
