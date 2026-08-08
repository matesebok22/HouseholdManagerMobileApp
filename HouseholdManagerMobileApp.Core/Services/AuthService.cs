using Supabase;

using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public class AuthService : IAuthService
{
    private readonly Client _supabaseClient;

    public AuthService(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task<bool> RegisterAsync(string username, string password)
    {
        try
        {
            User newUser = new() { Username = username, Password = password };
            var response = await _supabaseClient.From<User>().Insert(newUser);

            if (response.Models.Count != 0)
                return true;
            
            return false;
        }
        catch
        {
            return false;
        }
    }
}