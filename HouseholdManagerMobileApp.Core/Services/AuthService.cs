using Supabase;

namespace HouseholdManagerMobileApp.Core.Services;

public class AuthService : IAuthService
{
    private readonly Client _supabaseClient;
    public string CurrentUsername { get; private set; } = string.Empty;

    public AuthService(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        try
        {
            Dictionary<string, object> parameters = new()
            {
                { "p_username", username },
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
                return true;

            return false;
        }
        catch
        {
            return false;
        }
    }
}