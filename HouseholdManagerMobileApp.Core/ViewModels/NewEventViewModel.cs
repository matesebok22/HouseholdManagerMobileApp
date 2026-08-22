using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Services;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class NewEventViewModel : ViewModelBase
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

    public RelayCommand NavigateEventLogButtonCommand { get; }
    //public RelayCommand CreateEventButtonCommand { get; }

    public NewEventViewModel(IAuthService authService)
    {
        _authService = authService;

        NavigateEventLogButtonCommand = new RelayCommand(async _ => await NavigateToEventLogAsync());
        //CreateEventButtonCommand = new RelayCommand(async _ => await CreateEventAsync());

        WelcomeMessage = $"Üdv, {_authService.Nickname}";
    }

    private async Task NavigateToEventLogAsync()
    {
        if (_authService.CurrentUsername == string.Empty)
        {
            await DisplayErrorAsync("Az eseménynaplóhoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        await Shell.Current.GoToAsync("//EventLogPage");
    }

    private async Task LogoutAsync()
    {
        WelcomeMessage = ErrorMessage = string.Empty;
        await _authService.LogoutAsync();
    }

    private async Task DisplayErrorAsync(string message)
    {
        ErrorMessage = message;
        if (Shell.Current != null)
            await Shell.Current.DisplayAlert("Hiba", ErrorMessage, "OK");
    }
}