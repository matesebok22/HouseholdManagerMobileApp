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
            var response = await _supabaseClient
                .From<Notification>()
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
            var response = await _supabaseClient
                .From<Notification>()
                .Where(x => x.RecipientUserId == currentUserId)
                .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Descending)
                .Get();

            return response.Models ?? new List<Notification>();
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
            await _supabaseClient
                .From<Notification>()
                .Where(x => x.RecipientUserId == currentUserId)
                .Delete();
        }
        catch (Exception ex)
        {
            throw new Exception($"Hiba történt az értesítések törlése során!", ex);
        }
    }
}