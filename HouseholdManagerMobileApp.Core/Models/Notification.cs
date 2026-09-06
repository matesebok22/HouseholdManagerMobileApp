using SupabaseDB = Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using SQLite;

namespace HouseholdManagerMobileApp.Core.Models;

// Supabase online adatbázis Notification osztálya:
[SupabaseDB.Table("notifications")]
public class Notification : BaseModel
{
    [SupabaseDB.PrimaryKey("nid")]
    public string NID { get; set; } = string.Empty;

    [SupabaseDB.Column("recipient_user_id")]
    public string RecipientUserId { get; set; } = string.Empty;

    [SupabaseDB.Column("event_id")]
    public string EventId { get; set; } = string.Empty;

    [SupabaseDB.Column("notification_type")]
    public string NotificationType { get; set; } = string.Empty;

    [SupabaseDB.Column("title")]
    public string Title { get; set; } = string.Empty;

    [SupabaseDB.Column("message")]
    public string Message { get; set; } = string.Empty;

    [SupabaseDB.Column("related_household_id")]
    public string RelatedHouseholdId { get; set; } = string.Empty;

    [SupabaseDB.Column("is_read")]
    public bool IsRead { get; set; } = false;

    [SupabaseDB.Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [SupabaseDB.Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

// SQLite offline adatbázis Notification osztálya:
[Table("offline_notifications")]
public class OfflineNotification
{
    [PrimaryKey, Column("nid")]
    public string NID { get; set; } = string.Empty;

    [Column("recipient_user_id")]
    public string RecipientUserId { get; set; } = string.Empty;

    [Column("event_id")]
    public string EventId { get; set; } = string.Empty;

    [Column("notification_type")]
    public string NotificationType { get; set; } = string.Empty;

    [Column("title")]
    public string Title { get; set; } = string.Empty;

    [Column("message")]
    public string Message { get; set; } = string.Empty;

    [Column("related_household_id")]
    public string RelatedHouseholdId { get; set; } = string.Empty;

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}