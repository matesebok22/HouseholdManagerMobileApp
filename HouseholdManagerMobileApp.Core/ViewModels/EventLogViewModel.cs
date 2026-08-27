using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Models;
using HouseholdManagerMobileApp.Core.Services;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class EventLogViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IDatabaseService _databaseService;
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

    private string currentHouseholdName = string.Empty;
    public string CurrentHouseholdName
    {
        get => currentHouseholdName;
        set
        {
            if (currentHouseholdName == value) return;
            currentHouseholdName = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<Event> eventLog = [];
    public ObservableCollection<Event> EventLog
    {
        get => eventLog;
        set
        {
            if (eventLog == value) return;
            eventLog = value;
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

    private string expandedEventId = string.Empty;
    public string ExpandedEventId
    {
        get => expandedEventId;
        set
        {
            if (expandedEventId == value) return;
            expandedEventId = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand ToggleMenuButtonCommand { get; }
    public RelayCommand NavigateHomeButtonCommand { get; }
    public RelayCommand NavigateSettingsButtonCommand { get; }
    public RelayCommand NavigateNewEventButtonCommand { get; }
    public RelayCommand ToggleEventExpandCommand { get; }
    public RelayCommand EditEventButtonCommand { get; }
    public RelayCommand DeleteEventButtonCommand { get; }
    public RelayCommand LogoutButtonCommand { get; }

    public EventLogViewModel(IAuthService authService, IDatabaseService databaseService, IUserSessionService userSessionService)
    {
        _authService = authService;
        _databaseService = databaseService;
        _userSessionService = userSessionService;

        ToggleMenuButtonCommand = new RelayCommand(async _ => await Task.Run(() => IsMenuVisible = !IsMenuVisible));
        NavigateHomeButtonCommand = new RelayCommand(async _ => await NavigateToHomeAsync());
        NavigateSettingsButtonCommand = new RelayCommand(async _ => await NavigateToSettingsAsync());
        NavigateNewEventButtonCommand = new RelayCommand(async _ => await NavigateToNewEventAsync());
        ToggleEventExpandCommand = new RelayCommand(async parameter => await ToggleEventExpandAsync(parameter));
        EditEventButtonCommand = new RelayCommand(async _ => await EditEventAsync());
        DeleteEventButtonCommand = new RelayCommand(async _ => await DeleteEventAsync());
        LogoutButtonCommand = new RelayCommand(async _ => await LogoutAsync());

        WelcomeMessage = $"Üdv, {_userSessionService.GetCurrentUser().Nickname}";
        CurrentHouseholdName = _userSessionService.GetCurrentHousehold().HouseholdName;
        _ = LoadEventlogAsync();
    }

    private async Task NavigateToHomeAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            await DisplayErrorAsync("A főoldalhoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        _userSessionService.SetSelectedEvent(null); // Még lehet mozgatni fogom.

        await Shell.Current.GoToAsync("//UserPage");
    }

    private async Task NavigateToSettingsAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            await DisplayErrorAsync("A beállításokhoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        _userSessionService.SetSelectedEvent(null); // Még lehet mozgatni fogom.

        await Shell.Current.GoToAsync("//SettingsPage?test=asd123");
    }

    private async Task NavigateToNewEventAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            await DisplayErrorAsync("Az új esemény létrehozásához való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        await Shell.Current.GoToAsync("//NewEventPage");
    }

    private async Task LoadEventlogAsync()
    {
        EventLog.Clear();

        bool success = await _databaseService.LoadHouseholdsAsync();
        if (success)
        {
            foreach (Event _event in _userSessionService.GetCurrentEventLog())
                EventLog.Add(_event);
        }
    }

    private async Task EditEventAsync()
    {
        //await DisplayErrorAsync($"Módosításra nyomtál: {_userSessionService.GetSelectedEvent().EventName}");

        if (!_userSessionService.IsUserLoggedIn())
        {
            await DisplayErrorAsync("Az esemény módosításához való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        await Shell.Current.GoToAsync("//NewEventPage");
    }

    private async Task DeleteEventAsync()
    {
        //await DisplayErrorAsync($"Törlésre nyomtál: {_userSessionService.GetSelectedEvent().EventName}");

        if (!_userSessionService.IsUserLoggedIn())
        {
            await DisplayErrorAsync("Az esemény törlése sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        if (Shell.Current != null)
        {
            if (await Shell.Current.DisplayAlert("Alert", $"Biztosan törli a(z) {_userSessionService.GetSelectedEvent().EventName} eseményt?", "Igen", "Nem"))
            {
                await _databaseService.DeleteEventAsync();
                await LoadEventlogAsync();
            }
        }
    }

    private async Task ToggleEventExpandAsync(object parameter)
    {
        if (parameter is Event selectedEvent)
        {
            if (ExpandedEventId == selectedEvent.EID)
            {
                ExpandedEventId = string.Empty;
                _userSessionService.SetSelectedEvent(null);
            }
            else
            {
                ExpandedEventId = selectedEvent.EID;
                _userSessionService.SetSelectedEvent(selectedEvent);
            }
        }
        await Task.CompletedTask;
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