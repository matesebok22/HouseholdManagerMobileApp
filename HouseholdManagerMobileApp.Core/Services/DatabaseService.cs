using HouseholdManagerMobileApp.Core.Models;
using SQLite;
using Supabase;
using Supabase.Postgrest.Responses;

namespace HouseholdManagerMobileApp.Core.Services;

public class DatabaseService : IDatabaseService
{
    private readonly Client _supabaseClient;
    private readonly SQLiteAsyncConnection _sqliteDb;
    private readonly IUserSessionService _userSessionService;

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
            Dictionary<string, object> parameters = new()
            {
                { "p_username", _userSessionService.GetCurrentUser().Username },
                { "p_householdname", householdName }
            };

            var response = await _supabaseClient.Rpc("create_household", parameters);

            if (response.Content != null && response.Content.Trim() == "true")
            {
                User? user = await _supabaseClient.From<User>().Where(x => x.Username == _userSessionService.GetCurrentUser().Username).Single();
                if (user == null)
                    return false;

                Household? currentHousehold = await _supabaseClient.From<Household>().Where(x => x.HHID == user.CurrentHousehold).Single();
                if (currentHousehold == null)
                    return false;

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

        User? user = await _supabaseClient.From<User>().Where(x => x.Username == _userSessionService.GetCurrentUser().Username).Single();

        if (user == null)
            return false;

        // Már tagja-e a felhasználó a háztartásnak:
        if (user.HouseHold1 == household.HHID || user.HouseHold2 == household.HHID || user.HouseHold3 == household.HHID)
            return false;

        // Van-e szabad háztartás hely a felhasználónak:
        if (user.HouseHold1 == null)
        {
            user.HouseHold1 = household.HHID;
            user.CurrentHousehold = household.HHID;
            await user.Update<User>();

            return true;
        }
        else if (user.HouseHold2 == null)
        {
            user.HouseHold2 = household.HHID;
            user.CurrentHousehold = household.HHID;
            await user.Update<User>();

            return true;
        }
        else if (user.HouseHold3 == null)
        {
            user.HouseHold3 = household.HHID;
            user.CurrentHousehold = household.HHID;
            await user.Update<User>();

            return true;
        }

        return false;
    }

    public async Task<bool> ChangeSelectedHouseholdAsync(string currentHousehold)
    {
        if (!_userSessionService.IsOfflineMode())
        {
            Household? newHousehold = await _supabaseClient.From<Household>().Where(x => x.HouseholdAccess == currentHousehold).Single();
            if (newHousehold == null)
                return false;

            User? user = await _supabaseClient.From<User>().Where(x => x.Username == _userSessionService.GetCurrentUser().Username).Single();
            if (user == null)
                return false;

            user.CurrentHousehold = newHousehold.HHID;
            await user.Update<User>();

            List<Budget> budgets = await LoadBudgetsAsync(newHousehold);
            List<Event> eventlog = await LoadEventLogAsync(newHousehold);
            List<Household> households = await LoadHouseholdsAsync(user);

            await _userSessionService.ChangeHousehold(newHousehold, households, budgets);
        }

        return true;
    }

