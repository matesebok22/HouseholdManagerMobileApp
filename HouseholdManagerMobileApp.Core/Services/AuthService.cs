using Supabase;

using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public class AuthService : IAuthService
{
    private readonly Client _supabaseClient;
    public string CurrentUsername { get; private set; } = string.Empty;
    public string Nickname { get; private set; } = string.Empty;

    public AuthService(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<bool> RegisterAsync(string username, string nickname, string password)
    {
        try
        {
            Dictionary<string, object> parameters = new()
            {
                { "p_username", username },
                { "p_nickname", nickname },
                { "p_raw_password", password }
            };

            var response = await _supabaseClient.Rpc("register_user", parameters);

            if (response.Content != null && response.Content.Trim() == "true")
                return true;
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            Dictionary<string, object> parameters = new()
            {
                { "p_username", username },
                { "p_raw_password", password }
            };

            var response = await _supabaseClient.Rpc("login_user", parameters);

            if (response.Content != null && response.Content.Trim() == "true")
            {
                CurrentUsername = username;
                var result = await _supabaseClient.From<User>().Where(u => u.Username == username).Select("u_nick").Get();
                Nickname = result.Models.FirstOrDefault()?.Nickname ?? string.Empty;
                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
    {
        try
        {
            Dictionary<string, object> parameters = new()
            {
                { "p_username", CurrentUsername },
                { "p_old_password", oldPassword },
                { "p_new_password", newPassword }
            };

            var response = await _supabaseClient.Rpc("change_user_password", parameters);

            if (response.Content != null && response.Content.Trim() == "true")
                return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        CurrentUsername = Nickname = string.Empty;
        await Shell.Current.GoToAsync("//LoginPage");
    }
}