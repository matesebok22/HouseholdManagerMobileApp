using Supabase;

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

            var response = await _supabaseClient.Rpc("login_user", parameters);

            if (response.Content != null && response.Content.Trim() == "true")
            {
                User? user = await _supabaseClient.From<User>().Where(x => x.Username == username).Single();
                if (user == null)
                    return false;

                List<Household> householdList = [];
                if (user.HouseHold1 != null)
                {
                    Household? hh1 = await _supabaseClient.From<Household>().Where(x => x.HHID == user.HouseHold1).Single();
                    if (hh1 != null)
                        householdList.Add(hh1);
                }
                if (user.HouseHold2 != null)
                {
                    Household? hh2 = await _supabaseClient.From<Household>().Where(x => x.HHID == user.HouseHold2).Single();
                    if (hh2 != null)
                        householdList.Add(hh2);
                }
                if (user.HouseHold3 != null)
                {
                    Household? hh3 = await _supabaseClient.From<Household>().Where(x => x.HHID == user.HouseHold3).Single();
                    if (hh3 != null)
                        householdList.Add(hh3);
                }

                List<Budget> budgets = [];
                List<Event> eventlog = [];
                Household? household = null;
                if (user.CurrentHousehold != string.Empty)
                {
                    household = await _supabaseClient.From<Household>().Where(x => x.HHID == user.CurrentHousehold).Single();
                    if (household != null)
                    {
                        var budgetquery = await _supabaseClient.From<Budget>().Where(x => x.BudgetHousehold == household.HHID).Get();
                        budgets = budgetquery.Models.ToList();

                        var eventquery = await _supabaseClient.From<Event>().Where(x => x.EventHousehold == household.HHID).Get();
                        eventlog = eventquery.Models.ToList();
                    }
                }

                // Felhasználói session beállítása:
                await _userSessionService.SetUserSessionAsync(user, household, householdList, budgets, eventlog);

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
                { "p_username", /*CurrentUsername*/ _userSessionService.GetCurrentUser().Username }, // A ! jel a null-forgiving operator, amely "elnyomja" a null ellenőrzés hibát.
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
        if (Shell.Current != null) // Tesztelés ne akadjon meg.
            await Shell.Current.GoToAsync("//LoginPage");
    }
}