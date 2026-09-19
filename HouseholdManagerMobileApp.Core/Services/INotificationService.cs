using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.Services;

public interface INotificationService
{
    Task<List<Notification>> GetUnreadNotificationsAsync(); // Betölti az olvasatlan értesítéseket a jelenlegi felhasználó számára.
    Task<bool> MarkNotificationAsReadAsync(string notificationId); // Megjelöl egy értesítést olvasottként.
    Task<List<Notification>> GetAllNotificationsAsync(); // Lekéri az összes értesítést a jelenlegi felhasználónak.
    Task<bool> MarkAllNotificationsAsReadAsync(); // Összes értesítést megjelöl olvasottként a jelenlegi felhasználónak.
    Task DeleteAllNotificationsAsync(); // Törli a jelenlegi felhasználó összes értesítését.
}