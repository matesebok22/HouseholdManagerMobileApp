using HouseholdManagerMobileApp.Core.Models;
using Supabase;

namespace HouseholdManagerMobileApp.Core.Services;

public class DatabaseService : IDatabaseService
{
    private readonly Client _supabaseClient;
    private readonly IUserSessionService _userSessionService;
    public DatabaseService(Client supabaseClient, IUserSessionService userSessionService)
    {
        _supabaseClient = supabaseClient;
        _userSessionService = userSessionService;
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

    public async Task<bool> LoadHouseholdsAsync()
    {
        User? user = await _supabaseClient.From<User>().Where(x => x.Username == /*username*/ _userSessionService.GetCurrentUser().Username).Single();
        if (user == null)
            return false;

        if (user.HouseHold1 == null && user.HouseHold2 == null && user.HouseHold3 == null)
        {
            user.CurrentHousehold = string.Empty;
            await user.Update<User>();
        }

        Household? currentHousehold = await _supabaseClient.From<Household>().Where(x => x.HHID == user.CurrentHousehold).Single();
        if (currentHousehold == null)
            return false;

        // Jelenlegi háztartásban elérhető költségkeretek:
        List<Budget> budgets = [];
        var budgetquery = await _supabaseClient.From<Budget>().Where(x => x.BudgetHousehold == currentHousehold.HHID).Get();
        foreach (Budget budget in budgetquery.Models)
            budgets.Add(budget);

        List<Household> households = await LoadUserHouseholdsAsync(user);
        await _userSessionService.ChangeHousehold(currentHousehold, households, budgets);

        return true;
    }

    public async Task<bool> CreateHouseholdAsync(string householdName)
    {
        try
        {
            Dictionary<string, object> parameters = new()
            {
                { "p_username", _userSessionService.GetCurrentUser().Username },
                { "p_householdname", householdName }
            };

            var response = await _supabaseClient.Rpc("create_household", parameters);

            if (response.Content != null && response.Content.Trim() == "true")
            {
                User? user = await _supabaseClient.From<User>().Where(x => x.Username == _userSessionService.GetCurrentUser().Username).Single();
                if (user == null)
                    return false;

                Household? currentHousehold = await _supabaseClient.From<Household>().Where(x => x.HHID == user.CurrentHousehold).Single();
                if (currentHousehold == null)
                    return false;

                List<Household> households = await LoadUserHouseholdsAsync(user);
                await _userSessionService.ChangeHousehold(currentHousehold, households, []);

                return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> JoinHouseholdAsync(string householdAccess)
    {
        Household? household = await _supabaseClient.From<Household>().Where(x => x.HouseholdAccess == householdAccess).Single();

        if (household == null)
            return false;

        User? user = await _supabaseClient.From<User>().Where(x => x.Username == _userSessionService.GetCurrentUser().Username).Single();

        if (user == null)
            return false;

        // Már tagja-e a felhasználó a háztartásnak:
        if (user.HouseHold1 == household.HHID || user.HouseHold2 == household.HHID || user.HouseHold3 == household.HHID)
            return false;

        // Van-e szabad háztartás hely a felhasználónak:
        if (user.HouseHold1 == null)
        {
            user.HouseHold1 = household.HHID;
            user.CurrentHousehold = household.HHID;
            await user.Update<User>();

            List<Household> households = await LoadUserHouseholdsAsync(user);
            await _userSessionService.ChangeHousehold(household, households, []);

            return true;
        }
        else if (user.HouseHold2 == null)
        {
            user.HouseHold2 = household.HHID;
            user.CurrentHousehold = household.HHID;
            await user.Update<User>();

            List<Household> households = await LoadUserHouseholdsAsync(user);
            await _userSessionService.ChangeHousehold(household, households, []);

            return true;
        }
        else if (user.HouseHold3 == null)
        {
            user.HouseHold3 = household.HHID;
            user.CurrentHousehold = household.HHID;
            await user.Update<User>();

            List<Household> households = await LoadUserHouseholdsAsync(user);
            await _userSessionService.ChangeHousehold(household, households, []);

            return true;
        }

        return false;
    }

    public async Task<bool> ChangeSelectedHouseholdAsync(string currentHousehold)
    {
        Household? household = await _supabaseClient.From<Household>().Where(x => x.HouseholdAccess == currentHousehold).Single();
        if (household == null)
            return false;

        User? user = await _supabaseClient.From<User>().Where(x => x.Username == _userSessionService.GetCurrentUser().Username).Single();
        if (user == null)
            return false;

        user.CurrentHousehold = household.HHID;
        await user.Update<User>();

        // Jelenlegi háztartásban elérhető költségkeretek:
        List<Budget> budgets = [];
        var budgetquery = await _supabaseClient.From<Budget>().Where(x => x.BudgetHousehold == household.HHID).Get();
        foreach (Budget budget in budgetquery.Models)
            budgets.Add(budget);

        List<Household> households = await LoadUserHouseholdsAsync(user);
        await _userSessionService.ChangeHousehold(household, households, budgets);

        return true;
    }

    public async Task<bool> EditHouseholdAsync(string newHouseholdName, DateTime householdBudgetMonth, int newHouseholdBudget)
    {
        string currentDate = $"{householdBudgetMonth.Year}. {householdBudgetMonth.Month}.";

        Household? household = await _supabaseClient.From<Household>().Where(x => /*x.HouseholdAccess == CurrentHouseholdAccess*/ x.HHID == _userSessionService.GetCurrentHousehold().HHID).Single();
        if (household == null)
            return false;

        // Háztartáshoz tartozó összes költségkeret lekérése:
        Budget? budget = _userSessionService.GetCurrentBudgets().Find(x => $"{x.BudgetDate.Year}. {x.BudgetDate.Month}." == currentDate);
        if (budget == null)
        {
            // Ha nem talált meglévő hónapot, akkor készít egy új rekordot a "budgets" táblázatba
            DateTime newDate = new(householdBudgetMonth.Year, householdBudgetMonth.Month, 1);
            await _supabaseClient.From<Budget>().Insert(new Budget() { BudgetHousehold = household.HHID, BudgetDate = newDate, BudgetAmount = newHouseholdBudget });
        }
        else
        {
            if (budget.BudgetAmount != newHouseholdBudget)
            {
                budget.BudgetAmount = newHouseholdBudget;
                await budget.Update<Budget>();
            }
        }

        if (household.HouseholdName != newHouseholdName)
        {
            household.HouseholdName = newHouseholdName;
            await household.Update<Household>();
        }
        
        return true;
    }

    public async Task<bool> LeaveHouseholdAsync()
    {
        User? user = await _supabaseClient.From<User>().Where(x => x.UID == _userSessionService.GetCurrentUser().UID).Single();
        if (user == null)
            return false;

        Household? household = await _supabaseClient.From<Household>().Where(x => x.HHID == _userSessionService.GetCurrentHousehold().HHID).Single();
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

        // Ha a jelenlegi háztartás tulajdonosa, akkor megszünteti a háztartást is.
        if (_userSessionService.IsCurrentHouseholdOwner())
        {
            household.HouseholdOwner = null;
            await household.Update<Household>();
            await household.Delete<Household>();
        }

        List<Household> households = await LoadUserHouseholdsAsync(user);
        await _userSessionService.ChangeHousehold(household, households, []);

        return true;
    }

    private async Task<List<Household>> LoadUserHouseholdsAsync(User user)
    {
        List<Household> householdList = [];
        if (user.HouseHold1 != null)
        {
            Household? hh1 = await _supabaseClient.From<Household>().Where(x => x.HHID == user.HouseHold1).Single();
            if (hh1 != null)
                householdList.Add(hh1);
        }
        if (user.HouseHold2 != null)
        {
            Household? hh2 = await _supabaseClient.From<Household>().Where(x => x.HHID == user.HouseHold2).Single();
            if (hh2 != null)
                householdList.Add(hh2);
        }
        if (user.HouseHold3 != null)
        {
            Household? hh3 = await _supabaseClient.From<Household>().Where(x => x.HHID == user.HouseHold3).Single();
            if (hh3 != null)
                householdList.Add(hh3);
        }

        return householdList;
    }
}