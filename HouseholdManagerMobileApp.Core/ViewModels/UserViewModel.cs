using System.Collections.ObjectModel;
using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Helpers;
using HouseholdManagerMobileApp.Core.Models;
using HouseholdManagerMobileApp.Core.Services;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class UserViewModel : ViewModelBase
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

    private ObservableCollection<Notification> unreadNotifications = [];
    public ObservableCollection<Notification> UnreadNotifications
    {
        get => unreadNotifications;
        set
        {
            if (unreadNotifications == value) return;
            unreadNotifications = value;
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

    private string currentMonth = DateTime.Today.ToString("MMMM");
    public string CurrentMonth
    {
        get => currentMonth;
        set
        {
            if (currentMonth == value) return;
            currentMonth = value;
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

    private DateTime startOfWeek;
    public DateTime StartOfWeek
    {
        get => startOfWeek;
        set
        {
            if (startOfWeek == value) return;
            startOfWeek = value;
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
    public RelayCommand NavigateNotificationsButtonCommand { get; }
    public RelayCommand ToggleEventExpandCommand { get; }
    public RelayCommand MondayEventsButtonCommand { get; }
    public RelayCommand TuesdayEventsButtonCommand { get; }
    public RelayCommand WednesdayEventsButtonCommand { get; }
    public RelayCommand ThursdayEventsButtonCommand { get; }
    public RelayCommand FridayEventsButtonCommand { get; }
    public RelayCommand SaturdayEventsButtonCommand { get; }
    public RelayCommand SundayEventsButtonCommand { get; }
    public RelayCommand LogoutButtonCommand { get; }

    public UserViewModel(IAuthService authService, IUserSessionService userSessionService, INotificationService notificationService)
    {
        _authService = authService;
        _userSessionService = userSessionService;
        _notificationService = notificationService;

        ToggleMenuButtonCommand = new RelayCommand(async _ => await Task.Run(() => IsMenuVisible = !IsMenuVisible));
        NavigateSettingsButtonCommand = new RelayCommand(async _ => await NavigateToSettingsAsync());
        NavigateEventLogButtonCommand = new RelayCommand(async _ => await NavigateToEventLogAsync());
        NavigateNotificationsButtonCommand = new RelayCommand(async _ => await NavigateToNotificationsAsync());
        ToggleEventExpandCommand = new RelayCommand(async parameter => await ToggleEventExpandAsync(parameter));
        MondayEventsButtonCommand = new RelayCommand(async _ => await LoadSelectedDayEventsAsync(0));
        TuesdayEventsButtonCommand = new RelayCommand(async _ => await LoadSelectedDayEventsAsync(1));
        WednesdayEventsButtonCommand = new RelayCommand(async _ => await LoadSelectedDayEventsAsync(2));
        ThursdayEventsButtonCommand = new RelayCommand(async _ => await LoadSelectedDayEventsAsync(3));
        FridayEventsButtonCommand = new RelayCommand(async _ => await LoadSelectedDayEventsAsync(4));
        SaturdayEventsButtonCommand = new RelayCommand(async _ => await LoadSelectedDayEventsAsync(5));
        SundayEventsButtonCommand = new RelayCommand(async _ => await LoadSelectedDayEventsAsync(6));
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
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("A beállításokhoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
        }

        IsMenuVisible = false;
        await _notificationService.MarkAllNotificationsAsReadAsync();
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
        await _notificationService.MarkAllNotificationsAsReadAsync();
        await Shell.Current.GoToAsync("//EventLogPage");
    }

    private async Task NavigateToNotificationsAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az értesítésekhez való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        IsMenuVisible = false;
        await _notificationService.MarkAllNotificationsAsReadAsync();
        await Shell.Current.GoToAsync("//NotificationsPage");
    }

    private async Task LoadUserPageAsync()
    {
        // Olvasatlan értesítések betöltése:
        UnreadNotifications.Clear();
        List<Notification> unreadNotifications = await _notificationService.GetUnreadNotificationsAsync();
        foreach (Notification notification in unreadNotifications)
            UnreadNotifications.Add(notification);

        // Jelenlegi hét dátumának betöltése:
        DateTime today = DateTime.Today;
        int currentDayIndex = (int)today.DayOfWeek;
        int daysToMonday = currentDayIndex == 0 ? 6 : currentDayIndex - 1;

        StartOfWeek = today.AddDays(-daysToMonday); // Hétfő
        DateTime endOfWeek = StartOfWeek.AddDays(6); // Vasárnap

        CurrentWeekLabel = $"{StartOfWeek:MM.dd.} - {endOfWeek:MM.dd.}";

        // A hét eseményeinek betöltése:
        await LoadSelectedDayEventsAsync(currentDayIndex - 1);

        // Események betöltése a havi költségkeret kiszámolásához:
        string currentMonth = DateTime.Today.ToString("yyyy. MMMM");
        Budget? currentMonthBudget = _userSessionService.GetCurrentBudgets().Find(x => x.BudgetDate.ToString("yyyy. MMMM") == currentMonth);
        if (currentMonthBudget != null)
        {
            int currentMonthEventCosts = 0;
            foreach (Event _event in _userSessionService.GetCurrentEventLog())
            {
                if (_event.EventUserDate.ToString("yyyy. MMMM") == currentMonth)
                    currentMonthEventCosts += _event.EventCost;
            }

            CurrentMonthBudget = $"{currentMonthBudget.BudgetAmount} Ft";
            CurrentMonthUseableBudget = $"{currentMonthBudget.BudgetAmount - currentMonthEventCosts} Ft";
        }
        else
            CurrentMonthBudget = CurrentMonthUseableBudget = "- Ft";
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

    private async Task LoadSelectedDayEventsAsync(int dayNum)
    {
        EventLog.Clear();
        List<Event> sortedEventLog = _userSessionService.GetCurrentEventLog().Where(x => x.EventUserDate.ToString("MM. dd.") == StartOfWeek.AddDays(dayNum).ToString("MM. dd.")).OrderByDescending(x => x.EventUserDate).ToList();
        foreach (Event _event in sortedEventLog)
            EventLog.Add(_event);

        await Task.CompletedTask;
    }

    private async Task LogoutAsync()
    {
        IsMenuVisible = false;
        WelcomeMessage = string.Empty;
        await _notificationService.MarkAllNotificationsAsReadAsync();
        await _authService.LogoutAsync();
    }
}