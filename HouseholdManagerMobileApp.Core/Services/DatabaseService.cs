using Supabase;

namespace HouseholdManagerMobileApp.Core.Services;

public class DatabaseService : IDatabaseService
{
    private readonly Client _supabaseClient;

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
}