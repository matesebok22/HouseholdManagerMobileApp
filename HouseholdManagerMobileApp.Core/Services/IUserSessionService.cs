using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public interface IUserSessionService
{
    bool IsUserLoggedIn(); // A felhasználó bejelentkezett?
    bool IsOfflineMode(); // A felhasználó offline?
    bool IsCurrentHouseholdOwner(); // A felhasználó a jelenlegi háztartás vezetője?
    public User? GetCurrentUser();
    public Household? GetCurrentHousehold();
    public List<Household> GetCurrentHouseholdList();
    public List<User> GetCurrentHouseholdUsers();
    public List<Budget> GetCurrentBudgets();
    public List<Event> GetCurrentEventLog();
    public Event? GetSelectedEvent();
    public void SetSelectedEvent(Event? _event);
    Task SetUserSessionAsync(User user); // Sikeres bejelentkezéskor beállítja a sessiont.
    Task ClearUserSessionAsync(); // Sikeres kijelentkezéskor kiüríti a sessiont.
    Task ChangeHousehold(Household? currenthousehold, List<User> householdusers, List<Household> households, List<Budget> budgets); // Felhasználó háztartást vált.
    Task ReloadUser(User user); // Felhasználó újratöltése.
    Task ReloadEventLog(List<Event> eventlog); // Eseménynapló újratöltése.
}