namespace HouseholdManagerMobileApp.Core.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(string username, string nickname, string password); // Felhasználó regisztráció.
    Task<bool> LoginAsync(string username, string password); // Felhasználó online és offline bejelentkezés.
    Task<bool> OfflineLoginAsync(string username); // Offline bejelentkezést kezelő metódus.
    Task<bool> ChangePasswordAsync(string oldPassword, string newPassword); // Felhasználó jelszavának változtatása.
    Task LogoutAsync(); // Felhasználó kijelentkezése.
}