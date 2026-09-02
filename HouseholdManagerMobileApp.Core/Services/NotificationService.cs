using HouseholdManagerMobileApp.Core.Models;
using Supabase.Realtime;
using Supabase.Realtime.PostgresChanges;

namespace HouseholdManagerMobileApp.Core.Services;

public class NotificationService : INotificationService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly IUserSessionService _userSessionService;
    private RealtimeChannel? _notificationChannel;

    public NotificationService(Supabase.Client supabaseClient, IUserSessionService userSessionService)
    {
        _supabaseClient = supabaseClient;
        _userSessionService = userSessionService;
    }

    public async Task<List<Notification>> GetUnreadNotificationsAsync()
    {
        try
        {
            string currentUserId = _userSessionService.GetCurrentUser().UID;
            var response = await _supabaseClient.From<Notification>()
                .Where(x => x.RecipientUserId == currentUserId)
                .Where(x => !x.IsRead)
                .Order(x => x.UpdatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();
            
            return response.Models ?? [];
        }
        catch (Exception ex)
        {
            throw new Exception("Hiba történt az olvasatlan értesítések betöltése során.", ex);
        }
    }

    public async Task<bool> MarkNotificationAsReadAsync(string notificationId)
    {
        try
        {
            var response = await _supabaseClient.From<Notification>().Where(x => x.NID == notificationId).Set(x => x.IsRead, true).Update();

            return response.Models?.Count > 0;
        }
        catch (Exception ex)
        {
            throw new Exception($"Hiba történt az értesítés olvasottként jelölése során: {notificationId}", ex);
        }
    }

    public async Task SubscribeToNotificationsAsync(Action<Notification> onNotificationReceived)
    {
        try
        {
            var currentUserId = _userSessionService.GetCurrentUser().UID;

            // Csatorna példányosítása
            _notificationChannel = _supabaseClient.Realtime.Channel("notifications-channel");

            // 1. Paraméterek átadása közvetlenül a konstruktorban
            var options = new PostgresChangesOptions(
                schema: "public",
                table: "notifications",
                eventType: PostgresChangesOptions.ListenType.Inserts,
                filter: $"recipient_user_id=eq.{currentUserId}"
            );

            _notificationChannel.Register(options);

            // 2. Az AddPostgresChangeHandler metódus használatával lehet feliratkozni az értesítésekre
            _notificationChannel.AddPostgresChangeHandler(PostgresChangesOptions.ListenType.Inserts, (sender, change) =>
            {
                try
                {
                    // A C# SDK-ban a change.Model<T>() segítségével nyerjük ki a konvertált objektumot
                    var notification = change.Model<Notification>();
                    if (notification != null)
                    {
                        onNotificationReceived?.Invoke(notification);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Hiba az értesítés feldolgozása során: {ex.Message}");
                }
            });

            // 3. Feliratkozás indítása
            await _notificationChannel.Subscribe();
        }
        catch (Exception ex)
        {
            throw new Exception("Hiba történt a valós idejű értesítésekre való feliratkozás során.", ex);
        }
    }

    public async Task UnsubscribeFromNotificationsAsync()
    {
        try
        {
            if (_notificationChannel != null)
            {
                // 1. Szabályos leiratkozás a csatornáról (szinkron)
                _notificationChannel.Unsubscribe();

                // 2. Csatorna eltávolítása a kliensből (szinkron)
                _supabaseClient.Realtime.Remove(_notificationChannel);
                _notificationChannel = null;
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new Exception("Hiba történt a valós idejű értesítésekről való leiratkozás során.", ex);
        }
    }

    public async Task<List<Notification>> GetNotificationsByHouseholdAsync(string householdId)
    {
        try
        {
            var currentUserId = _userSessionService.GetCurrentUser().UID;

            var response = await _supabaseClient
                .From<Notification>()
                .Where(x => x.RecipientUserId == currentUserId)
                .Where(x => x.RelatedHouseholdId == householdId)
                .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            return response.Models ?? new List<Notification>();
        }
        catch (Exception ex)
        {
            throw new Exception($"Hiba történt az értesítések betöltése során a háztartáshoz: {householdId}", ex);
        }
    }

    public async Task<bool> MarkAllNotificationsAsReadAsync()
    {
        try
        {
            var currentUserId = _userSessionService.GetCurrentUser().UID;

            var response = await _supabaseClient
                .From<Notification>()
                .Where(x => x.RecipientUserId == currentUserId)
                .Where(x => x.IsRead == false)
                .Set(x => x.IsRead, true)
                .Update();

            // Ha a válasz sikeres, a Models nem null
            return response.Models != null;
        }
        catch (Exception ex)
        {
            throw new Exception("Hiba történt az összes értesítés megjelölése során.", ex);
        }
    }
}