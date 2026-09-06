using Newtonsoft.Json;
using SQLite;
using Supabase;

using HouseholdManagerMobileApp.Core.Helpers;
using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public class AuthService : IAuthService
{
    private readonly Client _supabaseClient;
    private readonly SQLiteAsyncConnection _sqliteDb;
    private readonly IDatabaseService _databaseService;
    private readonly IUserSessionService _userSessionService;
    public AuthService(Client supabaseClient, SQLiteAsyncConnection sqliteDb, IDatabaseService databaseService, IUserSessionService userSessionService)
    {
        _supabaseClient = supabaseClient;
        _sqliteDb = sqliteDb;
        _databaseService = databaseService;
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
        // Offline bejelentkezés, ha nincs internetkapcsolat:
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
            return await OfflineLoginAsync(username);

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

                    // Minden online bejelentkezésnél a korábbi offline token törlése:
                    string? lastLoggedUID = Preferences.Get("last_logged_uid", null);
                    if (!string.IsNullOrEmpty(lastLoggedUID))
                        SecureStorage.Remove($"login-{lastLoggedUID}");

                    if (user.CurrentHousehold != string.Empty)
                    {
                        currentHousehold = await _supabaseClient.From<Household>().Where(x => x.HHID == user.CurrentHousehold).Single();
                        if (currentHousehold != null)
                        {
                            var budgetquery = await _supabaseClient.From<Budget>().Where(x => x.BudgetHousehold == currentHousehold.HHID).Get();
                            budgets = budgetquery.Models.ToList();

                            var eventquery = await _supabaseClient.From<Event>().Where(x => x.EventHousehold == currentHousehold.HHID).Get();
                            eventlog = eventquery.Models.ToList();
                        }
                    }

                    // Felhasználói session beállítása:
                    await _userSessionService.SetUserSessionAsync(user, currentHousehold, [], budgets, eventlog);

                    // Offline adatbázis frissítése:
                    await _databaseService.CopyDataToOfflineDB();

                    // Offline bejelentkezéshez szükséges token beállítása:
                    await SecureStorage.SetAsync($"login-{user.UID}", DateTime.Now.Ticks.ToString());
                    Preferences.Set("last_logged_uid", user.UID);

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

    public async Task<bool> OfflineLoginAsync(string username)
    {
        try
        {
            OfflineUser? userquery = await _sqliteDb.Table<OfflineUser>().Where(x => x.Username == username).FirstOrDefaultAsync();
            if (userquery != null)
            {
                User user = new()
                {
                    UID = userquery.UID,
                    Username = userquery.Username,
                    CurrentHousehold = userquery.CurrentHousehold,
                    HouseHold1 = userquery.HouseHold1,
                    HouseHold2 = userquery.HouseHold2,
                    HouseHold3 = userquery.HouseHold3,
                    Nickname = userquery.Nickname
                };

                long lastLoggedTicks = long.Parse(await SecureStorage.GetAsync($"login-{user.UID}") ?? "0");
                long currentTicks = DateTime.Now.Ticks;
                // Lejárt-e a bejelentkezési token:
                if (lastLoggedTicks == 0 || (lastLoggedTicks + Constants.OfflineLoginValidity) < currentTicks) // DEBUG-olni, valamiért nem mentődik el a kulcs-érték.
                    return false;

                List<Budget> budgets = [];
                List<Event> eventlog = [];
                Household? currentHousehold = null;

                if (user.CurrentHousehold != string.Empty)
                {
                    OfflineHousehold? householdquery = await _sqliteDb.Table<OfflineHousehold>().Where(x => x.HHID == user.CurrentHousehold).FirstOrDefaultAsync();
                    if (householdquery != null)
                    {
                        currentHousehold = new Household
                        {
                            HHID = householdquery.HHID,
                            HouseholdName = householdquery.HouseholdName,
                            HouseholdOwner = householdquery.HouseholdOwner,
                            HouseholdAccess = householdquery.HouseholdAccess
                        };

                        List<OfflineBudget> budgetquery = await _sqliteDb.Table<OfflineBudget>().Where(x => x.BudgetHousehold == currentHousehold.HHID).ToListAsync();
                        budgets = budgetquery.Select(x => new Budget
                        {
                            BID = x.BID,
                            BudgetHousehold = x.BudgetHousehold,
                            BudgetDate = x.BudgetDate,
                            BudgetAmount = x.BudgetAmount
                        }).ToList();

                        List<OfflineEvent> eventquery = await _sqliteDb.Table<OfflineEvent>().Where(x => x.EventHousehold == currentHousehold.HHID).ToListAsync();
                        eventlog = eventquery.Select(x => new Event
                        {
                            EID = x.EID,
                            EventUser = x.EventUser,
                            EventHousehold = x.EventHousehold,
                            EventDate = x.EventDate,
                            EventName = x.EventName,
                            EventUserDate = x.EventUserDate,
                            EventType = x.EventType,
                            EventCost = x.EventCost,
                            EventDescription = x.EventDescription,
                            LastModifiedBy = x.LastModifiedBy
                        }).ToList();
                    }
                }

                // Felhasználói session beállítása:
                await _userSessionService.SetUserSessionAsync(user, currentHousehold, [], budgets, eventlog);

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
                { "p_username", _userSessionService.GetCurrentUser()!.Username }, // A ! jel a null-forgiving operator, amely "elnyomja" a null ellenőrzés hibát.
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