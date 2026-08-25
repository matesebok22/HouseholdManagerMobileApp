namespace HouseholdManagerMobileApp.Core.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(string username, string nickname, string password);
    Task<bool> LoginAsync(string username, string password);
    Task<bool> ChangePasswordAsync(string oldPassword, string newPassword);
    Task LogoutAsync();
}