using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HouseholdManagerMobileApp.Core.Models;

[Table("households")]
public class Household : BaseModel
{
    [PrimaryKey("hhid")]
    public string HHID { get; set; } = string.Empty;

    [Column("hh_name")]
    public string HouseholdName { get; set; } = string.Empty;

    [Column("hh_owner")]
    public string HouseholdOwner {  get; set; } = string.Empty;

    [Column("hh_access")]
    public string HouseholdAccess {  get; set; } = string.Empty;
}