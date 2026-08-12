namespace HouseholdManagerMobileApp.Core.Services;

public interface IDatabaseService
{
    Task InitializeAsync();
    Task<bool> CreateHouseholdAsync(string username, string householdName);
}
