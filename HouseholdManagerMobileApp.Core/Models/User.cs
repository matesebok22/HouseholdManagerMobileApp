using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HouseholdManagerMobileApp.Core.Models;

[Table("users")]
public class User : BaseModel
{
    [PrimaryKey("uid")]
    public string UID { get; set; } = string.Empty;

    [Column("u_name")]
    public string Username { get; set; } = string.Empty;

    [Column("u_pass")]
    public string Password { get; set; } = string.Empty;

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
