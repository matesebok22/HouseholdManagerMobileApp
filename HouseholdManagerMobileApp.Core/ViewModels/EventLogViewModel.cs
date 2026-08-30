using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Models;
using HouseholdManagerMobileApp.Core.Services;
using Microsoft.IdentityModel.Tokens;
using static Microsoft.IO.RecyclableMemoryStreamManager;

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

    private bool isLoading = false;
    public bool IsLoading
    {
        get => isLoading;
        set
        {
            if (isLoading == value) return;
            isLoading = value;
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

    private ObservableCollection<string> eventTypes = [];
    public ObservableCollection<string> EventTypes
    {
        get => eventTypes;
        set
        {
            if (eventTypes == value) return;
            eventTypes = value;
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

    private string changeSortTypeName = "Dátum nézet";
    public string ChangeSortTypeName
    {
        get => changeSortTypeName;
        set
        {
            if (changeSortTypeName == value) return;
            changeSortTypeName = value;
            OnPropertyChanged();
        }
    }

    private string selectedCategory = string.Empty;
    public string SelectedCategory
    {
        get => selectedCategory;
        set
        {
            if (selectedCategory == value) return;
            selectedCategory = value;
            OnPropertyChanged();
        }
    }

    private DateTime selectedMonth = DateTime.Today;
    public DateTime SelectedMonth
    {
        get => selectedMonth;
        set
        {
            if (selectedMonth == value) return;
            selectedMonth = value;
            OnPropertyChanged();
        }
    }

    private string selectedTypeName = string.Empty;
    public string SelectedTypeName
    {
        get => selectedTypeName;
        set
        {
            if (selectedTypeName == value) return;
            selectedTypeName = value;
            OnPropertyChanged();
        }
    }

    private string selectedMonthBudget = string.Empty;
    public string SelectedMonthBudget
    {
        get => selectedMonthBudget;
        set
        {
            if (selectedMonthBudget == value) return;
            selectedMonthBudget = value;
            OnPropertyChanged();
        }
    }

    private string selectedMonthUseableBudget = string.Empty;
    public string SelectedMonthUseableBudget
    {
        get => selectedMonthUseableBudget;
        set
        {
            if (selectedMonthUseableBudget == value) return;
            selectedMonthUseableBudget = value;
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
    public RelayCommand ChangeSortTypeButtonCommand { get; }
    public RelayCommand PreviousMonthButtonCommand { get; }
    public RelayCommand NextMonthButtonCommand { get; }
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
        ChangeSortTypeButtonCommand = new RelayCommand(async _ => await ChangeSortTypeAsync());
        PreviousMonthButtonCommand = new RelayCommand(async _ => await SelectTypeAsync(-1));
        NextMonthButtonCommand = new RelayCommand(async _ => await SelectTypeAsync(1));
        LogoutButtonCommand = new RelayCommand(async _ => await LogoutAsync());

        if (Shell.Current != null)
        {
            WelcomeMessage = $"Üdv, {_userSessionService.GetCurrentUser().Nickname}";
            CurrentHouseholdName = _userSessionService.GetCurrentHousehold().HouseholdName;

            _ = LoadEventlogAsync();
        }
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
        EventTypes.Clear();

        bool success = await _databaseService.LoadHouseholdsAsync();
        if (success)
            await LoadMonthAsync();

        Event? currentlySelected = _userSessionService.GetSelectedEvent();
        if (currentlySelected != null)
            ExpandedEventId = currentlySelected.EID;
    }

    private async Task LoadMonthAsync()
    {
        EventLog.Clear();
        List<Event> sortedEventLog = _userSessionService.GetCurrentEventLog().OrderByDescending(x => x.EventUserDate).ToList();

        if (ChangeSortTypeName == "Kategória nézet")
        {
            if (EventTypes.Count == 0)
            {
                EventTypes.Add("Kategória nélküli");

                foreach (Event _event in sortedEventLog)
                {
                    if (!EventTypes.Contains(_event.EventType))
                        EventTypes.Add(_event.EventType);
                }

                SelectedCategory = EventTypes[0];
            }

            // Kategória szerinti rendszerezés
            foreach (Event _event in sortedEventLog)
            {
                if (_event.EventType == SelectedCategory)
                    EventLog.Add(_event);
            }

            SelectedTypeName = SelectedCategory;
            SelectedMonthBudget = SelectedMonthUseableBudget = "- Ft";
        }
        else
        {
            // Hónap szerinti rendszerezés:
            string selectedMonth = SelectedMonth.ToString("yyyy. MMMM");
            SelectedTypeName = selectedMonth;

            foreach (Event _event in sortedEventLog)
            {
                if (_event.EventUserDate.ToString("yyyy. MMMM") == selectedMonth)
                    EventLog.Add(_event);
            }

            Budget? currentMonthBudget = _userSessionService.GetCurrentBudgets().Find(x => x.BudgetDate.ToString("yyyy. MMMM") == selectedMonth);
            if (currentMonthBudget != null)
            {
                SelectedMonthBudget = $"{currentMonthBudget.BudgetAmount} Ft";

                int currentMonthEventCosts = _userSessionService.GetCurrentEventLog().Where(x => x.EventUserDate.ToString("yyyy. MMMM") == selectedMonth).Sum(x => x.EventCost);

                SelectedMonthUseableBudget = $"{currentMonthBudget.BudgetAmount - currentMonthEventCosts} Ft";
            }
            else
                SelectedMonthBudget = SelectedMonthUseableBudget = "- Ft";
        }

        await Task.CompletedTask;
    }

    private async Task EditEventAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            await DisplayErrorAsync("Az esemény módosításához való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        if (Shell.Current != null)
            await Shell.Current.GoToAsync("//NewEventPage");
    }

    private async Task DeleteEventAsync()
    {
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

    private async Task ChangeSortTypeAsync()
    {
        ChangeSortTypeName = ChangeSortTypeName == "Kategória nézet" ? "Dátum nézet" : "Kategória nézet";

        if (Shell.Current != null)
            await LoadMonthAsync();
    }

    private async Task SelectTypeAsync(int num)
    {
        if (ChangeSortTypeName == "Kategória nézet")
        {
            int index = EventTypes.IndexOf(SelectedTypeName);
            if (index == 0 && num == -1)
                SelectedCategory = EventTypes[^1]; // Ugyanaz, mint a EventTypes[EventTypes.Count - 1]
            else if (index == EventTypes.Count - 1 && num == 1)
                SelectedCategory = EventTypes[0];
            else
                SelectedCategory = EventTypes[index + num];
        }
        else
            SelectedMonth = SelectedMonth.AddMonths(num);

        if (Shell.Current != null)
            await LoadMonthAsync();
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
        IsLoading = false;
        await _authService.LogoutAsync();
    }

    private async Task DisplayErrorAsync(string message)
    {
        ErrorMessage = message;
        if (Shell.Current != null)
            await Shell.Current.DisplayAlert("Hiba", ErrorMessage, "OK");
    }
}