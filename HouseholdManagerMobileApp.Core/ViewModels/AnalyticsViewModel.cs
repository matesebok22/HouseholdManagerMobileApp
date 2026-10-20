using System.Web;
using System.Collections.ObjectModel;

using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Helpers;
using HouseholdManagerMobileApp.Core.Models;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class AnalyticsViewModel : ViewModelBase, IQueryAttributable
{
    private readonly IUserSessionService _userSessionService;
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

    private bool queryIsDate = false;
    public bool QueryIsDate
    {
        get => queryIsDate;
        set
        {
            if (queryIsDate == value) return;
            queryIsDate = value;
            OnPropertyChanged();
        }
    }

    private string queryCategory = string.Empty;
    public string QueryCategory
    {
        get => queryCategory;
        set
        {
            if (queryCategory == value) return;
            queryCategory = value;
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

    private string categoryName = string.Empty;
    public string CategoryName
    {
        get => categoryName;
        set
        {
            if (categoryName == value) return;
            categoryName = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<(string, int)> defaultList = [];
    public ObservableCollection<(string, int)> DefaultList
    {
        get => defaultList;
        set
        {
            if (defaultList == value) return;
            defaultList = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<(string, int)> fullList = [];
    public ObservableCollection<(string, int)> FullList
    {
        get => fullList;
        set
        {
            if (fullList == value) return;
            fullList = value;
            OnPropertyChanged();
        }
    }

    private bool defaultListVisible = true;
    public bool DefaultListVisible
    {
        get => defaultListVisible;
        set
        {
            if (defaultListVisible == value) return;
            defaultListVisible = value;
            OnPropertyChanged();
        }
    }

    private bool fullListVisible = false;
    public bool FullListVisible
    {
        get => fullListVisible;
        set
        {
            if (fullListVisible == value) return;
            fullListVisible = value;
            OnPropertyChanged();
        }
    }

    private string changeListButtonName = string.Empty;
    public string ChangeListButtonName
    {
        get => changeListButtonName;
        set
        {
            if (changeListButtonName == value) return;
            changeListButtonName = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand NavigateEventLogButtonCommand { get; }
    public RelayCommand ChangeListButtonCommand { get; }

    public AnalyticsViewModel(IUserSessionService userSessionService, IAuthService authService)
    {
        _userSessionService = userSessionService;
        _authService = authService;

        NavigateEventLogButtonCommand = new RelayCommand(async _ => await NavigateToEventLogAsync());
        ChangeListButtonCommand = new RelayCommand(async _ => await ChangeListAsync());
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        QueryIsDate = query["isDate"].ToString() == "1";
        string encodedCategory = query["category"].ToString() ?? string.Empty;
        QueryCategory = HttpUtility.UrlDecode(encodedCategory);

        _ = LoadAnalyticsAsync();
    }

    private async Task NavigateToEventLogAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az eseménynaplóhoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        WelcomeMessage = string.Empty;

        await Shell.Current.GoToAsync("//EventLogPage");
    }

    private async Task LoadAnalyticsAsync()
    {
        if (_userSessionService.IsOfflineMode())
            WelcomeMessage = $"{_userSessionService.GetCurrentUser()!.Nickname} (Offline)";
        else
            WelcomeMessage = _userSessionService.GetCurrentUser()!.Nickname;

        CurrentHouseholdName = _userSessionService.GetCurrentHousehold()!.HouseholdName;
        if (QueryIsDate)
        {
            CategoryName = QueryCategory;
            ChangeListButtonName = "Év nézet";
        }
        else
        {
            CategoryName = $"{QueryCategory} - Idén";
            ChangeListButtonName = "Összes év nézet";
        }


        List<Event> eventlog = _userSessionService.GetCurrentEventLog();
        // Alapértelmezett lista betöltése:
        if (QueryIsDate)
        {
            DateTime currentMonth = DateTime.Parse(QueryCategory);
            List<(string, int)> filteredEvents = eventlog.Where(x => x.EventUserDate.Date == currentMonth.Date).GroupBy(x => x.EventType).Select(x => (x.Key, x.Sum(x => x.EventCost))).ToList();

            DefaultList = new ObservableCollection<(string, int)>(filteredEvents);
        }
        else
        {
            List<(string, int)> filteredEvents = eventlog.Where(x => x.EventType == QueryCategory && x.EventUserDate.Year == DateTime.Today.Year).GroupBy(x => x.EventUserDate.ToString("yyyy. MMMM")).Select(x => (x.Key, x.Sum(x => x.EventCost))).ToList();

            DefaultList = new ObservableCollection<(string, int)>(filteredEvents);
        }

        // Teljes lista betöltése:
        if (QueryIsDate)
        {
            DateTime currentYear = DateTime.Parse(QueryCategory);
            List<(string, int)> filteredFullEvents = eventlog.Where(x => x.EventUserDate.Year == currentYear.Year).GroupBy(x => x.EventType).Select(x => (x.Key, x.Sum(x => x.EventCost))).ToList();

            FullList = new ObservableCollection<(string, int)>(filteredFullEvents);
        }
        else
        {
            List<(string, int)> filteredFullEvents = eventlog.Where(x => x.EventType == QueryCategory).GroupBy(x => x.EventUserDate.ToString("yyyy.")).Select(x => (x.Key, x.Sum(x => x.EventCost))).ToList();

            FullList = new ObservableCollection<(string, int)>(filteredFullEvents);
        }

        await Task.CompletedTask;
    }

    private async Task ChangeListAsync()
    {
        DefaultListVisible = !DefaultListVisible;
        FullListVisible = !FullListVisible;

        if (QueryIsDate)
        {
            if (FullListVisible)
            {
                CategoryName = DateTime.Parse(QueryCategory).ToString("yyyy");
                ChangeListButtonName = "Hónap nézet";
            }
            else
            {
                CategoryName = QueryCategory;
                ChangeListButtonName = "Év nézet";
            }
        }
        else
        {
            if (FullListVisible)
            {
                CategoryName = $"{QueryCategory} - Összes";
                ChangeListButtonName = "Jelen év nézet";
            }
            else
            {
                CategoryName = $"{QueryCategory} - Idén";
                ChangeListButtonName = "Összes év nézet";
            }
        }

        await Task.CompletedTask;
    }

    private async Task LogoutAsync()
    {
        WelcomeMessage = string.Empty;
        await _authService.LogoutAsync();
    }
}