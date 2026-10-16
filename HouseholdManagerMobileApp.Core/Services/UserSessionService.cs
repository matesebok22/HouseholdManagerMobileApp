using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public class UserSessionService : IUserSessionService
{
    private bool userLoggedIn = false;
    private bool isOfflineMode = false;
    private User? currentUser = null;
    private Household? currentHousehold = null;
    private List<Household> householdList = [];
    private List<User> householdUsers = [];
    private List<Budget> currentBudgets = [];
    private List<Event> currentEventLog = [];
    private bool currentHouseholdOwner = false;
    private Event? selectedEvent = null;

    public bool IsUserLoggedIn() => userLoggedIn;
    public bool IsOfflineMode() => isOfflineMode;
    public bool IsCurrentHouseholdOwner() => currentHouseholdOwner;

    public User? GetCurrentUser() => currentUser;
    public Household? GetCurrentHousehold() => currentHousehold;
    public List<Household> GetCurrentHouseholdList() => householdList;
    public List<User> GetCurrentHouseholdUsers() => householdUsers;
    public List<Budget> GetCurrentBudgets() => currentBudgets;
    public List<Event> GetCurrentEventLog() => currentEventLog;
    public Event? GetSelectedEvent() => selectedEvent;
    public void SetSelectedEvent(Event? _event) => selectedEvent = _event;

    public async Task SetUserSessionAsync(User user)
    {
        userLoggedIn = true;
        isOfflineMode = Connectivity.Current.NetworkAccess != NetworkAccess.Internet;
        currentUser = user;

        await Task.CompletedTask;
    }

    public async Task ClearUserSessionAsync()
    {
        userLoggedIn = false;
        currentUser = null;
        currentHousehold = null;
        currentHouseholdOwner = false;
        selectedEvent = null;

        householdList.Clear();
        householdUsers.Clear();
        currentBudgets.Clear();
        currentEventLog.Clear();

        await Task.CompletedTask;
    }

    public async Task ChangeHousehold(Household? currenthousehold, List<User> householdusers, List<Household> households, List<Budget> budgets)
    {
        currentHouseholdOwner = currentUser!.UID == currenthousehold?.HouseholdOwner;
        currentHousehold = currenthousehold;
        householdUsers = householdusers;
        householdList.Clear();
        currentBudgets.Clear();
        currentEventLog.Clear();
        householdList = households;
        currentBudgets = budgets;

        await Task.CompletedTask;
    }

    public async Task ReloadUser(User user)
    {
        currentUser = user;

        await Task.CompletedTask;
    }

    public async Task ReloadEventLog(List<Event> eventlog)
    {
        currentEventLog.Clear();
        currentEventLog = eventlog;

        await Task.CompletedTask;
    }
}