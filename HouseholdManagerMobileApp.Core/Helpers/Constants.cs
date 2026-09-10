namespace HouseholdManagerMobileApp.Core.Helpers;

public static class Constants
{
    // Supabase környezet konfigurációs adatok:
    public const string SupabaseUrl = "https://juldhlhcetwmonjpdinq.supabase.co";
    public const string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imp1bGRobGhjZXR3bW9uanBkaW5xIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NzQ3MTY0MDMsImV4cCI6MjA5MDI5MjQwM30.Rf-n5iuI3NyPGaAWFGWbnKEMrEHdT8lckHUdR8XXiW0";

    // SQLite környezet konfigurációs adatok:
    public static string SqliteDbPath => Path.Combine(FileSystem.AppDataDirectory, "HHMMApp.db3");

    // Egyéb konstansok:
    private const long TicksPerSec = 10000000L;
    public const long OfflineLoginValidity = 86400L * TicksPerSec; // tickben (24 óra)
}
