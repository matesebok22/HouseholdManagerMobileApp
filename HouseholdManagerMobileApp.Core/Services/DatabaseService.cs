using HouseholdManagerMobileApp.Core.Models;
using Supabase;

namespace HouseholdManagerMobileApp.Core.Services;

public class DatabaseService : IDatabaseService
{
    private readonly Client _supabaseClient;
    public bool CurrentHouseholdOwner { get; private set; } = false;
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
        CurrentHouseholdOwner = false;
        CurrentHouseholdName = string.Empty;
        CurrentHouseholdAccess = string.Empty;

        User? user = await _supabaseClient.From<User>().Where(x => x.Username == username).Single();
        if (user == null)
            return [];

        Household? currentHousehold = await _supabaseClient.From<Household>().Where(x => x.HHID == user.CurrentHousehold).Single();
        if (currentHousehold == null)
            return [];

        CurrentHouseholdOwner = currentHousehold.HouseholdOwner == user.UID ? true : false;
        CurrentHouseholdName = currentHousehold.HouseholdName;
        CurrentHouseholdAccess = currentHousehold.HouseholdAccess;

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
            {
                User? user = await _supabaseClient.From<User>().Where(x => x.Username == username).Single();
                if (user == null)
                    return false;

                Household? currentHousehold = await _supabaseClient.From<Household>().Where(x => x.HHID == user.CurrentHousehold).Single();
                if (currentHousehold == null)
                    return false;

                CurrentHouseholdOwner = true;
                CurrentHouseholdName = currentHousehold.HouseholdName;
                CurrentHouseholdAccess = currentHousehold.HouseholdAccess;

                return true;
            }

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

            CurrentHouseholdOwner = false;
            CurrentHouseholdName = household.HouseholdName;
            CurrentHouseholdAccess = household.HouseholdAccess;
            return true;
        }
        else if (user.HouseHold2 == null)
        {
            user.HouseHold2 = household.HHID;
            user.CurrentHousehold = household.HHID;
            await user.Update<User>();

            CurrentHouseholdOwner = false;
            CurrentHouseholdName = household.HouseholdName;
            CurrentHouseholdAccess = household.HouseholdAccess;
            return true;
        }
        else if (user.HouseHold3 == null)
        {
            user.HouseHold3 = household.HHID;
            user.CurrentHousehold = household.HHID;
            await user.Update<User>();

            CurrentHouseholdOwner = false;
            CurrentHouseholdName = household.HouseholdName;
            CurrentHouseholdAccess = household.HouseholdAccess;
            return true;
        }

        return false;
    }

    public async Task<bool> ChangeSelectedHouseholdAsync(string username, string currentHousehold)
    {
        Household? household = await _supabaseClient.From<Household>().Where(x => x.HouseholdAccess == currentHousehold).Single();
        if (household == null)
            return false;

        User? user = await _supabaseClient.From<User>().Where(x => x.Username == username).Single();
        if (user == null)
            return false;

        CurrentHouseholdOwner = household.HouseholdOwner == user.UID ? true : false;
        user.CurrentHousehold = household.HHID;
        await user.Update<User>();
        CurrentHouseholdName = household.HouseholdName;
        CurrentHouseholdAccess = household.HouseholdAccess;

        return true;
    }

    public Task<bool> UpdateHouseholdAsync(string newHouseholdName, DateTime householdBudgetMonth, int newHouseholdBudget)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> LeaveHouseholdAsync(string username)
    {
        User? user = await _supabaseClient.From<User>().Where(x => x.Username == username).Single();
        if (user == null)
            return false;

        Household? household = await _supabaseClient.From<Household>().Where(x => x.HouseholdAccess == CurrentHouseholdAccess).Single();
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

        CurrentHouseholdOwner = false;
        CurrentHouseholdName = string.Empty;
        CurrentHouseholdAccess = string.Empty;

        return true;
    }
}