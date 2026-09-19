using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public interface IDatabaseService
{
    Task InitializeAsync(); // Supabase és SQLite adatbázisok inicializálása.
    Task<bool> LoadDataAsync(int mode); // A beállítások és eseménynapló oldalak számára adatbetöltést biztosít.
    Task<bool> CreateHouseholdAsync(string householdName); // Új háztartás készítése online módban.
    Task<bool> JoinHouseholdAsync(string householdAccess); // Új háztartáshoz csatlakozás online módban.
    Task<bool> ChangeSelectedHouseholdAsync(string currentHousehold); // Jelenlegi háztartás megváltoztatása online módban.
    Task<bool> EditHouseholdAsync(string newHouseholdName, DateTime householdBudgetMonth, int newHouseholdBudget); // Jelenlegi háztartás nevének és költségkereteinek módosítása online és offline módban.
    Task<bool> LeaveHouseholdAsync(); // Jelenlegi háztartásból kilépés, vagy vezetőként felbontása online módban.
    Task<bool> CreateEventAsync(string eventName, DateTime eventUserDate, string eventType, int eventCost, string eventDesc); // Új esemény készítése online és offline módban.
    Task<bool> EditEventAsync(string eventName, DateTime eventUserDate, string eventType, int eventCost, string eventDesc); // Már létező esemény módosítása online és offline módban.
    Task DeleteEventAsync(); // Esemény törlése online módban.
    Task<bool> CopyDataToOfflineDB(); // Supabasen tárolt adatok másolása lokális SQLite adatbázisba.
    Task<bool> SyncDatabasesAsync(); // Online bejelentkezésnél a Supabase adatbázis szinkronizálása a lokális SQlite adatbázissal.
}