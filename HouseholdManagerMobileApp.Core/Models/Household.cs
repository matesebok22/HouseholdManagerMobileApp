using SupabaseDB = Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using SQLite;

namespace HouseholdManagerMobileApp.Core.Models;

// Supabase online adatbázis Household osztálya:
[SupabaseDB.Table("households")]
public class Household : BaseModel
{
    [SupabaseDB.PrimaryKey("hhid")]
    public string HHID { get; set; } = string.Empty;

    [SupabaseDB.Column("hh_name")]
    public string HouseholdName { get; set; } = string.Empty;

    [SupabaseDB.Column("hh_owner")]
    public string HouseholdOwner { get; set; } = string.Empty;

    [SupabaseDB.Column("hh_access")]
    public string HouseholdAccess { get; set; } = string.Empty;
}

// SQLite offline adatbázis Household osztálya:
[Table("offline_households")]
public class OfflineHousehold
{
    [PrimaryKey, Column("hhid")]
    public string HHID { get; set; } = string.Empty;

    [Column("hh_name")]
    public string HouseholdName { get; set; } = string.Empty;

    [Column("hh_owner")]
    public string HouseholdOwner { get; set; } = string.Empty;

    [Column("hh_access")]
    public string HouseholdAccess { get; set; } = string.Empty;
}