    public async Task<bool> EditHouseholdAsync(string newHouseholdName, DateTime householdBudgetMonth, int newHouseholdBudget)
    {
        string currentDate = $"{householdBudgetMonth.Year}. {householdBudgetMonth.Month}.";

        if (_userSessionService.GetCurrentHousehold().HouseholdName != newHouseholdName)
        {
            if (!_userSessionService.IsOfflineMode())
                await _supabaseClient.From<Household>().Where(x => x.HHID == _userSessionService.GetCurrentHousehold().HHID).Set(x => x.HouseholdName, newHouseholdName).Update();
            else
            {
                string sqliteQuery = @"UPDATE offline_households SET hh_name = ? WHERE hhid = ?";
                await _sqliteDb.ExecuteAsync(sqliteQuery, newHouseholdName, _userSessionService.GetCurrentHousehold().HHID);

                _userSessionService.GetCurrentHousehold().HouseholdName = newHouseholdName;
            }
        }
            
        // Háztartáshoz tartozó összes költségkeret lekérése:
        Budget? budget = _userSessionService.GetCurrentBudgets().Find(x => $"{x.BudgetDate.Year}. {x.BudgetDate.Month}." == currentDate);
        if (budget == null)
        {
            DateTime newDate = new(householdBudgetMonth.Year, householdBudgetMonth.Month, 1);

            if (!_userSessionService.IsOfflineMode())
                await _supabaseClient.From<Budget>().Insert(new Budget() { BudgetHousehold = _userSessionService.GetCurrentHousehold().HHID, BudgetDate = newDate, BudgetAmount = newHouseholdBudget });
            else
            {
                string sqliteQuery = @"INSERT INTO offline_budgets (bid, b_hh, b_userdate, b_amount, b_made_offline) VALUES (?, ?, ?, ?, 1)";
                await _sqliteDb.ExecuteAsync(sqliteQuery, Guid.NewGuid().ToString(), _userSessionService.GetCurrentHousehold().HHID, newDate, newHouseholdBudget);
            }
        }
        else
        {
            if (budget.BudgetAmount != newHouseholdBudget)
            {
                budget.BudgetAmount = newHouseholdBudget;
                if (!_userSessionService.IsOfflineMode())
                    await budget.Update<Budget>();
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
        User? user = await _supabaseClient.From<User>().Where(x => x.UID == _userSessionService.GetCurrentUser().UID).Single();
        if (user == null)
            return false;

        Household? household = await _supabaseClient.From<Household>().Where(x => x.HHID == _userSessionService.GetCurrentHousehold().HHID).Single();
        if (household == null)
            return false;

        if (user.HouseHold1 == household.HHID)
        {
            user.HouseHold1 = null;
            if (user.HouseHold2 != null)
                user.CurrentHousehold = user.HouseHold2;
            if (user.HouseHold3 != null)
                user.CurrentHousehold = user.HouseHold3;
        }
            
        if (user.HouseHold2 == household.HHID)
        {
            user.HouseHold2 = null;
            if (user.HouseHold1 != null)
                user.CurrentHousehold = user.HouseHold1;
            if (user.HouseHold3 != null)
                user.CurrentHousehold = user.HouseHold3;
        }
            
        if (user.HouseHold3 == household.HHID)
        {
            user.HouseHold3 = null;
            if (user.HouseHold1 != null)
                user.CurrentHousehold = user.HouseHold1;
            if (user.HouseHold2 != null)
                user.CurrentHousehold = user.HouseHold2;
        }

        if (user.HouseHold1 == null && user.HouseHold2 == null && user.HouseHold3 == null)
            user.CurrentHousehold = "";

        await user.Update<User>();

        // Ha a jelenlegi háztartás tulajdonosa, akkor megszünteti a háztartást is.
        if (_userSessionService.IsCurrentHouseholdOwner())
        {
            household.HouseholdOwner = null;
            await household.Update<Household>();
            await household.Delete<Household>();
        }

        SecureStorage.Remove($"notifications-{user.UID}-{household.HHID}");

        return true;
    }

    public async Task<bool> CreateEventAsync(string eventName, DateTime eventUserDate, string eventType, int eventCost, string eventDesc)
    {
        Event newEvent = new()
        {
            EventUser = _userSessionService.GetCurrentUser().UID,
            EventHousehold = _userSessionService.GetCurrentHousehold().HHID,
            EventDate = DateTime.Now,
            EventName = eventName,
            EventUserDate = eventUserDate,
            EventType = eventType,
            EventCost = eventCost,
            EventDescription = eventDesc,
            LastModifiedBy = _userSessionService.GetCurrentUser().UID
        };

        if (!_userSessionService.IsOfflineMode())
        {
            var response = await _supabaseClient.From<Event>().Insert(newEvent);
            if (response.Models != null && response.Models.Count > 0)
                return true;
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
                                    .Where(x => x.EID == _userSessionService.GetSelectedEvent().EID)
                                    .Set(x => x.EventName, eventName)
                                    .Set(x => x.EventUserDate, eventUserDate)
                                    .Set(x => x.EventType, eventType)
                                    .Set(x => x.EventCost, eventCost)
                                    .Set(x => x.EventDescription, eventDesc)
                                    .Set(x => x.LastModifiedBy, _userSessionService.GetCurrentUser().UID)
                                    .Update();
            if (response.Models != null && response.Models.Count > 0)
                return true;
        }
        else
        {
            string sqliteQuery = @"UPDATE offline_events SET e_name = ?, e_userdate = ?, e_type = ?, e_cost = ?, e_desc = ?, last_modified_by = ?, e_made_offline = 1 WHERE eid = ?";
            await _sqliteDb.ExecuteAsync(sqliteQuery, eventName, eventUserDate, eventType, eventCost, eventDesc, _userSessionService.GetCurrentUser().UID, _userSessionService.GetSelectedEvent().EID);
            return true;
        }

        return false;
    }

    public async Task DeleteEventAsync()
    {
        await _supabaseClient.From<Event>().Where(x => x.EID == _userSessionService.GetSelectedEvent().EID).Delete();
    }

    public async Task<bool> LoadDataAsync(int mode)
    {
        switch (mode)
        {
            // Beállítások adatainak betöltése:
            case 1:
                {
                    if (!_userSessionService.IsOfflineMode())
                    {
                        List<Budget> budgets = await LoadBudgetsAsync(_userSessionService.GetCurrentHousehold());
                        List<Household> households = await LoadHouseholdsAsync(_userSessionService.GetCurrentUser());
                        await _userSessionService.ChangeHousehold(_userSessionService.GetCurrentHousehold(), households, budgets);
                    }
                    else
                    {
                        List<OfflineBudget> offlineBudgets = await _sqliteDb.Table<OfflineBudget>().ToListAsync();
                        List<Budget> budgets = [];
                        foreach (OfflineBudget budget in offlineBudgets)
                        {
                            budgets.Add(new()
                            {
                                BID = budget.BID,
                                BudgetHousehold = budget.BudgetHousehold,
                                BudgetDate = budget.BudgetDate,
                                BudgetAmount = budget.BudgetAmount
                            });
                        }
                        await _userSessionService.ChangeHousehold(_userSessionService.GetCurrentHousehold(), [], budgets);
                    }

                    break;
                }
            // Eseménynapló adatainak betöltése:
            case 2:
                {
                    List<Event> eventlog = [];
                    if (!_userSessionService.IsOfflineMode())
                        eventlog = await LoadEventLogAsync(_userSessionService.GetCurrentHousehold());
                    else
                    {
                        List<OfflineEvent> offlineEventlog = await _sqliteDb.Table<OfflineEvent>().ToListAsync();
                        foreach (OfflineEvent _event in offlineEventlog)
                        {
                            eventlog.Add(new()
                            {
                                EID = _event.EID,
                                EventUser = _event.EventUser,
                                EventHousehold = _event.EventHousehold,
                                EventDate = _event.EventDate,
                                EventName = _event.EventName,
                                EventUserDate = _event.EventUserDate,
                                EventType = _event.EventType,
                                EventCost = _event.EventCost,
                                EventDescription = _event.EventDescription,
                                LastModifiedBy = _event.LastModifiedBy
                            });
                        }
                    }

                    await _userSessionService.ReloadEventLog(eventlog);
                    break;
                }
        }

        return true;
    }

    private async Task<List<Household>> LoadHouseholdsAsync(User user)
    {
        List<string> householdIds = new();
        if (!string.IsNullOrEmpty(user.HouseHold1)) householdIds.Add(user.HouseHold1);
        if (!string.IsNullOrEmpty(user.HouseHold2)) householdIds.Add(user.HouseHold2);
        if (!string.IsNullOrEmpty(user.HouseHold3)) householdIds.Add(user.HouseHold3);

        if (householdIds.Count == 0)
            return [];

        ModeledResponse<Household> householdList = await _supabaseClient.From<Household>().Where(x => householdIds.Contains(x.HHID)).Get();

        return householdList.Models ?? [];
    }

    private async Task<List<Budget>> LoadBudgetsAsync(Household household)
    {
        // Jelenlegi háztartásban elérhető költségkeretek:
        List<Budget> budgets = [];
        var budgetquery = await _supabaseClient.From<Budget>().Where(x => x.BudgetHousehold == household.HHID).Get();
        foreach (Budget budget in budgetquery.Models)
            budgets.Add(budget);

        return budgets;
    }

    private async Task<List<Event>> LoadEventLogAsync(Household household)
    {
        // Jelenlegi háztartásban elérhető eseménynapló:
        List<Event> eventlog = [];
        var eventquery = await _supabaseClient.From<Event>().Where(x => x.EventHousehold == household.HHID).Get();
        foreach (Event _event in eventquery.Models)
            eventlog.Add(_event);

        return eventlog;
    }

    public async Task<bool> CopyDataToOfflineDB()
    {
        try
        {
            User user = _userSessionService.GetCurrentUser();
            Household household = _userSessionService.GetCurrentHousehold();
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

            await _sqliteDb.InsertAsync(offlineData.User);
            await _sqliteDb.InsertAsync(offlineData.Household);
            if (offlineData.Budgets.Count > 0)
                await _sqliteDb.InsertAllAsync(offlineData.Budgets);
            if (offlineData.Events.Count > 0)
                await _sqliteDb.InsertAllAsync(offlineData.Events);

            return true;
        }
        catch
        {
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