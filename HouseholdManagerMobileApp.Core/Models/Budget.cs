using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HouseholdManagerMobileApp.Core.Models;

[Table("budgets")]
public class Budget : BaseModel
{
    [PrimaryKey("bid")]
    public string BID { get; set; } = string.Empty;

    [Column("b_hh")]
    public string BudgetHousehold { get; set; } = string.Empty;

    [Column("b_userdate")]
    public DateTime BudgetDate { get; set; }

    [Column("b_amount")]
    public int BudgetAmount { get; set; }
}