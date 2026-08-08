namespace HouseholdManagerMobileApp.Core.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(string username, string password);
}
