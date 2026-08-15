using HouseholdManagerMobileApp.Core.Models;
using Supabase;

namespace HouseholdManagerMobileApp.Core.Services;

public class DatabaseService : IDatabaseService
{
    private readonly Client _supabaseClient;
    public string CurrentHouseholdName { get; private set; } = string.Empty;
    public string CurrentHouseholdAccess { get; private set; } = string.Empty;

    public DatabaseService(Client supabaseClient)
    {
        _supabaseClient = supabaseClient;
    }

    public async Task InitializeAsync()
    {
        // Supabase adatbázis elindítása:
        try
        {
            await _supabaseClient.InitializeAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Hiba történt a Supabase adatbázis inicializálása során.\n", ex);
        }
    }

    public async Task<List<(string, string)>> LoadHouseholdsAsync(string username)
    {
        User? user = await _supabaseClient.From<User>().Where(x => x.Username == username).Single();
        if (user == null)
            return [];

        List<(string, string)> households = [];

        if (user.HouseHold1 != null)
        {
            var response = await _supabaseClient.From<Household>().Where(x => x.HHID == user.HouseHold1).Single();
            if (response == null)
                return [];
            households.Add((response.HouseholdName, response.HouseholdAccess));
        }
        if (user.HouseHold2 != null)
        {
            var response = await _supabaseClient.From<Household>().Where(x => x.HHID == user.HouseHold2).Single();
            if (response == null)
                return [];
            households.Add((response.HouseholdName, response.HouseholdAccess));
        }    
        if (user.HouseHold3 != null)
        {
            var response = await _supabaseClient.From<Household>().Where(x => x.HHID == user.HouseHold3).Single();
            if (response == null)
                return [];
            households.Add((response.HouseholdName, response.HouseholdAccess));
        }

        return households;
    }

    public async Task<bool> CreateHouseholdAsync(string username, string householdName)
    {
        try
        {
            Dictionary<string, object> parameters = new()
            {
                { "p_username", username },
                { "p_householdname", householdName }
            };

            var response = await _supabaseClient.Rpc("create_household", parameters);

            if (response.Content != null && response.Content.Trim() == "true")
                return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> JoinHouseholdAsync(string username, string householdAccess)
    {
        Household? household = await _supabaseClient.From<Household>().Where(x => x.HouseholdAccess == householdAccess).Single();

        if (household == null)
            return false;

        User? user = await _supabaseClient.From<User>().Where(x => x.Username == username).Single();

        if (user == null)
            return false;

        // Van-e szabad háztartás hely a felhasználónak:
        if (user.HouseHold1 == null)
        {
            user.HouseHold1 = household.HHID;
            user.CurrentHousehold = household.HHID;
            await user.Update<User>();

            CurrentHouseholdName = household.HouseholdName;
            CurrentHouseholdAccess = household.HouseholdAccess;
            return true;
        }
        else if (user.HouseHold2 == null)
        {
            user.HouseHold2 = household.HHID;
            user.CurrentHousehold = household.HHID;
            await user.Update<User>();

            CurrentHouseholdName = household.HouseholdName;
            CurrentHouseholdAccess = household.HouseholdAccess;
            return true;
        }
        else if (user.HouseHold3 == null)
        {
            user.HouseHold3 = household.HHID;
            user.CurrentHousehold = household.HHID;
            await user.Update<User>();

            CurrentHouseholdName = household.HouseholdName;
            CurrentHouseholdAccess = household.HouseholdAccess;
            return true;
        }

        return false;
    }

    public Task<bool> UpdateHouseholdAsync(string newHouseholdName, DateTime householdBudgetMonth, int newHouseholdBudget)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsHouseholdOwnerAsync(string username)
    {
        throw new NotImplementedException();
    }
}