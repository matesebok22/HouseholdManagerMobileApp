using System.Collections.ObjectModel;

using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Helpers;
using HouseholdManagerMobileApp.Core.Models;
using HouseholdManagerMobileApp.Core.Services;
using Microsoft.IdentityModel.Tokens;

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

            NavigateNewEventButtonCommand.RefreshCanExecute();
            NavigateAnalyticsButtonCommand.RefreshCanExecute();
            ChangeSortTypeButtonCommand.RefreshCanExecute();
        }
    }

    private bool activeHousehold = false;
    public bool ActiveHousehold
    {
        get => activeHousehold;
        set
        {
            if (activeHousehold == value) return;
            activeHousehold = value;
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

    private string analyticsButtonName = "Hónap/Év statisztikák";
    public string AnalyticsButtonName
    {
        get => analyticsButtonName;
        set
        {
            if (analyticsButtonName == value) return;
            analyticsButtonName = value;
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
    public RelayCommand NavigateNotificationsButtonCommand { get; }
    public RelayCommand NavigateSettingsButtonCommand { get; }
    public RelayCommand NavigateNewEventButtonCommand { get; }
    public RelayCommand NavigateAnalyticsButtonCommand { get; }
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
        NavigateNotificationsButtonCommand = new RelayCommand(async _ => await NavigateToNotificationsAsync());
        NavigateSettingsButtonCommand = new RelayCommand(async _ => await NavigateToSettingsAsync());
        NavigateNewEventButtonCommand = new RelayCommand(async _ => await NavigateToNewEventAsync(), _ => !IsLoading);
        NavigateAnalyticsButtonCommand = new RelayCommand(async _ => await NavigatoToAnalyticsAsync(), _ => !IsLoading);
        ToggleEventExpandCommand = new RelayCommand(async parameter => await ToggleEventExpandAsync(parameter));
        EditEventButtonCommand = new RelayCommand(async _ => await EditEventAsync());
        DeleteEventButtonCommand = new RelayCommand(async _ => await DeleteEventAsync());
        ChangeSortTypeButtonCommand = new RelayCommand(async _ => await ChangeSortTypeAsync(), _ => !IsLoading);
        PreviousMonthButtonCommand = new RelayCommand(async _ => await SelectTypeAsync(-1));
        NextMonthButtonCommand = new RelayCommand(async _ => await SelectTypeAsync(1));
        LogoutButtonCommand = new RelayCommand(async _ => await LogoutAsync());

        if (Shell.Current != null)
            _ = LoadEventlogAsync();
    }

    private async Task NavigateToHomeAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("A kezdőlaphoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        _userSessionService.SetSelectedEvent(null);

        await Shell.Current.GoToAsync("//UserPage");
    }

    private async Task NavigateToNotificationsAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az értesítésekhez való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        _userSessionService.SetSelectedEvent(null);

        IsMenuVisible = false;
        await Shell.Current.GoToAsync("//NotificationsPage");
    }

    private async Task NavigateToSettingsAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("A beállításokhoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        _userSessionService.SetSelectedEvent(null);

        await Shell.Current.GoToAsync("//SettingsPage");
    }

    private async Task NavigateToNewEventAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az új esemény létrehozásához való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        _userSessionService.SetSelectedEvent(null);

        await Shell.Current.GoToAsync($"//NewEventPage");
    }

    private async Task NavigatoToAnalyticsAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az új esemény létrehozásához való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        _userSessionService.SetSelectedEvent(null);

        if (AnalyticsButtonName.Contains("Hónap/Év statisztikák"))
            await Shell.Current.GoToAsync($"//AnalyticsPage?isDate=1&category={SelectedTypeName}");
        else
            await Shell.Current.GoToAsync($"//AnalyticsPage?isDate=0&category={SelectedTypeName}");
    }

    private async Task LoadEventlogAsync()
    {
        IsLoading = true;

        if (_userSessionService.IsOfflineMode())
            WelcomeMessage = $"{_userSessionService.GetCurrentUser()!.Nickname} (Offline)";
        else
            WelcomeMessage = _userSessionService.GetCurrentUser()!.Nickname;

        if (_userSessionService.GetCurrentHousehold() != null)
            CurrentHouseholdName = _userSessionService.GetCurrentHousehold()!.HouseholdName;
        else
            CurrentHouseholdName = "Nem vagy háztartásban";

        ActiveHousehold = _userSessionService.GetCurrentHousehold() != null;

        EventTypes.Clear();

        bool success = await _databaseService.LoadEventlogPageAsync();
        if (success)
            await LoadMonthAsync();

        Event? currentlySelected = _userSessionService.GetSelectedEvent();
        if (currentlySelected != null)
            ExpandedEventId = currentlySelected.EID;

        IsLoading = false;
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
                    if (!_event.EventType.Trim().IsNullOrEmpty() && !EventTypes.Contains(_event.EventType))
                        EventTypes.Add(_event.EventType);
                }

                SelectedCategory = EventTypes[0];
            }

            // Kategória szerinti rendszerezés:
            foreach (Event _event in sortedEventLog)
            {
                if (_event.EventType == SelectedCategory || (_event.EventType.Trim().IsNullOrEmpty() && SelectedCategory == "Kategória nélküli"))
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
            int currentMonthEventCosts = 0;

            foreach (Event _event in sortedEventLog)
            {
                if (_event.EventUserDate.ToString("yyyy. MMMM") == selectedMonth)
                {
                    EventLog.Add(_event);
                    currentMonthEventCosts += _event.EventCost;
                }
            }

            Budget? currentMonthBudget = _userSessionService.GetCurrentBudgets().Find(x => x.BudgetDate.ToString("yyyy. MMMM") == selectedMonth);
            if (currentMonthBudget != null)
            {
                SelectedMonthBudget = $"{currentMonthBudget.BudgetAmount} Ft";
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
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az esemény módosításához való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        if (Shell.Current != null)
            await Shell.Current.GoToAsync($"//NewEventPage");
    }

    private async Task DeleteEventAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az esemény törlése sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        if (_userSessionService.IsOfflineMode())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Offline módban nem lehetséges az esemény törlése.");
            return;
        }

        if (Shell.Current != null)
        {
            if (await Shell.Current.DisplayAlert("Alert", $"Biztosan törli a(z) {_userSessionService.GetSelectedEvent()!.EventName} eseményt?", "Igen", "Nem"))
            {
                await _databaseService.DeleteEventAsync();
                await LoadEventlogAsync();
            }
        }
    }

    private async Task ChangeSortTypeAsync()
    {
        ChangeSortTypeName = ChangeSortTypeName == "Kategória nézet" ? "Dátum nézet" : "Kategória nézet";
        AnalyticsButtonName = ChangeSortTypeName == "Kategória nézet" ? "Kategória statisztikák" : "Hónap/Év statisztikák";

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
        WelcomeMessage = string.Empty;
        IsLoading = false;
        await _authService.LogoutAsync();
    }
}