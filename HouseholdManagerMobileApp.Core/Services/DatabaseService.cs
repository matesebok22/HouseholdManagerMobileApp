using System.Diagnostics;
using HouseholdManagerMobileApp.Core.Models;
using Microsoft.IdentityModel.Tokens;
using SQLite;
using Supabase;
using Supabase.Postgrest.Responses;

namespace HouseholdManagerMobileApp.Core.Services;

public class DatabaseService : IDatabaseService
{
    private readonly Client _supabaseClient;
    private readonly SQLiteAsyncConnection _sqliteDb;
    private readonly IUserSessionService _userSessionService;

    private bool loadOfflineDBOnce = false;

    public DatabaseService(Client supabaseClient, SQLiteAsyncConnection sqliteDb, IUserSessionService userSessionService)
    {
        _supabaseClient = supabaseClient;
        _sqliteDb = sqliteDb;
        _userSessionService = userSessionService;
    }

    public async Task InitializeAsync()
    {
        // Supabase adatbázis elindítása:
        try
        {
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet) // Ha van internetkapcsolat
                await _supabaseClient.InitializeAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Hiba történt a Supabase adatbázis inicializálása során.\n", ex);
        }

        // SQLite adatbázis elindítása:
        try
        {
            await _sqliteDb.CreateTableAsync<OfflineUser>();
            await _sqliteDb.CreateTableAsync<OfflineHousehold>();
            await _sqliteDb.CreateTableAsync<OfflineBudget>();
            await _sqliteDb.CreateTableAsync<OfflineEvent>();
            await _sqliteDb.CreateTableAsync<OfflineNotification>();
        }
        catch (Exception ex)
        {
            throw new Exception("Hiba történt az SQLite adatbázis inicializálása során.\n", ex);
        }
    }

    public async Task<bool> CreateHouseholdAsync(string householdName)
    {
        try
        {
            Dictionary<string, string> parameters = new()
            {
                { "p_userid", _userSessionService.GetCurrentUser()!.UID },
                { "p_householdname", householdName }
            };

            var response = await _supabaseClient.Rpc("create_household", parameters);

            if (response.Content != null && response.Content.Trim() == "true")
            {
                User? user = await _supabaseClient.From<User>().Where(x => x.UID == _userSessionService.GetCurrentUser()!.UID).Single();
                if (user == null)
                    return false;

                await _userSessionService.ReloadUser(user);

                loadOfflineDBOnce = false;

                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> JoinHouseholdAsync(string householdAccess)
    {
        Household? household = await _supabaseClient.From<Household>().Where(x => x.HouseholdAccess == householdAccess).Single();
        if (household == null)
            return false;

        //User? user = await _supabaseClient.From<User>().Where(x => x.Username == _userSessionService.GetCurrentUser()!.Username).Single();
        User? user = await _supabaseClient.From<User>().Where(x => x.UID == _userSessionService.GetCurrentUser()!.UID).Single();
        if (user == null)
            return false;

        // Már tagja-e a felhasználó a háztartásnak:
        if (user.HouseHold1 == household.HHID || user.HouseHold2 == household.HHID || user.HouseHold3 == household.HHID)
            return false;

        // Van-e szabad háztartás hely a felhasználónak:
        if (user.HouseHold1 == null)
            user.HouseHold1 = household.HHID;
        else if (user.HouseHold2 == null)
            user.HouseHold2 = household.HHID;
        else if (user.HouseHold3 == null)
            user.HouseHold3 = household.HHID;
        else
            return false;

        user.CurrentHousehold = household.HHID;
        await user.Update<User>();

        await _userSessionService.ReloadUser(user);

        loadOfflineDBOnce = false;

        return true;
    }

    public async Task<bool> EditHouseholdAsync(string newHouseholdName, DateTime householdBudgetMonth, int newHouseholdBudget)
    {
        string currentDate = $"{householdBudgetMonth.Year}. {householdBudgetMonth.Month}.";

        if (_userSessionService.GetCurrentHousehold()!.HouseholdName != newHouseholdName)
        {
            if (!_userSessionService.IsOfflineMode())
                await _supabaseClient.From<Household>().Where(x => x.HHID == _userSessionService.GetCurrentHousehold()!.HHID).Set(x => x.HouseholdName, newHouseholdName).Update();
            else
            {
                string sqliteQuery = @"UPDATE offline_households SET hh_name = ? WHERE hhid = ?";
                await _sqliteDb.ExecuteAsync(sqliteQuery, newHouseholdName, _userSessionService.GetCurrentHousehold()!.HHID);

                _userSessionService.GetCurrentHousehold()!.HouseholdName = newHouseholdName;
            }
        }
            
        // Háztartáshoz tartozó összes költségkeret lekérése:
        Budget? budget = _userSessionService.GetCurrentBudgets().Find(x => $"{x.BudgetDate.Year}. {x.BudgetDate.Month}." == currentDate);
        if (budget == null)
        {
            DateTime newDate = new(householdBudgetMonth.Year, householdBudgetMonth.Month, 1);

            if (!_userSessionService.IsOfflineMode())
            {
                await _supabaseClient.From<Budget>().Insert(new Budget() { BudgetHousehold = _userSessionService.GetCurrentHousehold()!.HHID, BudgetDate = newDate, BudgetAmount = newHouseholdBudget });

                loadOfflineDBOnce = false;
            }
            else
            {
                string sqliteQuery = @"INSERT INTO offline_budgets (bid, b_hh, b_userdate, b_amount, b_made_offline) VALUES (?, ?, ?, ?, 1)";
                await _sqliteDb.ExecuteAsync(sqliteQuery, Guid.NewGuid().ToString(), _userSessionService.GetCurrentHousehold()!.HHID, newDate, newHouseholdBudget);
            }
        }
        else
        {
            if (budget.BudgetAmount != newHouseholdBudget)
            {
                budget.BudgetAmount = newHouseholdBudget;
                if (!_userSessionService.IsOfflineMode())
                {
                    await budget.Update<Budget>();

                    loadOfflineDBOnce = false;
                }
                else
                {
                    string sqliteQuery = @"UPDATE offline_budgets SET b_amount = ?, b_made_offline = 1 WHERE bid = ?";
                    await _sqliteDb.ExecuteAsync(sqliteQuery, budget.BudgetAmount, budget.BID);
                }
            }
        }
        
        return true;
    }

    public async Task<bool> LeaveHouseholdAsync()
    {
        User? user = await _supabaseClient.From<User>().Where(x => x.UID == _userSessionService.GetCurrentUser()!.UID).Single();
        if (user == null)
            return false;

        Household? household = await _supabaseClient.From<Household>().Where(x => x.HHID == _userSessionService.GetCurrentHousehold()!.HHID).Single();
        if (household == null)
            return false;

        string[] households = [user.HouseHold1, user.HouseHold2, user.HouseHold3];
        int householdIndex = Array.FindIndex(households, x => x == household.HHID);

        if (householdIndex != -1)
        {
            switch (householdIndex)
            {
                case 0: user.HouseHold1 = null!; break;
                case 1: user.HouseHold2 = null!; break;
                case 2: user.HouseHold3 = null!; break;
            }

            user.CurrentHousehold = households.Where(x => x != null && x != household.HHID).FirstOrDefault() ?? null!;
        }

        await user.Update<User>();

        // Ha a jelenlegi háztartás tulajdonosa, akkor megszünteti a háztartást is.
        if (_userSessionService.IsCurrentHouseholdOwner())
        {
            household.HouseholdOwner = null!;
            await household.Update<Household>();
            await household.Delete<Household>();
        }

        await _userSessionService.ReloadUser(user);

        loadOfflineDBOnce = false;

        SecureStorage.Remove($"notifications-{user.UID}-{household.HHID}");

        return true;
    }

    public async Task<bool> CreateEventAsync(string eventName, DateTime eventUserDate, string eventType, int eventCost, string eventDesc)
    {
        Event newEvent = new()
        {
            EventUser = _userSessionService.GetCurrentUser()!.UID,
            EventHousehold = _userSessionService.GetCurrentHousehold()!.HHID,
            EventDate = DateTime.Now,
            EventName = eventName,
            EventUserDate = eventUserDate,
            EventType = eventType,
            EventCost = eventCost,
            EventDescription = eventDesc,
            LastModifiedBy = _userSessionService.GetCurrentUser()!.UID
        };

        if (!_userSessionService.IsOfflineMode())
        {
            var response = await _supabaseClient.From<Event>().Insert(newEvent);
            if (response.Models != null && response.Models.Count > 0)
            {
                loadOfflineDBOnce = false;

                return true;
            }
        }
        else
        {
            string sqliteQuery = @"INSERT INTO offline_events (eid, e_user, e_hh, e_date, e_name, e_userdate, e_type, e_cost, e_desc, last_modified_by, e_made_offline) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, 1)";
            await _sqliteDb.ExecuteAsync(sqliteQuery, Guid.NewGuid().ToString(), newEvent.EventUser, newEvent.EventHousehold, newEvent.EventDate, newEvent.EventName, newEvent.EventUserDate, newEvent.EventType, newEvent.EventCost, newEvent.EventDescription, newEvent.LastModifiedBy);
            return true;
        }

        return false;
    }

    public async Task<bool> EditEventAsync(string eventName, DateTime eventUserDate, string eventType, int eventCost, string eventDesc)
    {
        if (!_userSessionService.IsOfflineMode())
        {
            var response = await _supabaseClient.From<Event>()
                                    .Where(x => x.EID == _userSessionService.GetSelectedEvent()!.EID)
                                    .Set(x => x.EventName, eventName)
                                    .Set(x => x.EventUserDate, eventUserDate)
                                    .Set(x => x.EventType, eventType)
                                    .Set(x => x.EventCost, eventCost)
                                    .Set(x => x.EventDescription, eventDesc)
                                    .Set(x => x.LastModifiedBy, _userSessionService.GetCurrentUser()!.UID)
                                    .Update();
            if (response.Models != null && response.Models.Count > 0)
            {
                loadOfflineDBOnce = false;

                return true;
            }
        }
        else
        {
            string sqliteQuery = @"UPDATE offline_events SET e_name = ?, e_userdate = ?, e_type = ?, e_cost = ?, e_desc = ?, last_modified_by = ?, e_made_offline = 1 WHERE eid = ?";
            await _sqliteDb.ExecuteAsync(sqliteQuery, eventName, eventUserDate, eventType, eventCost, eventDesc, _userSessionService.GetCurrentUser()!.UID, _userSessionService.GetSelectedEvent()!.EID);
            return true;
        }

        return false;
    }

    public async Task DeleteEventAsync()
    {
        await _supabaseClient.From<Event>().Where(x => x.EID == _userSessionService.GetSelectedEvent()!.EID).Delete();

        loadOfflineDBOnce = false;
    }

    public async Task<bool> LoadUserPageAsync()
    {
        // Szükséges adatok: currenthousehold, eventlog, budgets
        Household? currenthousehold = await LoadCurrentHouseholdAsync(_userSessionService.GetCurrentUser()!);
        if (currenthousehold != null)
        {
            List<Event> eventlog = await LoadEventLogAsync(currenthousehold);
            List<Budget> budgets = await LoadBudgetsAsync(currenthousehold);

            await _userSessionService.ChangeHousehold(currenthousehold, [], [], budgets);
            await _userSessionService.ReloadEventLog(eventlog);
        }

        if (!_userSessionService.IsOfflineMode() && !loadOfflineDBOnce)
        {
            if (await CopyDataToOfflineDB() == false)
                return false;
            loadOfflineDBOnce = true;
        }

        return true;
    }

    public async Task<bool> LoadSettingsPageAsync(string selectedHouseholdAccess = "")
    {
        // Szükséges adatok: currenthousehold, householdlist, householdusers, budgets
        if (!selectedHouseholdAccess.IsNullOrEmpty())
        {
            Household? household = await _supabaseClient.From<Household>().Where(x => x.HouseholdAccess == selectedHouseholdAccess).Single();
            if (household != null)
            {
                await _supabaseClient.From<User>().Where(x => x.UID == _userSessionService.GetCurrentUser()!.UID).Set(x => x.CurrentHousehold, household.HHID).Update();
                User? user = await _supabaseClient.From<User>().Where(x => x.UID == _userSessionService.GetCurrentUser()!.UID).Single();
                if (user != null)
                    await _userSessionService.ReloadUser(user);

                loadOfflineDBOnce = false; // Új háztartás betöltésekor újra kell másolni az offline adatbázisba a szükséges adatokat.
            }
        }

        Household? currenthousehold = await LoadCurrentHouseholdAsync(_userSessionService.GetCurrentUser()!);
        if (currenthousehold != null)
        {
            List<Household> householdList = await LoadHouseholdsAsync(_userSessionService.GetCurrentUser()!);
            List<User> householdUsers = await LoadHouseholdUsersAsync(currenthousehold);
            List<Budget> budgets = await LoadBudgetsAsync(currenthousehold);

            await _userSessionService.ChangeHousehold(currenthousehold, householdUsers, householdList, budgets);
        }
        else
            await _userSessionService.ChangeHousehold(null, [], [], []);

        if (!_userSessionService.IsOfflineMode() && !loadOfflineDBOnce)
        {
            if (await CopyDataToOfflineDB() == false)
                return false;
            loadOfflineDBOnce = true;
        }

        return true;
    }

    public async Task<bool> LoadEventlogPageAsync()
    {
        // Szükséges adatok: eventlog
        List<Event> eventlog = await LoadEventLogAsync(_userSessionService.GetCurrentHousehold()!);

        await _userSessionService.ReloadEventLog(eventlog);

        if (!_userSessionService.IsOfflineMode() && !loadOfflineDBOnce)
        {
            if (await CopyDataToOfflineDB() == false)
                return false;
            loadOfflineDBOnce = true;
        }

        return true;
    }

    private async Task<List<Household>> LoadHouseholdsAsync(User user)
    {
        if (_userSessionService.IsOfflineMode())
            return [];

        // Felhasználó háztartásai:
        List<string> householdIds = new();
        if (!string.IsNullOrEmpty(user.HouseHold1)) householdIds.Add(user.HouseHold1);
        if (!string.IsNullOrEmpty(user.HouseHold2)) householdIds.Add(user.HouseHold2);
        if (!string.IsNullOrEmpty(user.HouseHold3)) householdIds.Add(user.HouseHold3);

        if (householdIds.Count == 0)
            return [];

        ModeledResponse<Household> householdList = await _supabaseClient.From<Household>().Where(x => householdIds.Contains(x.HHID)).Get();

        return householdList.Models ?? [];
    }

    private async Task<List<User>> LoadHouseholdUsersAsync(Household household)
    {
        if (_userSessionService.IsOfflineMode())
            return [];

        // Jelenlegi háztartás tagjai:
        ModeledResponse<User> usersquery = await _supabaseClient.From<User>()
            .Or(
            [
                new Supabase.Postgrest.QueryFilter("hh_1", Supabase.Postgrest.Constants.Operator.Equals, household.HHID),
                new Supabase.Postgrest.QueryFilter("hh_2", Supabase.Postgrest.Constants.Operator.Equals, household.HHID),
                new Supabase.Postgrest.QueryFilter("hh_3", Supabase.Postgrest.Constants.Operator.Equals, household.HHID)
            ])
            .Get();

        return usersquery.Models ?? [];
    }

    private async Task<Household?> LoadCurrentHouseholdAsync(User user)
    {
        // Jelenlegi háztartás:
        Household? currenthousehold = null;
        if (!_userSessionService.IsOfflineMode())
            currenthousehold = await _supabaseClient.From<Household>().Where(x => x.HHID == user.CurrentHousehold).Single();
        else
        {
            OfflineHousehold? offlineHousehold = await _sqliteDb.Table<OfflineHousehold>().Where(x => x.HHID == user.CurrentHousehold).FirstOrDefaultAsync();
            if (offlineHousehold != null)
            {
                currenthousehold = new()
                {
                    HHID = offlineHousehold.HHID,
                    HouseholdName = offlineHousehold.HouseholdName,
                    HouseholdOwner = offlineHousehold.HouseholdOwner,
                    HouseholdAccess = offlineHousehold.HouseholdAccess
                };
            }
        }

        return currenthousehold;
    }

    private async Task<List<Budget>> LoadBudgetsAsync(Household household)
    {
        // Jelenlegi háztartásban elérhető költségkeretek:
        if (!_userSessionService.IsOfflineMode())
        {
            ModeledResponse<Budget> budgetquery = await _supabaseClient.From<Budget>().Where(x => x.BudgetHousehold == household.HHID).Get();

            return budgetquery.Models ?? [];
        }
        else
        {
            List<OfflineBudget> offlineBudgets = await _sqliteDb.Table<OfflineBudget>().Where(x => x.BudgetHousehold == household.HHID).ToListAsync();
            List<Budget> budgets = [];
            foreach (OfflineBudget offlinebudget in offlineBudgets)
            {
                budgets.Add(new()
                {
                    BID = offlinebudget.BID,
                    BudgetHousehold = offlinebudget.BudgetHousehold,
                    BudgetDate = offlinebudget.BudgetDate,
                    BudgetAmount = offlinebudget.BudgetAmount
                });
            }

            return budgets ?? [];
        }
    }

    private async Task<List<Event>> LoadEventLogAsync(Household household)
    {
        // Jelenlegi háztartásban elérhető eseménynapló:
        if (!_userSessionService.IsOfflineMode())
        {
            ModeledResponse<Event> eventquery = await _supabaseClient.From<Event>().Where(x => x.EventHousehold == household.HHID).Get();

            return eventquery.Models ?? [];
        }
        else
        {
            List<OfflineEvent> offlineEventlog = await _sqliteDb.Table<OfflineEvent>().Where(x => x.EventHousehold == household.HHID).ToListAsync();
            List<Event> eventlog = [];
            foreach (OfflineEvent offlineevent in offlineEventlog)
            {
                eventlog.Add(new()
                {
                    EID = offlineevent.EID,
                    EventUser = offlineevent.EventUser,
                    EventHousehold = offlineevent.EventHousehold,
                    EventDate = offlineevent.EventDate,
                    EventName = offlineevent.EventName,
                    EventUserDate = offlineevent.EventUserDate,
                    EventType = offlineevent.EventType,
                    EventCost = offlineevent.EventCost,
                    EventDescription = offlineevent.EventDescription,
                    LastModifiedBy = offlineevent.LastModifiedBy
                });
            }

            return eventlog ?? [];
        }
    }

    public async Task<bool> CopyDataToOfflineDB()
    {
        try
        {
            // Offline táblázatok kiürítése:
            if (!loadOfflineDBOnce)
            {
                await Task.WhenAll(
                    _sqliteDb.DeleteAllAsync<OfflineUser>(),
                    _sqliteDb.DeleteAllAsync<OfflineHousehold>(),
                    _sqliteDb.DeleteAllAsync<OfflineBudget>(),
                    _sqliteDb.DeleteAllAsync<OfflineEvent>(),
                    _sqliteDb.DeleteAllAsync<OfflineNotification>()
                    );
            }

            User user = _userSessionService.GetCurrentUser()!;
            Household household = _userSessionService.GetCurrentHousehold() ?? new();
            List<Budget> budgets = _userSessionService.GetCurrentBudgets();
            List<Event> eventlog = _userSessionService.GetCurrentEventLog();

            var offlineData = new
            {
                User = new OfflineUser
                {
                    UID = user.UID,
                    Username = user.Username,
                    Password = user.Password,
                    CurrentHousehold = user.CurrentHousehold,
                    HouseHold1 = user.HouseHold1,
                    HouseHold2 = user.HouseHold2,
                    HouseHold3 = user.HouseHold3,
                    Nickname = user.Nickname
                },
                Household = new OfflineHousehold
                {
                    HHID = household.HHID,
                    HouseholdName = household.HouseholdName,
                    HouseholdAccess = household.HouseholdAccess,
                    HouseholdOwner = household.HouseholdOwner
                },
                Budgets = budgets.Select(x => new OfflineBudget
                {
                    BID = x.BID,
                    BudgetHousehold = x.BudgetHousehold,
                    BudgetDate = x.BudgetDate,
                    BudgetAmount = x.BudgetAmount
                }).ToList(),
                Events = eventlog.Select(x => new OfflineEvent
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
                }).ToList()
            };

            /*await _sqliteDb.InsertOrReplaceAsync(offlineData.User);
            await _sqliteDb.InsertOrReplaceAsync(offlineData.Household);
            if (offlineData.Budgets.Count > 0)
            {
                foreach (OfflineBudget budget in offlineData.Budgets)
                    await _sqliteDb.InsertOrReplaceAsync(budget);
            }
                
            if (offlineData.Events.Count > 0)
            {
                foreach (OfflineEvent _event in offlineData.Events)
                    await _sqliteDb.InsertOrReplaceAsync(_event);
           }*/

            await _sqliteDb.InsertAsync(offlineData.User);
            await _sqliteDb.InsertAsync(offlineData.Household);
            if (offlineData.Budgets.Count > 0)
                await _sqliteDb.InsertAllAsync(offlineData.Budgets);
            if (offlineData.Events.Count > 0)
                await _sqliteDb.InsertAllAsync(offlineData.Events);

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Hiba a másolás közben: {ex}");
            return false;
        }
    }

    public async Task<bool> SyncDatabasesAsync()
    {
        try
        {
            // Offline adatbázisban lévő új/módosított adatok feltöltése az online adatbázisba:
            List<OfflineBudget> newOfflineBudgets = await _sqliteDb.Table<OfflineBudget>().Where(x => x.BudgetMadeOffline).ToListAsync();
            List<OfflineEvent> newOfflineEvents = await _sqliteDb.Table<OfflineEvent>().Where(x => x.EventMadeOffline).ToListAsync();

            if (newOfflineBudgets.Count > 0)
            {
                List<Budget> budgets = newOfflineBudgets.Select(x => new Budget
                {
                    BID = x.BID,
                    BudgetHousehold = x.BudgetHousehold,
                    BudgetDate = x.BudgetDate,
                    BudgetAmount = x.BudgetAmount
                }).ToList();

                await _supabaseClient.From<Budget>().Upsert(budgets);
            }

            if (newOfflineEvents.Count > 0)
            {
                List<Event> events = newOfflineEvents.Select(x => new Event
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

                await _supabaseClient.From<Event>().Upsert(events);
            }

            // Offline táblázatok kiürítése:
            await Task.WhenAll(
                _sqliteDb.DeleteAllAsync<OfflineUser>(),
                _sqliteDb.DeleteAllAsync<OfflineHousehold>(),
                _sqliteDb.DeleteAllAsync<OfflineBudget>(),
                _sqliteDb.DeleteAllAsync<OfflineEvent>(),
                _sqliteDb.DeleteAllAsync<OfflineNotification>()
                );

            return true;
        }
        catch
        {
            return false;
        }
    }
}