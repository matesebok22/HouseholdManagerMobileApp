using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Models;
using HouseholdManagerMobileApp.Core.Services;

namespace HouseholdManagerMobileApp.Core.ViewModels;

// TODO:
// - Nap gombjainak működését implementálni.
// - Amikor az adott hét 2 hónapban van (pl. 08.31 - 09.06), akkor a gombok a megfelelő hónap költségvetését mutassák.

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

    private string currentMonthBudget = string.Empty;
    public string CurrentMonthBudget
    {
        get => currentMonthBudget;
        set
        {
            if (currentMonthBudget == value) return;
            currentMonthBudget = value;
            OnPropertyChanged();
        }
    }

    private string currentMonthUseableBudget = string.Empty;
    public string CurrentMonthUseableBudget
    {
        get => currentMonthUseableBudget;
        set
        {
            if (currentMonthUseableBudget == value) return;
            currentMonthUseableBudget = value;
            OnPropertyChanged();
        }
    }

    private string currentWeekLabel = string.Empty;
    public string CurrentWeekLabel
    {
        get => currentWeekLabel;
        set
        {
            if (currentWeekLabel == value) return;
            currentWeekLabel = value;
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
    public RelayCommand NavigateSettingsButtonCommand { get; }
    public RelayCommand NavigateEventLogButtonCommand { get; }
    public RelayCommand ToggleEventExpandCommand { get; }
    public RelayCommand LogoutButtonCommand { get; }

    public UserViewModel(IAuthService authService, IUserSessionService userSessionService)
    {
        _authService = authService;
        _userSessionService = userSessionService;

        ToggleMenuButtonCommand = new RelayCommand(async _ => await Task.Run(() => IsMenuVisible = !IsMenuVisible));
        NavigateSettingsButtonCommand = new RelayCommand(async _ => await NavigateToSettingsAsync());
        NavigateEventLogButtonCommand = new RelayCommand(async _ => await NavigateToEventLogAsync());
        ToggleEventExpandCommand = new RelayCommand(async parameter => await ToggleEventExpandAsync(parameter));
        LogoutButtonCommand = new RelayCommand(async _ => await LogoutAsync());

        if (Shell.Current != null)
        {
            WelcomeMessage = $"Üdv, {_userSessionService.GetCurrentUser().Nickname}";

            _ = LoadUserPageAsync();
        }
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

    private async Task LoadUserPageAsync()
    {
        // Jelenlegi hét dátumának betöltése:
        DateTime today = DateTime.Today;
        int currentDayIndex = (int)today.DayOfWeek;
        int daysToMonday = currentDayIndex == 0 ? 6 : currentDayIndex - 1;

        DateTime startOfWeek = today.AddDays(-daysToMonday); // Hétfő
        DateTime endOfWeek = startOfWeek.AddDays(6); // Vasárnap

        CurrentWeekLabel = $"{startOfWeek:MM.dd.} - {endOfWeek:MM.dd.}";

        // Események betöltése:
        EventLog.Clear();
        List<Event> sortedEventLog = _userSessionService.GetCurrentEventLog().OrderByDescending(x => x.EventUserDate).ToList();

        // Egyelőre a jelenlegi hónap megjelenítése (WIP):
        string currentMonth = DateTime.Today.ToString("yyyy. MMMM");
        int currentMonthEventCosts = 0;

        foreach (Event _event in sortedEventLog)
        {
            if (_event.EventUserDate.ToString("yyyy. MMMM") == currentMonth)
            {
                EventLog.Add(_event);
                currentMonthEventCosts += _event.EventCost;
            }
        }

        Budget? currentMonthBudget = _userSessionService.GetCurrentBudgets().Find(x => x.BudgetDate.ToString("yyyy. MMMM") == currentMonth);
        if (currentMonthBudget != null)
        {
            CurrentMonthBudget = $"{currentMonthBudget.BudgetAmount} Ft";
            CurrentMonthUseableBudget = $"{currentMonthBudget.BudgetAmount - currentMonthEventCosts} Ft";
        }
        else
            CurrentMonthBudget = CurrentMonthUseableBudget = "- Ft";

        await Task.CompletedTask;
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