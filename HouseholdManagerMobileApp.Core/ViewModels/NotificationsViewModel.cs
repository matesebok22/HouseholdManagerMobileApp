using System.Collections.ObjectModel;

using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Helpers;
using HouseholdManagerMobileApp.Core.Models;
using HouseholdManagerMobileApp.Core.Services;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class NotificationsViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IUserSessionService _userSessionService;
    private readonly INotificationService _notificationService;

    private string welcomeMessage = string.Empty;
    public string WelcomeMessage
    {
        get => welcomeMessage;
        set
        {
            if (welcomeMessage == value) return;
            welcomeMessage = value;
            OnPropertyChanged();
        }
    }

    private string errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => errorMessage;
        set
        {
            if (errorMessage == value) return;
            errorMessage = value;
            OnPropertyChanged();
        }
    }

    private bool isMenuVisible = false;
    public bool IsMenuVisible
    {
        get => isMenuVisible;
        set
        {
            if (isMenuVisible == value) return;
            isMenuVisible = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<Notification> notifications = [];
    public ObservableCollection<Notification> Notifications
    {
        get => notifications;
        set
        {
            if (notifications == value) return;
            notifications = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand ToggleMenuButtonCommand { get; }
    public RelayCommand NavigateHomeButtonCommand { get; }
    public RelayCommand NavigateSettingsButtonCommand { get; }
    public RelayCommand NavigateEventLogButtonCommand { get; }
    public RelayCommand DeleteAllNotificationsButtonCommand { get; }
    public RelayCommand LogoutButtonCommand { get; }

    public NotificationsViewModel(IAuthService authService, IUserSessionService userSessionService, INotificationService notificationService)
    {
        _authService = authService;
        _userSessionService = userSessionService;
        _notificationService = notificationService;

        ToggleMenuButtonCommand = new RelayCommand(async _ => await Task.Run(() => IsMenuVisible = !IsMenuVisible));
        NavigateHomeButtonCommand = new RelayCommand(async _ => await NavigateToHomeAsync());
        NavigateSettingsButtonCommand = new RelayCommand(async _ => await NavigateToSettingsAsync());
        NavigateEventLogButtonCommand = new RelayCommand(async _ => await NavigateToEventLogAsync());
        DeleteAllNotificationsButtonCommand = new RelayCommand(async _ => await DeleteAllNotificationsAsync());
        LogoutButtonCommand = new RelayCommand(async _ => await LogoutAsync());

        if (Shell.Current != null)
        {
            if (_userSessionService.IsOfflineMode())
                WelcomeMessage = $"{_userSessionService.GetCurrentUser()!.Nickname} (Offline)";
            else
                WelcomeMessage = _userSessionService.GetCurrentUser()!.Nickname;
        }

        _ = LoadNotificationsPageAsync();
    }

    private async Task NavigateToHomeAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("A kezdőlaphoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        IsMenuVisible = false;
        await Shell.Current.GoToAsync("//UserPage");
    }

    private async Task NavigateToSettingsAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("A beállításokhoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        await Shell.Current.GoToAsync("//SettingsPage?test=asd123");
    }

    private async Task NavigateToEventLogAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az eseménynaplóhoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        IsMenuVisible = false;
        await Shell.Current.GoToAsync("//EventLogPage");
    }

    public async Task LoadNotificationsPageAsync()
    {
        Notifications.Clear();
        foreach (Notification notification in await _notificationService.GetAllNotificationsAsync())
            Notifications.Add(notification);
    }

    private async Task DeleteAllNotificationsAsync()
    {
        if (_userSessionService.IsOfflineMode())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Offline módban nem elérhető az értesítések törlése.");
            return;
        }

        if (Shell.Current != null)
        {
            if (await Shell.Current.DisplayAlert("Megerősítés", "Biztosan törölni szeretnéd az összes értesítést?", "Igen", "Nem"))
                await _notificationService.DeleteAllNotificationsAsync();
            else
                return;
        }

        Notifications.Clear();
    }

    private async Task LogoutAsync()
    {
        IsMenuVisible = false;
        WelcomeMessage = string.Empty;
        await _authService.LogoutAsync();
    }
}