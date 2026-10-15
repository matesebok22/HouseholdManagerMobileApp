using SQLite;

using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public class NotificationService : INotificationService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly SQLiteAsyncConnection _sqliteDb;
    private readonly IUserSessionService _userSessionService;

    public NotificationService(Supabase.Client supabaseClient, SQLiteAsyncConnection sqliteDb, IUserSessionService userSessionService)
    {
        _supabaseClient = supabaseClient;
        _sqliteDb = sqliteDb;
        _userSessionService = userSessionService;
    }

    public async Task<List<Notification>> GetUnreadNotificationsAsync()
    {
        try
        {
            string currentUserId = _userSessionService.GetCurrentUser()!.UID;
            List<string> disabledHouseholds = await GetDisabledNotificationHouseholdsAsync();
            List<Notification> notifications = [];

            if (!_userSessionService.IsOfflineMode())
            {
                var response = await _supabaseClient.From<Notification>().Where(x => x.RecipientUserId == currentUserId).Where(x => !x.IsRead).Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending).Get();
                notifications = response.Models ?? [];
            }
            else
            {
                var response = await _sqliteDb.Table<OfflineNotification>().Where(x => x.RecipientUserId == currentUserId && x.IsRead == false).OrderByDescending(x => x.CreatedAt).ToListAsync();
                notifications = await TransformToNotificationAsync(response) ?? [];
            }

            // Letiltott háztartások szűrése:
            if (disabledHouseholds.Count > 0)
                notifications = notifications.Where(n => !disabledHouseholds.Contains(n.RelatedHouseholdId)).ToList();

            return notifications;
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

    public async Task<List<Notification>> GetAllNotificationsAsync()
    {
        try
        {
            string currentUserId = _userSessionService.GetCurrentUser()!.UID;
            List<string> disabledHouseholds = await GetDisabledNotificationHouseholdsAsync();
            List<Notification> notifications = [];

            if (!_userSessionService.IsOfflineMode())
            {
                var response = await _supabaseClient.From<Notification>().Where(x => x.RecipientUserId == currentUserId).Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending).Get();
                notifications = response.Models ?? [];
            }
            else
            {
                var response = await _sqliteDb.Table<OfflineNotification>().Where(x => x.RecipientUserId == currentUserId).OrderByDescending(x => x.CreatedAt).ToListAsync();
                notifications = await TransformToNotificationAsync(response) ?? [];
            }

            // Letiltott háztartások szűrése:
            if (disabledHouseholds.Count > 0)
                notifications = notifications.Where(x => !disabledHouseholds.Contains(x.RelatedHouseholdId)).ToList();

            return notifications;
        }
        catch (Exception ex)
        {
            throw new Exception($"Hiba történt az értesítések betöltése során!", ex);
        }
    }

    public async Task<bool> MarkAllNotificationsAsReadAsync()
    {
        try
        {
            var currentUserId = _userSessionService.GetCurrentUser()!.UID;

            if (!_userSessionService.IsOfflineMode())
            {
                var response = await _supabaseClient.From<Notification>().Where(x => x.RecipientUserId == currentUserId).Where(x => x.IsRead == false).Set(x => x.IsRead, true).Update();

                string sqliteQuery = @"UPDATE offline_notifications SET is_read = 1 WHERE recipient_user_id = ? AND is_read = 0";
                await _sqliteDb.ExecuteAsync(sqliteQuery, currentUserId);

                return response.Models != null;
            }
            else
            {
                string sqliteQuery = @"UPDATE offline_notifications SET is_read = 1 WHERE recipient_user_id = ? AND is_read = 0";
                int response = await _sqliteDb.ExecuteAsync(sqliteQuery, currentUserId);
                return response > 0;
            }
            
        }
        catch (Exception ex)
        {
            throw new Exception("Hiba történt az összes értesítés megjelölése során.", ex);
        }
    }

    public async Task DeleteAllNotificationsAsync()
    {
        try
        {
            string currentUserId = _userSessionService.GetCurrentUser()!.UID;
            await _supabaseClient.From<Notification>().Where(x => x.RecipientUserId == currentUserId).Delete();
        }
        catch (Exception ex)
        {
            throw new Exception($"Hiba történt az értesítések törlése során!", ex);
        }
    }

    private async Task<List<string>> GetDisabledNotificationHouseholdsAsync()
    {
        List<string> disabledHouseholds = [];
        User currentUser = _userSessionService.GetCurrentUser()!;

        string[] householdIds = [currentUser.HouseHold1, currentUser.HouseHold2, currentUser.HouseHold3];
        foreach (string householdId in householdIds)
        {
            string value = await SecureStorage.GetAsync($"notifications-{currentUser.UID}-{householdId}") ?? "";

            if (!string.IsNullOrEmpty(value) && !bool.Parse(value))
                disabledHouseholds.Add(householdId);
        }

        return disabledHouseholds;
    }

    private async Task<List<Notification>> TransformToNotificationAsync(List<OfflineNotification> offlineNotifications)
    {
        List<Notification> notifications = [];
        foreach (var offlineNotification in offlineNotifications)
        {
            notifications.Add(new Notification
            {
                NID = offlineNotification.NID,
                RecipientUserId = offlineNotification.RecipientUserId,
                EventId = offlineNotification.EventId,
                NotificationType = offlineNotification.NotificationType,
                Title = offlineNotification.Title,
                Message = offlineNotification.Message,
                RelatedHouseholdId = offlineNotification.RelatedHouseholdId,
                IsRead = offlineNotification.IsRead,
                CreatedAt = offlineNotification.CreatedAt,
                UpdatedAt = offlineNotification.UpdatedAt
            });
        }

        return await Task.FromResult(notifications);
    }
}