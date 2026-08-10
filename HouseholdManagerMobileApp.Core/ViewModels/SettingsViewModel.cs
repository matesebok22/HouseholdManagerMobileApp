using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Services;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class SettingsViewModel : ViewModelBase
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

    private string oldPassword = string.Empty;
    public string OldPassword
    {
        get => oldPassword;
        set
        {
            if (oldPassword == value) return;
            oldPassword = value;
            OnPropertyChanged();
        }
    }

    private string newPassword = string.Empty;
    public string NewPassword
    {
        get => newPassword;
        set
        {
            if (newPassword == value) return;
            newPassword = value;
            OnPropertyChanged();
        }
    }

    private string confirmNewPassword = string.Empty;
    public string ConfirmNewPassword
    {
        get => confirmNewPassword;
        set
        {
            if (confirmNewPassword == value) return;
            confirmNewPassword = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand ToggleMenuButtonCommand { get; }
    public RelayCommand NavigateHomeButtonCommand { get; }
    public RelayCommand ChangePasswordButtonCommand { get; }
    public RelayCommand LogoutButtonCommand { get; }

    public SettingsViewModel(IAuthService authService)
    {
        _authService = authService;

        ToggleMenuButtonCommand = new RelayCommand(async _ => await Task.Run(() => IsMenuVisible = !IsMenuVisible));
        NavigateHomeButtonCommand = new RelayCommand(async _ => await NavigateToHomeAsync());
        ChangePasswordButtonCommand = new RelayCommand(async _ => await ChangePasswordAsync());
        LogoutButtonCommand = new RelayCommand(async _ => await LogoutAsync());

        WelcomeMessage = $"Üdv, {_authService.Nickname}";
    }

    private async Task NavigateToHomeAsync()
    {
        try
        {
            if (_authService.CurrentUsername == string.Empty)
                throw new InvalidNavigationException("Username is empty. Cannot navigate to home.");

            IsMenuVisible = false;
            await Shell.Current.GoToAsync("//UserPage");
        }
        catch (InvalidNavigationException)
        {
            _ = Shell.Current.DisplayAlert("Hiba", "A kezdőlaphoz való navigálás sikertelen. Kérlek, jelentkezz be újra.", "OK");
            await LogoutAsync();
        }
    }
    
    private async Task ChangePasswordAsync()
    {
        try
        {
            if (_authService.CurrentUsername == string.Empty)
                throw new InvalidNavigationException("Username is empty. Cannot change password.");

            if (string.IsNullOrEmpty(OldPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmNewPassword))
            {
                _ = Shell.Current.DisplayAlert("Hiba", "Minden jelszó mezőt ki kell tölteni.", "OK");
                throw new InvalidOperationException();
            }

            if (OldPassword == NewPassword || OldPassword == ConfirmNewPassword)
            {
                _ = Shell.Current.DisplayAlert("Hiba", "A régi jelszó nem egyezhet az új jelszóval.", "OK");
                throw new InvalidOperationException();
            }

            if (NewPassword != ConfirmNewPassword)
            {
                _ = Shell.Current.DisplayAlert("Hiba", "Az új jelszavak nem egyeznek.", "OK");
                throw new InvalidOperationException();
            }

            bool success = await _authService.ChangePasswordAsync(OldPassword, NewPassword);

            if (success)
            {
                _ = Shell.Current.DisplayAlert("Siker", "A jelszó sikeresen megváltozott.", "OK");
                OldPassword = NewPassword = ConfirmNewPassword = string.Empty;
            }
            else
                _ = Shell.Current.DisplayAlert("Hiba", "A régi és a jelenlegi jelszó nem egyezik!", "OK");
        }
        catch (InvalidNavigationException)
        {
            _ = Shell.Current.DisplayAlert("Hiba", "A jelszó megváltoztatása sikertelen. Kérlek, jelentkezz be újra.", "OK");
            await LogoutAsync();
        }
        catch (InvalidOperationException)
        {
            return;
        }
    }

    private async Task LogoutAsync()
    {
        IsMenuVisible = false;
        WelcomeMessage = string.Empty;
        await _authService.LogoutAsync();
    }
}
