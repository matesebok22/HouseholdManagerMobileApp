using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public interface IUserSessionService
{
    bool IsUserLoggedIn();
    bool IsCurrentHouseholdOwner();
    public User? GetCurrentUser();
    public Household? GetCurrentHousehold();
    public List<Household> GetCurrentHouseholdList();
    public List<Budget> GetCurrentBudgets();
    public List<Event> GetCurrentEventLog();
    public Event? GetSelectedEvent();
    public void SetSelectedEvent(Event? _event);
    Task SetUserSessionAsync(User user, Household household, List<Household> hhList, List<Budget> budgets, List<Event> eventlog);
    Task ClearUserSessionAsync();
    Task ChangeHousehold(Household household, List<Household> households, List<Budget> budgets, List<Event> eventlog);
    Task ReloadEventLog(List<Event> eventlog);
}