using Supabase;
using Newtonsoft.Json;

using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public class AuthService : IAuthService
{
    private readonly Client _supabaseClient;
    private readonly IUserSessionService _userSessionService;
    public AuthService(Client supabaseClient, IUserSessionService userSessionService)
    {
        _supabaseClient = supabaseClient;
        _userSessionService = userSessionService;
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

            var response = await _supabaseClient.Rpc("login_user_v2", parameters);
            string rawJson = response.Content ?? "";

            if (!string.IsNullOrWhiteSpace(rawJson))
            {
                List<User>? userList = JsonConvert.DeserializeObject<List<User>>(rawJson);
                if (userList != null && userList.Count > 0)
                {
                    User user = userList[0];
                    List<Budget> budgets = [];
                    List<Event> eventlog = [];
                    Household? currentHousehold = null;

                    if (user.CurrentHousehold != string.Empty)
                    {
                        currentHousehold = await _supabaseClient.From<Household>().Where(x => x.HHID == user.CurrentHousehold).Single();
                        if (currentHousehold != null)
                        {
                            var budgetquery = await _supabaseClient.From<Budget>().Where(x => x.BudgetHousehold == currentHousehold.HHID).Get();
                            budgets = budgetquery.Models.ToList();

                            var eventquery = await _supabaseClient.From<Event>().Where(x => x.EventHousehold == currentHousehold.HHID).Get();
                            eventlog = eventquery.Models.ToList();

                            // Felhasználói session beállítása:
                            await _userSessionService.SetUserSessionAsync(user, currentHousehold, [], budgets, eventlog);
                        }
                    }

                    return true;
                }
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
                { "p_username", _userSessionService.GetCurrentUser().Username }, // A ! jel a null-forgiving operator, amely "elnyomja" a null ellenőrzés hibát.
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
        await _userSessionService.ClearUserSessionAsync();
        if (Shell.Current != null)
            await Shell.Current.GoToAsync("//LoginPage");
    }
}