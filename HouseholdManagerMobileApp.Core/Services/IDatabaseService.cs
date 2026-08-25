using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<bool> LoadHouseholdsAsync();
    Task<bool> CreateHouseholdAsync(string householdName);
    Task<bool> JoinHouseholdAsync(string householdAccess);
    Task<bool> ChangeSelectedHouseholdAsync(string currentHousehold);
    Task<bool> EditHouseholdAsync(string newHouseholdName, DateTime householdBudgetMonth, int newHouseholdBudget);
    Task<bool> LeaveHouseholdAsync();
}