using SupabaseDB = Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using SQLite;

namespace HouseholdManagerMobileApp.Core.Models;

// Supabase online adatbázis Budget osztálya:
[SupabaseDB.Table("budgets")]
public class Budget : BaseModel
{
    [SupabaseDB.PrimaryKey("bid")]
    public string BID { get; set; } = string.Empty;

    [SupabaseDB.Column("b_hh")]
    public string BudgetHousehold { get; set; } = string.Empty;

    [SupabaseDB.Column("b_userdate")]
    public DateTime BudgetDate { get; set; }

    [SupabaseDB.Column("b_amount")]
    public int BudgetAmount { get; set; }
}

// SQLite offline adatbázis Budget osztálya:
[Table("offline_budgets")]
public class OfflineBudget
{
    [PrimaryKey, Column("bid")]
    public string BID { get; set; } = string.Empty;

    [Column("b_hh")]
    public string BudgetHousehold { get; set; } = string.Empty;

    [Column("b_userdate")]
    public DateTime BudgetDate { get; set; }

    [Column("b_amount")]
    public int BudgetAmount { get; set; }
}