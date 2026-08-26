using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public class UserSessionService : IUserSessionService
{
    private bool userLoggedIn = false;
    private User? currentUser = null;
    private Household? currentHousehold = null;
    private List<Household> householdList = [];
    private List<Budget> currentBudgets = [];
    private List<Event> currentEventLog = [];
    private bool currentHouseholdOwner = false;
    private Event? selectedEvent = null;

    public bool IsUserLoggedIn() => userLoggedIn;

    public bool IsCurrentHouseholdOwner() => currentHouseholdOwner;

    public User? GetCurrentUser() => currentUser;
    public Household? GetCurrentHousehold() => currentHousehold;
    public List<Household> GetCurrentHouseholdList() => householdList;
    public List<Budget> GetCurrentBudgets() => currentBudgets;
    public List<Event> GetCurrentEventLog() => currentEventLog;
    public Event? GetSelectedEvent() => selectedEvent;

    public async Task SetUserSessionAsync(User user, Household household, List<Household> hhList, List<Budget> budgets, List<Event> eventlog)
    {
        userLoggedIn = true;
        currentUser = user;
        currentHousehold = household;
        householdList = hhList;
        currentBudgets = budgets;
        currentEventLog = eventlog;
        currentHouseholdOwner = household != null ? user.UID == household.HouseholdOwner : false;
        await Task.CompletedTask;
    }

    public async Task ClearUserSessionAsync()
    {
        userLoggedIn = false;
        currentUser = null;
        currentHousehold = null;
        currentBudgets.Clear();
        currentEventLog.Clear();
        currentHouseholdOwner = false;
        selectedEvent = null;
        await Task.CompletedTask;
    }

    public async Task ChangeHousehold(Household currenthousehold, List<Household> households, List<Budget> budgets, List<Event> eventlog)
    {
        currentHouseholdOwner = currentUser.UID == currenthousehold.HouseholdOwner;
        currentHousehold = currenthousehold;
        householdList.Clear();
        householdList = households;
        currentBudgets.Clear();
        currentBudgets = budgets;
        await ReloadEventLog(eventlog);
    }

    public async Task ReloadEventLog(List<Event> eventlog)
    {
        currentEventLog.Clear();
        currentEventLog = eventlog;

        await Task.CompletedTask;
    }
}