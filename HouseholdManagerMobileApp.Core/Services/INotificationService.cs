using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public interface INotificationService
{
    // Betölti az olvasatlan értesítéseket a jelenlegi felhasználó számára.
    Task<List<Notification>> GetUnreadNotificationsAsync();

    // Megjelöl egy értesítést olvasottként.
    Task<bool> MarkNotificationAsReadAsync(string notificationId);

    // Lekéri az összes értesítést a jelenlegi felhasználónak.
    Task<List<Notification>> GetAllNotificationsAsync();

    // Összes értesítést megjelöl olvasottként a jelenlegi felhasználónak.
    Task<bool> MarkAllNotificationsAsReadAsync();

    // Törli a jelenlegi felhasználó összes értesítését.
    Task DeleteAllNotificationsAsync();
}