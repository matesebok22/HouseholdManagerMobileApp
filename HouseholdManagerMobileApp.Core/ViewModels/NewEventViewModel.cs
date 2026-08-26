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

    private int eventCost;
    public int EventCost
    {
        get => eventCost;
        set
        {
            if (eventCost == value) return;
            eventCost = value;
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

        WelcomeMessage = $"Üdv, {_userSessionService.GetCurrentUser().Nickname}";
    }

    private async Task NavigateToEventLogAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            await DisplayErrorAsync("Az eseménynaplóhoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        await Shell.Current.GoToAsync("//EventLogPage");
    }

    private async Task CreateEventAsync()
    {
        IsLoading = true;

        if (!_userSessionService.IsUserLoggedIn())
        {
            await DisplayErrorAsync("Az új esemény létrehozása sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        if (EventName == string.Empty || EventCost == 0)
        {
            await DisplayErrorAsync("Kötelező megadni az esemény nevét és költségét!");
            return;
        }

        if (EventName.Length >= 30)
        {
            await DisplayErrorAsync("Az esemény neve maximum 30 karakter hosszú lehet!");
            return;
        }

        if (EventType.Length >= 20)
        {
            await DisplayErrorAsync("Az esemény típusa maximum 20 karakter hosszú lehet!");
            return;
        }

        if (EventDesc.Length >= 500)
        {
            await DisplayErrorAsync("Az esemény leírása maximum 500 karakter hosszú lehet!");
            return;
        }

        bool success = await _databaseService.CreateEventAsync(EventName, FullEventDate, EventType, EventCost, EventDesc);
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
            await DisplayErrorAsync("Új esemény létrehozása sikertelen!");

        IsLoading = false;
    }

    private async Task LogoutAsync()
    {
        WelcomeMessage = ErrorMessage = EventName = EventType = EventDesc = string.Empty;
        EventDate = DateTime.Today;
        EventTime = DateTime.Now.TimeOfDay;
        EventCost = 0;
        await _authService.LogoutAsync();
    }

    private async Task DisplayErrorAsync(string message)
    {
        ErrorMessage = message;
        IsLoading = false;
        if (Shell.Current != null)
            await Shell.Current.DisplayAlert("Hiba", ErrorMessage, "OK");
    }
}