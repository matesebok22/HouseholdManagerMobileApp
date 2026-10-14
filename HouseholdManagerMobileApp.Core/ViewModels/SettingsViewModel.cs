using System.Collections.ObjectModel;

using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Helpers;
using HouseholdManagerMobileApp.Core.Models;
using HouseholdManagerMobileApp.Core.Services;
using Microsoft.IdentityModel.Tokens;

namespace HouseholdManagerMobileApp.Core.ViewModels;

/* AMI NEM MŰKÖDIK:
- Háztartáshoz csatlakozáskor, létrehozáskor, kilépéskor, felbontáskor a háztartás listája, kiválaszott elem, tagok listája nem frissül (csak ki- és bejelentkezéskor).
- Valamit kezdeni a SelectedHousehold változóval, hogy ne hívja kétszer ugyanazt a metódust.
- A tagok listája még ki- bejelentkezéskor sem frissül.
- Esemény létrehozása után nem enged bejelentkezni.
- Az offline adatbázis csak bejelentkezéskor frissül, nem frissül ha új eseményt adunk hozzá, háztartást váltunk stb.
- Potenciális hiba, amikor online jelentkezünk be, de utána elmegy az internet és váltunk oldalt, akkor kivételt dobhat, nem dob ki a bejelentkezéshez.
*/
public class SettingsViewModel : ViewModelBase//, IQueryAttributable
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

    private string newHouseholdName = string.Empty;
    public string NewHouseholdName
    {
        get => newHouseholdName;
        set
        {
            if (newHouseholdName == value) return;
            newHouseholdName = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<string> householdsList = [];
    public ObservableCollection<string> HouseholdsList
    {
        get => householdsList;
        set
        {
            if (householdsList == value) return;
            householdsList = value;
            OnPropertyChanged();
        }
    }

    private string selectedHousehold = string.Empty;
    public string SelectedHousehold
    {
        get => selectedHousehold;
        set
        {
            if (selectedHousehold == value) return;
            selectedHousehold = value;
            OnPropertyChanged();

            if (!string.IsNullOrEmpty(selectedHousehold) && !IsLoading)
            {
                _ = LoadHouseholdsAsync();
            }
        }
    }

    private ObservableCollection<string> usersInHousehold = [];
    public ObservableCollection<string> UsersInHousehold
    {
        get => usersInHousehold;
        set
        {
            if (usersInHousehold == value) return;
            usersInHousehold = value;
            OnPropertyChanged();
        }
    }

    private string householdAccessCode = string.Empty;
    public string HouseholdAccessCode
    {
        get => householdAccessCode;
        set
        {
            if (householdAccessCode == value) return;
            householdAccessCode = value;
            OnPropertyChanged();
        }
    }

    private string editHouseholdName = string.Empty;
    public string EditHouseholdName
    {
        get => editHouseholdName;
        set
        {
            if (editHouseholdName == value) return;
            editHouseholdName = value;
            OnPropertyChanged();
        }
    }

    private DateTime editHouseholdDate = DateTime.Today;
    public DateTime EditHouseholdDate
    {
        get => editHouseholdDate;
        set
        {
            if (editHouseholdDate == value) return;
            editHouseholdDate = value;
            OnPropertyChanged();

            LoadSelectedMonthBudget();
        }
    }

    private int editHouseholdBudget;
    public int EditHouseholdBudget
    {
        get => editHouseholdBudget;
        set
        {
            if (editHouseholdBudget == value) return;
            editHouseholdBudget = value;
            OnPropertyChanged();
        }
    }

    private bool notificationsIsChecked;
    public bool NotificationsIsChecked
    {
        get => notificationsIsChecked;
        set
        {
            if (notificationsIsChecked == value) return;
            notificationsIsChecked = value;
            OnPropertyChanged();

            _ = ToggleNotificationsAsync();
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
    public RelayCommand NavigateEventLogButtonCommand { get; }
    public RelayCommand NavigateNotificationsButtonCommand { get; }
    public RelayCommand ShowJoinHouseholdButtonCommand { get; }
    public RelayCommand ShowCreateHouseholdButtonCommand { get; }
    public RelayCommand ShowEditHouseholdButtonCommand { get; }
    public RelayCommand CreateHouseholdButtonCommand { get; }
    public RelayCommand JoinHouseholdButtonCommand { get; }
    public RelayCommand EditHouseholdButtonCommand { get; }
    public RelayCommand ChangePasswordButtonCommand { get; }
    public RelayCommand LeaveHouseholdButtonCommand { get; }
    public RelayCommand DisbandHouseholdButtonCommand { get; }
    public RelayCommand ToggleNotificationsCommand { get; }
    public RelayCommand LogoutButtonCommand { get; }

    public SettingsViewModel(IAuthService authService, IDatabaseService databaseService, IUserSessionService userSessionService)
    {
        _authService = authService;
        _databaseService = databaseService;
        _userSessionService = userSessionService;

        ToggleMenuButtonCommand = new RelayCommand(async _ => await Task.Run(() => IsMenuVisible = !IsMenuVisible));
        NavigateHomeButtonCommand = new RelayCommand(async _ => await NavigateToHomeAsync());
        NavigateEventLogButtonCommand = new RelayCommand(async _ => await NavigateToEventLogAsync());
        NavigateNotificationsButtonCommand = new RelayCommand(async _ => await NavigateToNotificationsAsync());
        ShowJoinHouseholdButtonCommand = new RelayCommand(async _ => await Task.Run(() => ChangeVisibility(1, IsJoinHouseholdVisible)));
        ShowCreateHouseholdButtonCommand = new RelayCommand(async _ => await Task.Run(() => ChangeVisibility(2, IsCreateHouseholdVisible)));
        ShowEditHouseholdButtonCommand = new RelayCommand(async _ => await Task.Run(() => ChangeVisibility(3, IsEditHouseholdVisible)), _ => _userSessionService.IsCurrentHouseholdOwner());
        CreateHouseholdButtonCommand = new RelayCommand(async _ => await CreateHouseholdAsync());
        JoinHouseholdButtonCommand = new RelayCommand(async _ => await JoinHouseholdAsync());
        EditHouseholdButtonCommand = new RelayCommand(async _ => await EditHouseholdAsync());
        ChangePasswordButtonCommand = new RelayCommand(async _ => await ChangePasswordAsync());
        LeaveHouseholdButtonCommand = new RelayCommand(async _ => await LeaveHouseholdAsync(), _ => (ActiveHousehold && !_userSessionService.IsCurrentHouseholdOwner()));
        DisbandHouseholdButtonCommand = new RelayCommand(async _ => await DisbandHouseholdAsync(), _ => (ActiveHousehold && _userSessionService.IsCurrentHouseholdOwner()));
        ToggleNotificationsCommand = new RelayCommand(async _ => await Task.Run(() => NotificationsIsChecked = !NotificationsIsChecked));
        LogoutButtonCommand = new RelayCommand(async _ => await LogoutAsync());

        if (Shell.Current != null)
            _ = LoadHouseholdsAsync();
    }

    private async Task NavigateToHomeAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("A kezdőlaphoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        IsMenuVisible = false;
        await Shell.Current.GoToAsync("//UserPage");
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
        await Shell.Current.GoToAsync("//NotificationsPage");
    }

    private async Task LoadHouseholdsAsync()
    {
        IsLoading = true;
        IsCreateHouseholdVisible = IsEditHouseholdVisible = IsJoinHouseholdVisible = false;
        bool success = false;

        if (SelectedHousehold.IsNullOrEmpty())
            success = await _databaseService.LoadSettingsPageAsync();
        else
        {
            string[] parts = SelectedHousehold.Split(" - ");
            success = await _databaseService.LoadSettingsPageAsync(parts[1]);
        }

        if (_userSessionService.IsOfflineMode())
            WelcomeMessage = $"{_userSessionService.GetCurrentUser()!.Nickname} (Offline)";
        else
            WelcomeMessage = _userSessionService.GetCurrentUser()!.Nickname;

        if (_userSessionService.GetCurrentHousehold() != null)
            ActiveHousehold = true;

        if (success)
        {
            HouseholdsList.Clear();
            if (!_userSessionService.IsOfflineMode())
            {
                foreach (Household household in _userSessionService.GetCurrentHouseholdList())
                    HouseholdsList.Add($"{household.HouseholdName} - {household.HouseholdAccess}");
            }
            else
            {
                Household userHH = _userSessionService.GetCurrentHousehold()!;
                HouseholdsList.Add($"{userHH.HouseholdName} - {userHH.HouseholdAccess}");
            }

            Household? currentHousehold = _userSessionService.GetCurrentHousehold();
            if (currentHousehold != null)
            {
                string householdDisplay = $"{currentHousehold.HouseholdName} - {currentHousehold.HouseholdAccess}";
                if (HouseholdsList.Contains(householdDisplay))
                {
                    selectedHousehold = householdDisplay;
                    OnPropertyChanged(nameof(SelectedHousehold));
                }
            }

            UsersInHousehold.Clear();
            foreach (User user in _userSessionService.GetCurrentHouseholdUsers())
            {
                string userText = user.Nickname;
                userText += user.UID == _userSessionService.GetCurrentUser()!.UID ? " (Ön)" : "";
                userText += user.UID == _userSessionService.GetCurrentHousehold()!.HouseholdOwner ? " (Alapító)" : "";
                UsersInHousehold.Add(userText);
            }

            if (Shell.Current != null)
            {
                string userID = _userSessionService.GetCurrentUser()!.UID;
                Household? userHH = _userSessionService.GetCurrentHousehold();
                if (userHH != null)
                {
                    NotificationsIsChecked = bool.Parse(await SecureStorage.GetAsync($"notifications-{userID}-{userHH.HHID}") ?? "true");

                    EditHouseholdName = userHH.HouseholdName;
                    EditHouseholdDate = DateTime.Today;
                    LoadSelectedMonthBudget();
                }
            }

            ShowEditHouseholdButtonCommand.RefreshCanExecute();
            LeaveHouseholdButtonCommand.RefreshCanExecute();
            DisbandHouseholdButtonCommand.RefreshCanExecute();
        }

        IsLoading = false;
    }

    private async Task JoinHouseholdAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Háztartáshoz csatlakozás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        if (_userSessionService.IsOfflineMode())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Offline módban nem lehetséges a háztartáshoz csatlakozás.");
            return;
        }

        if (HouseholdAccessCode.Trim() == string.Empty)
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Háztartás azonosító megadása kötelező!");
            return;
        }

        bool success = await _databaseService.JoinHouseholdAsync(HouseholdAccessCode);
        if (success)
        {
            if (Shell.Current != null)
                _ = Shell.Current.DisplayAlert("Siker", "Háztartáshoz sikeresen csatlakozva.", "OK");
            HouseholdAccessCode = SelectedHousehold = string.Empty;
            IsJoinHouseholdVisible = false;

            await LoadHouseholdsAsync();
        }
        else
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Hiba a háztartáshoz csatlakozás közben.");
    }

    private async Task CreateHouseholdAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Új háztartás készítése sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        if (_userSessionService.IsOfflineMode())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Offline módban nem lehetséges a háztartás készítése.");
            return;
        }

        if (NewHouseholdName.Trim() == string.Empty)
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Háztartásnév megadása kötelező!");
            return;
        }

        bool success = await _databaseService.CreateHouseholdAsync(NewHouseholdName);
        if (success)
        {
            if (Shell.Current != null)
                _ = Shell.Current.DisplayAlert("Siker", "Háztartás sikeresen létrehozva.", "OK");
            NewHouseholdName = string.Empty;
            IsCreateHouseholdVisible = false;
            await LoadHouseholdsAsync();
        }
        else
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Hiba a háztartás elkészítése közben.");
    }

    private async Task EditHouseholdAsync()
    {
        bool success = await _databaseService.EditHouseholdAsync(EditHouseholdName, EditHouseholdDate, EditHouseholdBudget);

        if (success)
        {
            if (Shell.Current != null)
                _ = Shell.Current.DisplayAlert("Siker", "Háztartás sikeresen módosítva.", "OK");
            IsEditHouseholdVisible = false;
            
            EditHouseholdName = _userSessionService.GetCurrentHousehold()!.HouseholdName;

            await LoadHouseholdsAsync();
        }
        else
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Hiba a háztartás módosítása közben.");
    }

    private async Task LeaveHouseholdAsync()
    {
        if (_userSessionService.IsOfflineMode())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Offline módban nem lehetséges a háztartásból való kilépés.");
            return;
        }

        if (Shell.Current != null)
        {
            if (await Shell.Current.DisplayAlert("Alert", "Biztosan kilép a jelenlegi háztartásból?", "Igen", "Nem") == false)
                return;
        }

        bool success = await _databaseService.LeaveHouseholdAsync();
        if (success)
        {
            if (Shell.Current != null)
                _ = Shell.Current.DisplayAlert("Siker", "Háztartásból sikeresen kilépve.", "OK");
            await LoadHouseholdsAsync();
        }
        else
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Hiba a háztartásból kilépés közben.");
    }

    private async Task DisbandHouseholdAsync()
    {
        if (_userSessionService.IsOfflineMode())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Offline módban nem lehetséges a háztartás felbontása.");
            return;
        }

        if (Shell.Current != null)
        {
            if (await Shell.Current.DisplayAlert("Alert", "Biztosan felbontja a jelenlegi háztartást?", "Igen", "Nem") == false)
                return;
        }

        bool success = await _databaseService.LeaveHouseholdAsync();
        if (success)
        {
            if (Shell.Current != null)
                _ = Shell.Current.DisplayAlert("Siker", "Háztartás sikeresen felbontva.", "OK");
            await LoadHouseholdsAsync();
        }
        else
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Hiba a háztartás felbontása közben.");
    }

    private async Task ChangePasswordAsync()
    {
        if (!_userSessionService.IsUserLoggedIn())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("A jelszó megváltoztatása sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        if (_userSessionService.IsOfflineMode())
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Offline módban nem lehetséges a jelszó megváltoztatása.");
            return;
        }

        if (string.IsNullOrEmpty(OldPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmNewPassword))
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Minden jelszó mezőt ki kell tölteni.");
            return;
        }

        if (OldPassword == NewPassword || OldPassword == ConfirmNewPassword)
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("A régi jelszó nem egyezhet az új jelszóval.");
            return;
        }
        else if (NewPassword.Length < 8)
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az új jelszónak legalább 8 karakter hosszúnak kell lennie!");
            return;
        }
        else if (!NewPassword.Any(char.IsUpper))
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az új jelszónak tartalmaznia kell legalább egy nagybetűt!");
            return;
        }
        else if (!NewPassword.Any(char.IsLower))
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az új jelszónak tartalmaznia kell legalább egy kisbetűt!");
            return;
        }
        else if (!NewPassword.Any(char.IsDigit))
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az új jelszónak tartalmaznia kell legalább egy számot!");
            return;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Az új jelszavak nem egyeznek.");
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
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("A régi és a jelenlegi jelszó nem egyezik!");
    }

    private async Task ToggleNotificationsAsync()
    {
        // A telefon értesítések engedélyezése vagy letiltása a felhasználói beállítások alapján:
        string userID = _userSessionService.GetCurrentUser()!.UID;
        string userHH = _userSessionService.GetCurrentHousehold()!.HHID;
        await SecureStorage.SetAsync($"notifications-{userID}-{userHH}", NotificationsIsChecked.ToString());
    }

    private async Task LogoutAsync()
    {
        IsMenuVisible = false;
        WelcomeMessage = string.Empty;
        await _authService.LogoutAsync();
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

    private void LoadSelectedMonthBudget()
    {
        EditHouseholdBudget = 0;
        string selectedDate = $"{EditHouseholdDate.Year}. {EditHouseholdDate.Month}.";

        foreach (Budget budget in _userSessionService.GetCurrentBudgets())
        {
            if (selectedDate == $"{budget.BudgetDate.Year}. {budget.BudgetDate.Month}.")
                EditHouseholdBudget = budget.BudgetAmount;
        }
    }
}