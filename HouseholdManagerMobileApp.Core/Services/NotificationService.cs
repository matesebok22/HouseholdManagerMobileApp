using HouseholdManagerMobileApp.Core.Models;
using Supabase.Realtime;

namespace HouseholdManagerMobileApp.Core.Services;

public class NotificationService : INotificationService
{
    private readonly Supabase.Client _supabaseClient;
    private readonly IUserSessionService _userSessionService;

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
            List<string> disabledHouseholds = await GetDisabledNotificationHouseholdsAsync();

            var response = await _supabaseClient.From<Notification>().Where(x => x.RecipientUserId == currentUserId).Where(x => !x.IsRead).Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending).Get();

            var notifications = response.Models ?? [];

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

    /*public async Task<List<Notification>> GetNotificationsByHouseholdAsync(string householdId)
    {
        try
        {
            string currentUserId = _userSessionService.GetCurrentUser().UID;
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
    }*/

    public async Task<List<Notification>> GetAllNotificationsAsync()
    {
        try
        {
            string currentUserId = _userSessionService.GetCurrentUser().UID;
            List<string> disabledHouseholds = await GetDisabledNotificationHouseholdsAsync();

            var response = await _supabaseClient.From<Notification>().Where(x => x.RecipientUserId == currentUserId).Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending).Get();

            var notifications = response.Models ?? [];

            // Letiltott háztartások szűrése:
            if (disabledHouseholds.Count > 0)
                notifications = notifications.Where(n => !disabledHouseholds.Contains(n.RelatedHouseholdId)).ToList();

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

    public async Task DeleteAllNotificationsAsync()
    {
        try
        {
            string currentUserId = _userSessionService.GetCurrentUser().UID;
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
        User currentUser = _userSessionService.GetCurrentUser();

        List<string> householdIds = [];
        householdIds.Add(currentUser.HouseHold1);
        householdIds.Add(currentUser.HouseHold2);
        householdIds.Add(currentUser.HouseHold3);
        foreach (string householdId in householdIds)
        {
            string value = await SecureStorage.GetAsync($"notifications-{currentUser.UID}-{householdId}");

            if (!string.IsNullOrEmpty(value) && !bool.Parse(value))
                disabledHouseholds.Add(householdId);
        }

        return disabledHouseholds;
    }
}