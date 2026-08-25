using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Services;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class UserViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IUserSessionService _userSessionService;

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

    public RelayCommand ToggleMenuButtonCommand { get; }
    public RelayCommand NavigateSettingsButtonCommand { get; }
    public RelayCommand NavigateEventLogButtonCommand { get; }
    public RelayCommand LogoutButtonCommand { get; }

    public UserViewModel(IAuthService authService, IUserSessionService userSessionService)
    {
        _authService = authService;
        _userSessionService = userSessionService;

        ToggleMenuButtonCommand = new RelayCommand(async _ => await Task.Run(() => IsMenuVisible = !IsMenuVisible));
        NavigateSettingsButtonCommand = new RelayCommand(async _ => await NavigateToSettingsAsync());
        NavigateEventLogButtonCommand = new RelayCommand(async _ => await NavigateToEventLogAsync());
        LogoutButtonCommand = new RelayCommand(async _ => await LogoutAsync());

        WelcomeMessage = $"Üdv, {_userSessionService.GetCurrentUser().Nickname}";
    }

    private async Task NavigateToSettingsAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            await DisplayErrorAsync("A beállításokhoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
        }

        IsMenuVisible = false;
        await Shell.Current.GoToAsync("//SettingsPage?test=asd123");
    }

    private async Task NavigateToEventLogAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            await DisplayErrorAsync("Az eseménynaplóhoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        IsMenuVisible = false;
        await Shell.Current.GoToAsync("//EventLogPage");
    }

    private async Task LogoutAsync()
    {
        IsMenuVisible = false;
        WelcomeMessage = string.Empty;
        await _authService.LogoutAsync();
    }

    private async Task DisplayErrorAsync(string message)
    {
        ErrorMessage = message;
        if (Shell.Current != null)
            await Shell.Current.DisplayAlert("Hiba", ErrorMessage, "OK");
    }
}