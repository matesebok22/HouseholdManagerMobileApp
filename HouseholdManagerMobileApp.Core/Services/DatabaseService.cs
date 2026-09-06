using Supabase;
using SQLite;

using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public class DatabaseService : IDatabaseService
{
    private readonly Client _supabaseClient;
    private readonly SQLiteAsyncConnection _sqliteDb;
    private readonly IUserSessionService _userSessionService;
    private readonly INotificationService _notificationService;
    public DatabaseService(Client supabaseClient, SQLiteAsyncConnection sqliteDb, IUserSessionService userSessionService, INotificationService notificationService)
    {
        _supabaseClient = supabaseClient;
        _sqliteDb = sqliteDb;
        _userSessionService = userSessionService;
        _notificationService = notificationService;
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

        return true;
    }

    public async Task<bool> EditHouseholdAsync(string newHouseholdName, DateTime householdBudgetMonth, int newHouseholdBudget)
    {
        string currentDate = $"{householdBudgetMonth.Year}. {householdBudgetMonth.Month}.";

        if (_userSessionService.GetCurrentHousehold().HouseholdName != newHouseholdName)
            await _supabaseClient.From<Household>().Where(x => x.HHID == _userSessionService.GetCurrentHousehold().HHID).Set(x => x.HouseholdName, newHouseholdName).Update();

        // Háztartáshoz tartozó összes költségkeret lekérése:
        Budget? budget = _userSessionService.GetCurrentBudgets().Find(x => $"{x.BudgetDate.Year}. {x.BudgetDate.Month}." == currentDate);
        if (budget == null)
        {
            DateTime newDate = new(householdBudgetMonth.Year, householdBudgetMonth.Month, 1);
            await _supabaseClient.From<Budget>().Insert(new Budget() { BudgetHousehold = _userSessionService.GetCurrentHousehold().HHID, BudgetDate = newDate, BudgetAmount = newHouseholdBudget });
        }
        else
        {
            if (budget.BudgetAmount != newHouseholdBudget)
            {
                budget.BudgetAmount = newHouseholdBudget;
                await budget.Update<Budget>();
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

        var response = await _supabaseClient.From<Event>().Insert(newEvent);
        if (response.Models != null && response.Models.Count > 0)
            return true;
        else
            return false;
    }

    public async Task<bool> EditEventAsync(string eventName, DateTime eventUserDate, string eventType, int eventCost, string eventDesc)
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
        else
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
                    List<Budget> budgets = await LoadBudgetsAsync(_userSessionService.GetCurrentHousehold());
                    List<Household> households = await LoadHouseholdsAsync(_userSessionService.GetCurrentUser());
                    await _userSessionService.ChangeHousehold(_userSessionService.GetCurrentHousehold(), households, budgets);
                    break;
                }
            // Eseménynapló adatainak betöltése:
            case 2:
                {
                    List<Event> eventlog = await LoadEventLogAsync(_userSessionService.GetCurrentHousehold());
                    await _userSessionService.ReloadEventLog(eventlog);
                    break;
                }
        }

        return true;
    }

    private async Task<List<Household>> LoadHouseholdsAsync(User user)
    {
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

        return householdList;
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
            OfflineUser offUser = new()
            {
                UID = user.UID,
                Username = user.Username,
                Password = user.Password,
                CurrentHousehold = user.CurrentHousehold,
                HouseHold1 = user.HouseHold1,
                HouseHold2 = user.HouseHold2,
                HouseHold3 = user.HouseHold3,
                Nickname = user.Nickname
            };
            await _sqliteDb.InsertOrReplaceAsync(offUser);

            Household household = _userSessionService.GetCurrentHousehold();
            OfflineHousehold offHousehold = new()
            {
                HHID = household.HHID,
                HouseholdName = household.HouseholdName,
                HouseholdAccess = household.HouseholdAccess,
                HouseholdOwner = household.HouseholdOwner
            };
            await _sqliteDb.InsertOrReplaceAsync(offHousehold);

            List<Budget> budgets = _userSessionService.GetCurrentBudgets();
            foreach (Budget budget in budgets)
            {
                OfflineBudget offBudget = new()
                {
                    BID = budget.BID,
                    BudgetHousehold = budget.BudgetHousehold,
                    BudgetDate = budget.BudgetDate,
                    BudgetAmount = budget.BudgetAmount
                };
                await _sqliteDb.InsertOrReplaceAsync(offBudget);
            }

            List<Event> eventlog = _userSessionService.GetCurrentEventLog();
            foreach (Event _event in eventlog)
            {
                OfflineEvent offEvent = new()
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
                };
                await _sqliteDb.InsertOrReplaceAsync(offEvent);
            }

            var response = await _supabaseClient.From<Notification>().Where(x => x.RecipientUserId == user.UID).Get();
            List<Notification> notifications = response.Models ?? [];
            foreach (Notification notification in notifications)
            {
                OfflineNotification offNotification = new()
                {
                    NID = notification.NID,
                    RecipientUserId = notification.RecipientUserId,
                    EventId = notification.EventId,
                    NotificationType = notification.NotificationType,
                    Title = notification.Title,
                    Message = notification.Message,
                    RelatedHouseholdId = notification.RelatedHouseholdId,
                    IsRead = notification.IsRead,
                    CreatedAt = notification.CreatedAt,
                    UpdatedAt = notification.UpdatedAt
                };
                await _sqliteDb.InsertOrReplaceAsync(offNotification);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }
}