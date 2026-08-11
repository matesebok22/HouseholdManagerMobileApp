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

    private bool isJoinHouseholdVisible = false;
    public bool IsJoinHouseholdVisible
    {
        get => isJoinHouseholdVisible;
        set
        {
            if (isJoinHouseholdVisible == value) return;
            isJoinHouseholdVisible = value;
            OnPropertyChanged();
        }
    }

    private bool isCreateHouseholdVisible = false;
    public bool IsCreateHouseholdVisible
    {
        get => isCreateHouseholdVisible;
        set
        {
            if (isCreateHouseholdVisible == value) return;
            isCreateHouseholdVisible = value;
            OnPropertyChanged();
        }
    }

    private bool isEditHouseholdVisible = false;
    public bool IsEditHouseholdVisible
    {
        get => isEditHouseholdVisible;
        set
        {
            if (isEditHouseholdVisible == value) return;
            isEditHouseholdVisible = value;
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
    public RelayCommand ShowJoinHouseholdButtonCommand { get; }
    public RelayCommand ShowCreateHouseholdButtonCommand { get; }
    public RelayCommand ShowEditHouseholdButtonCommand { get; }
    public RelayCommand ChangePasswordButtonCommand { get; }
    public RelayCommand LogoutButtonCommand { get; }

    public SettingsViewModel(IAuthService authService)
    {
        _authService = authService;

        ToggleMenuButtonCommand = new RelayCommand(async _ => await Task.Run(() => IsMenuVisible = !IsMenuVisible));
        NavigateHomeButtonCommand = new RelayCommand(async _ => await NavigateToHomeAsync());
        ShowJoinHouseholdButtonCommand = new RelayCommand(async _ => await Task.Run(() => ChangeVisibility(1, IsJoinHouseholdVisible)));
        ShowCreateHouseholdButtonCommand = new RelayCommand(async _ => await Task.Run(() => ChangeVisibility(2, IsCreateHouseholdVisible)));
        ShowEditHouseholdButtonCommand = new RelayCommand(async _ => await Task.Run(() => ChangeVisibility(3, IsEditHouseholdVisible)));
        ChangePasswordButtonCommand = new RelayCommand(async _ => await ChangePasswordAsync());
        LogoutButtonCommand = new RelayCommand(async _ => await LogoutAsync());

        WelcomeMessage = $"Üdv, {_authService.Nickname}";
    }

    private async Task NavigateToHomeAsync()
    {
        if (_authService.CurrentUsername == string.Empty)
        {
            await DisplayErrorAsync("A kezdőlaphoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
        }

        IsMenuVisible = false;
        await Shell.Current.GoToAsync("//UserPage");
    }
    
    private async Task ChangePasswordAsync()
    {
        if (_authService.CurrentUsername == string.Empty)
        {
            await DisplayErrorAsync("A jelszó megváltoztatása sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        if (string.IsNullOrEmpty(OldPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmNewPassword))
        {
            await DisplayErrorAsync("Minden jelszó mezőt ki kell tölteni.");
            return;
        }

        if (OldPassword == NewPassword || OldPassword == ConfirmNewPassword)
        {
            await DisplayErrorAsync("A régi jelszó nem egyezhet az új jelszóval.");
            return;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            await DisplayErrorAsync("Az új jelszavak nem egyeznek.");
            return;
        }

        bool success = await _authService.ChangePasswordAsync(OldPassword, NewPassword);

        if (success)
        {
            if (Shell.Current != null)
                _ = Shell.Current.DisplayAlert("Siker", "A jelszó sikeresen megváltozott.", "OK");
            OldPassword = NewPassword = ConfirmNewPassword = string.Empty;
        }
        else
            await DisplayErrorAsync("A régi és a jelenlegi jelszó nem egyezik!");
    }

    private async Task LogoutAsync()
    {
        IsMenuVisible = false;
        WelcomeMessage = ErrorMessage = string.Empty;
        await _authService.LogoutAsync();
    }

    private async Task DisplayErrorAsync(string message)
    {
        ErrorMessage = message;
        if (Shell.Current != null)
            await Shell.Current.DisplayAlert("Hiba", ErrorMessage, "OK");
    }

    private void ChangeVisibility(int optionNum, bool param)
    {
        bool isVisible = param;
        IsJoinHouseholdVisible = IsCreateHouseholdVisible = IsEditHouseholdVisible = false;
        switch (optionNum)
        {
            case 1: IsJoinHouseholdVisible = !isVisible; break;
            case 2: IsCreateHouseholdVisible = !isVisible; break;
            case 3: IsEditHouseholdVisible = !isVisible; break;
        }
    }
}