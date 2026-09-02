using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public interface INotificationService
{
    // Betölti az olvasatlan értesítéseket az aktuális felhasználó számára.
    Task<List<Notification>> GetUnreadNotificationsAsync();

    // Megjelöl egy értesítést olvasottként.
    Task<bool> MarkNotificationAsReadAsync(string notificationId);

    // Feliratkozik a valós idejű értesítésekre a Supabase Realtime-on keresztül.
    Task SubscribeToNotificationsAsync(Action<Notification> onNotificationReceived);

    // Leiratkozik a valós idejű értesítésekről.
    Task UnsubscribeFromNotificationsAsync();

    // Lekéri az összes értesítést egy felhasználónak az adott háztartásban.
    Task<List<Notification>> GetNotificationsByHouseholdAsync(string householdId);

    // Összes értesítést megjelöl olvasottként egy felhasználónak.
    Task<bool> MarkAllNotificationsAsReadAsync();
}