using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public interface IDatabaseService
{
    public bool CurrentHouseholdOwner { get; }
    public string CurrentHouseholdName { get; }
    public string CurrentHouseholdAccess { get; }
    public List<Budget> CurrentHouseholdBudgets { get; }
    Task InitializeAsync();
    Task<List<(string, string)>> LoadHouseholdsAsync(string username);
    Task<bool> CreateHouseholdAsync(string username, string householdName);
    Task<bool> JoinHouseholdAsync(string username, string householdAccess);
    Task<bool> ChangeSelectedHouseholdAsync(string username, string currentHousehold);
    Task<bool> EditHouseholdAsync(string newHouseholdName, DateTime householdBudgetMonth, int newHouseholdBudget);
    Task<bool> LeaveHouseholdAsync(string username);
}