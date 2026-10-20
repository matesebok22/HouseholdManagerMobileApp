using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.Models;
using HouseholdManagerMobileApp.Core.Helpers;
using System.ComponentModel.Design;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class NewEventViewModel : ViewModelBase
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

    private string eventName = string.Empty;
    public string EventName
    {
        get => eventName;
        set
        {
            if (eventName == value) return;
            eventName = value;
            OnPropertyChanged();
        }
    }

    private DateTime eventDate = DateTime.Today;
    public DateTime EventDate
    {
        get => eventDate;
        set
        {
            if (eventDate == value) return;
            eventDate = value;

            _ = CalculateUseableBudgetAsync();

            OnPropertyChanged();
        }
    }

    private TimeSpan eventTime = DateTime.Now.TimeOfDay;
    public TimeSpan EventTime
    {
        get => eventTime;
        set
        {
            if (eventTime == value) return;
            eventTime = value;
            OnPropertyChanged();
        }
    }

    public DateTime FullEventDate => EventDate.Date + EventTime;

    private string eventType = string.Empty;
    public string EventType
    {
        get => eventType;
        set
        {
            if (eventType == value) return;
            eventType = value;
            OnPropertyChanged();
        }
    }

    private int eventCost = 0;
    public int EventCost
    {
        get => eventCost;
        set
        {
            if (eventCost == value) return;
            eventCost = value;

            if (UseableBudgetLabel.Contains("Ft"))
                UseableBudgetLabel = $"{(UseableBudget - eventCost).ToString()} Ft";

            OnPropertyChanged();
        }
    }

    private string eventDesc = string.Empty;
    public string EventDesc
    {
        get => eventDesc;
        set
        {
            if (eventDesc == value) return;
            eventDesc = value;
            OnPropertyChanged();
        }
    }

    private int useableBudget = 0;
    public int UseableBudget
    {
        get => useableBudget;
        set
        {
            if (useableBudget == value) return;
            useableBudget = value;
            OnPropertyChanged();
        }
    }

    private string useableBudgetLabel = string.Empty;
    public string UseableBudgetLabel
    {
        get => useableBudgetLabel;
        set
        {
            if (useableBudgetLabel == value) return;
            useableBudgetLabel = value;
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
            CreateEventButtonCommand.RefreshCanExecute();
        }
    }

    public RelayCommand NavigateEventLogButtonCommand { get; }
    public RelayCommand CreateEventButtonCommand { get; }

    public NewEventViewModel(IAuthService authService, IDatabaseService databaseService, IUserSessionService userSessionService)
    {
        _authService = authService;
        _databaseService = databaseService;
        _userSessionService = userSessionService;

        NavigateEventLogButtonCommand = new RelayCommand(async _ => await NavigateToEventLogAsync());
        CreateEventButtonCommand = new RelayCommand(async _ => await CreateEventAsync(), _ => !IsLoading);
        
        if (Shell.Current != null)
        {
            if (_userSessionService.IsOfflineMode())
                WelcomeMessage = $"{_userSessionService.GetCurrentUser()!.Nickname} (Offline)";
            else
                WelcomeMessage = _userSessionService.GetCurrentUser()!.Nickname;

            if (_userSessionService.GetSelectedEvent() != null)
            {
                Event eventToEdit = _userSessionService.GetSelectedEvent() ?? new();
                EventName = eventToEdit.EventName;
                EventDate = eventToEdit.EventUserDate.Date;
                EventTime = eventToEdit.EventUserDate.TimeOfDay;
                EventType = eventToEdit.EventType;
                EventCost = eventToEdit.EventCost;
                EventDesc = eventToEdit.EventDescription;
            }
            else
                _ = CalculateUseableBudgetAsync();
        }
    }

    // A GotoPage folyamán elküldött értékek feldolgozása:
    /*public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        UseableBudget = int.Parse(query["useableBudget"].ToString()!);
        if (_userSessionService.GetSelectedEvent() != null)
            UseableBudget += _userSessionService.GetSelectedEvent()!.EventCost;

        UseableBudgetLabel = (UseableBudget - EventCost).ToString();
    }*/

    private async Task NavigateToEventLogAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az eseménynaplóhoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        WelcomeMessage = ErrorMessage = EventName = EventType = EventDesc = string.Empty;
        EventDate = DateTime.Today;
        EventTime = DateTime.Now.TimeOfDay;
        EventCost = 0;

        await Shell.Current.GoToAsync("//EventLogPage");
    }

    private async Task CreateEventAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az új esemény létrehozása sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        if (EventName == string.Empty || EventCost == 0)
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Kötelező megadni az esemény nevét és költségét!");
            return;
        }

        if (EventName.Length >= 30)
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az esemény neve maximum 30 karakter hosszú lehet!");
            return;
        }

        if (EventType.Length >= 20)
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az esemény típusa maximum 20 karakter hosszú lehet!");
            return;
        }

        if (EventDesc.Length >= 500)
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az esemény leírása maximum 500 karakter hosszú lehet!");
            return;
        }

        IsLoading = true;

        bool success;
        if (_userSessionService.GetSelectedEvent() == null)
            success = await _databaseService.CreateEventAsync(EventName, FullEventDate, EventType, EventCost, EventDesc);
        else
            success = await _databaseService.EditEventAsync(EventName, FullEventDate, EventType, EventCost, EventDesc);

        if (success)
        {
            WelcomeMessage = ErrorMessage = EventName = EventType = EventDesc = string.Empty;
            EventDate = DateTime.Today;
            EventTime = DateTime.Now.TimeOfDay;
            EventCost = 0;

            IsLoading = false;
            if (Shell.Current != null)
                await Shell.Current.GoToAsync("//EventLogPage");
        }
        else
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Új esemény létrehozása sikertelen!");

        IsLoading = false;
    }

    // TESZTELNI!!!
    private async Task CalculateUseableBudgetAsync()
    {
        string selectedMonth = EventDate.ToString("yyyy. MMMM");
        Budget? currentMonthBudget = _userSessionService.GetCurrentBudgets().Find(x => x.BudgetDate.ToString("yyyy. MMMM") == selectedMonth);
        int currentMonthEventCosts = _userSessionService.GetCurrentEventLog().Where(x => x.EventUserDate.ToString("yyyy. MMMM") == selectedMonth).Sum(x => x.EventCost);

        if (currentMonthBudget != null && currentMonthBudget.BudgetAmount != 0)
        {
            UseableBudget = currentMonthBudget.BudgetAmount - currentMonthEventCosts;

            if (_userSessionService.GetSelectedEvent() != null)
            {
                if (_userSessionService.GetSelectedEvent()!.EventUserDate.ToString("yyyy. MMMM") == selectedMonth)
                    UseableBudget += _userSessionService.GetSelectedEvent()!.EventCost;
            }

            UseableBudgetLabel = $"{UseableBudget - EventCost} Ft";
        }
        else
            UseableBudgetLabel = $"Nincs beállítva költségkeret!";

        await Task.CompletedTask;
    }

    private async Task LogoutAsync()
    {
        WelcomeMessage = EventName = EventType = EventDesc = string.Empty;
        EventDate = DateTime.Today;
        EventTime = DateTime.Now.TimeOfDay;
        EventCost = 0;
        await _authService.LogoutAsync();
    }
}