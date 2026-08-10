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
    public RelayCommand LogoutButtonCommand { get; }

    public UserViewModel(IAuthService authService)
    {
        _authService = authService;

        ToggleMenuButtonCommand = new RelayCommand(async _ => await Task.Run(() => IsMenuVisible = !IsMenuVisible));
        NavigateSettingsButtonCommand = new RelayCommand(async _ => await NavigateToSettingsAsync());
        LogoutButtonCommand = new RelayCommand(async _ => await LogoutAsync());

        WelcomeMessage = $"Üdv, {_authService.Nickname}";
    }

    private async Task NavigateToSettingsAsync()
    {
        try
        {
            if (_authService.Nickname == string.Empty)
                throw new InvalidOperationException("Nickname is empty. Cannot navigate to settings.");

            IsMenuVisible = false;
            await Shell.Current.GoToAsync("//SettingsPage");
        }
        catch (InvalidOperationException)
        {
            _ = Shell.Current.DisplayAlert("Hiba", "A beállításokhoz való navigálás sikertelen. Kérlek, jelentkezz be újra.", "OK");
            await LogoutAsync();
        }
    }

    private async Task LogoutAsync()
    {
        IsMenuVisible = false;
        WelcomeMessage = string.Empty;
        await _authService.LogoutAsync();
    }
}
