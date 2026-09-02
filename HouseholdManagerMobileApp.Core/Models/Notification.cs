using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace HouseholdManagerMobileApp.Core.Models;

[Table("notifications")]
public class Notification : BaseModel
{
    [PrimaryKey("nid")]
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