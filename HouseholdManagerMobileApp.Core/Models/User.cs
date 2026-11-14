using Newtonsoft.Json;
using SupabaseDB = Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using SQLite;

namespace HouseholdManagerMobileApp.Core.Models;

// Supabase online adatbázis User osztálya:
[SupabaseDB.Table("users")]
public class User : BaseModel
{
    [SupabaseDB.PrimaryKey("uid")]
    [JsonProperty("uid")]
    public string UID { get; set; } = string.Empty;

    [SupabaseDB.Column("u_name")]
    [JsonProperty("u_name")]
    public string Username { get; set; } = string.Empty;

    [SupabaseDB.Column("u_currenthh")]
    [JsonProperty("u_currenthh")]
    public string CurrentHousehold { get; set; } = string.Empty;

    [SupabaseDB.Column("hh_1")]
    [JsonProperty("hh_1")]
    public string HouseHold1 { get; set; } = string.Empty;

    [SupabaseDB.Column("hh_2")]
    [JsonProperty("hh_2")]
    public string HouseHold2 { get; set; } = string.Empty;

    [SupabaseDB.Column("hh_3")]
    [JsonProperty("hh_3")]
    public string HouseHold3 { get; set; } = string.Empty;

    [SupabaseDB.Column("u_nick")]
    [JsonProperty("u_nick")]
    public string Nickname { get; set; } = string.Empty;
}

// SQLite offline adatbázis User osztálya:
[Table("offline_users")]    
public class OfflineUser
{
    [PrimaryKey, Column("uid")]
    public string UID { get; set; } = string.Empty;

    [Column("u_name")]
    public string Username { get; set; } = string.Empty;

    [Column("u_currenthh")]
    public string CurrentHousehold { get; set; } = string.Empty;

    [Column("hh_1")]
    public string HouseHold1 { get; set; } = string.Empty;

    [Column("hh_2")]
    public string HouseHold2 { get; set; } = string.Empty;

    [Column("hh_3")]
    public string HouseHold3 { get; set; } = string.Empty;

    [Column("u_nick")]
    public string Nickname { get; set; } = string.Empty;
}