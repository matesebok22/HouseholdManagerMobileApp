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
}
