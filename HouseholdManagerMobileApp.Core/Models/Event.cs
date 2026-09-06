using SupabaseDB = Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using SQLite;

namespace HouseholdManagerMobileApp.Core.Models;

// Supabase online adatbázis Event osztálya:
[SupabaseDB.Table("events")]
public class Event : BaseModel
{
    [SupabaseDB.PrimaryKey("eid")]
    public string EID { get; set; } = string.Empty;

    [SupabaseDB.Column("e_user")]
    public string EventUser { get; set; } = string.Empty;

    [SupabaseDB.Column("e_hh")]
    public string EventHousehold { get; set; } = string.Empty;

    [SupabaseDB.Column("e_date")]
    public DateTime EventDate { get; set; } = DateTime.Now;

    [SupabaseDB.Column("e_name")]
    public string EventName { get; set; } = string.Empty;

    [SupabaseDB.Column("e_userdate")]
    public DateTime EventUserDate { get; set; }

    [SupabaseDB.Column("e_type")]
    public string EventType { get; set; } = string.Empty;

    [SupabaseDB.Column("e_cost")]
    public int EventCost { get; set; } = 0;

    [SupabaseDB.Column("e_desc")]
    public string EventDescription { get; set; } = string.Empty;

    [SupabaseDB.Column("last_modified_by")]
    public string LastModifiedBy { get; set; } = string.Empty;
}

// SQLite offline adatbázis Event osztálya:
[Table("offline_events")]
public class OfflineEvent
{
    [PrimaryKey, Column("eid")]
    public string EID { get; set; } = string.Empty;

    [Column("e_user")]
    public string EventUser { get; set; } = string.Empty;

    [Column("e_hh")]
    public string EventHousehold { get; set; } = string.Empty;

    [Column("e_date")]
    public DateTime EventDate { get; set; } = DateTime.Now;

    [Column("e_name")]
    public string EventName { get; set; } = string.Empty;

    [Column("e_userdate")]
    public DateTime EventUserDate { get; set; }

    [Column("e_type")]
    public string EventType { get; set; } = string.Empty;

    [Column("e_cost")]
    public int EventCost { get; set; } = 0;

    [Column("e_desc")]
    public string EventDescription { get; set; } = string.Empty;

    [Column("last_modified_by")]
    public string LastModifiedBy { get; set; } = string.Empty;

    [Column("e_made_offline")]
    public bool EventMadeOffline { get; set; } = false;
}
