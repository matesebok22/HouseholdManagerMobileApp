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

public class SettingsViewModel : ViewModelBase, IQueryAttributable
{
    private readonly IAuthService _authService;
    private readonly IDatabaseService _databaseService;

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

    private string selectedHouseholdID = string.Empty;
    public string SelectedHouseholdID
    {
        get => selectedHouseholdID;
        set
        {
            if (selectedHouseholdID == value) return;
            selectedHouseholdID = value;
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

            // Frissítsd az adatbázist, amikor új háztartást választ ki
            if (!string.IsNullOrEmpty(selectedHousehold))
            {
                _ = ChangeSelectedHouseholdAsync();
            }
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
    public RelayCommand ShowJoinHouseholdButtonCommand { get; }
    public RelayCommand ShowCreateHouseholdButtonCommand { get; }
    public RelayCommand ShowEditHouseholdButtonCommand { get; }
    public RelayCommand CreateHouseholdButtonCommand { get; }
    public RelayCommand JoinHouseholdButtonCommand { get; }
    public RelayCommand EditHouseholdButtonCommand { get; }
    public RelayCommand ChangePasswordButtonCommand { get; }
    public RelayCommand LeaveHouseholdButtonCommand { get; }
    public RelayCommand DisbandHouseholdButtonCommand { get; }
    public RelayCommand LogoutButtonCommand { get; }

    public SettingsViewModel(IAuthService authService, IDatabaseService databaseService)
    {
        _authService = authService;
        _databaseService = databaseService;

        ToggleMenuButtonCommand = new RelayCommand(async _ => await Task.Run(() => IsMenuVisible = !IsMenuVisible));
        NavigateHomeButtonCommand = new RelayCommand(async _ => await NavigateToHomeAsync());
        ShowJoinHouseholdButtonCommand = new RelayCommand(async _ => await Task.Run(() => ChangeVisibility(1, IsJoinHouseholdVisible)));
        ShowCreateHouseholdButtonCommand = new RelayCommand(async _ => await Task.Run(() => ChangeVisibility(2, IsCreateHouseholdVisible)));
        ShowEditHouseholdButtonCommand = new RelayCommand(async _ => await Task.Run(() => ChangeVisibility(3, IsEditHouseholdVisible)), _ => _databaseService.CurrentHouseholdOwner);
        CreateHouseholdButtonCommand = new RelayCommand(async _ => await CreateHouseholdAsync());
        JoinHouseholdButtonCommand = new RelayCommand(async _ => await JoinHouseholdAsync());
        EditHouseholdButtonCommand = new RelayCommand(async _ => await EditHouseholdAsync());
        ChangePasswordButtonCommand = new RelayCommand(async _ => await ChangePasswordAsync());
        LeaveHouseholdButtonCommand = new RelayCommand(async _ => await LeaveHouseholdAsync());
        DisbandHouseholdButtonCommand = new RelayCommand(async _ => await DisbandHouseholdAsync(), _ => _databaseService.CurrentHouseholdOwner);
        LogoutButtonCommand = new RelayCommand(async _ => await LogoutAsync());

        WelcomeMessage = $"Üdv, {_authService.Nickname}";
    }

    // Csak akkor fut le, ha van paraméter az útvonal végén (? után)
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _ = LoadHouseholdsAsync();
    }

    private async Task NavigateToHomeAsync()
    {
        if (_authService.CurrentUsername == string.Empty)
        {
            await DisplayErrorAsync("A kezdőlaphoz való navigálás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        IsMenuVisible = false;
        await Shell.Current.GoToAsync("//UserPage");
    }

    private async Task LoadHouseholdsAsync()
    {
        HouseholdsList.Clear();

        var households = await _databaseService.LoadHouseholdsAsync(_authService.CurrentUsername);

        foreach ((string, string) household in households)
            HouseholdsList.Add($"{household.Item1} - {household.Item2}");

        if (HouseholdsList.Count > 0)
            SelectedHousehold = $"{_databaseService.CurrentHouseholdName} - {_databaseService.CurrentHouseholdAccess}";
    }

    private async Task ChangeSelectedHouseholdAsync()
    {
        try
        {
            IsCreateHouseholdVisible = IsEditHouseholdVisible = IsJoinHouseholdVisible = false;

            string[] parts = SelectedHousehold.Split(" - ");
            if (parts.Length == 2)
            {
                string householdAccessCode = parts[1];
                bool success = await _databaseService.ChangeSelectedHouseholdAsync(_authService.CurrentUsername, householdAccessCode);

                if (success)
                {
                    ShowEditHouseholdButtonCommand.RefreshCanExecute();
                    DisbandHouseholdButtonCommand.RefreshCanExecute();

                    EditHouseholdName = _databaseService.CurrentHouseholdName;
                    EditHouseholdDate = DateTime.Today;
                    LoadSelectedMonthBudget();
                }
                else
                    await DisplayErrorAsync("Háztartás váltás sikertelen!");
            }
        }
        catch (Exception ex)
        {
            await DisplayErrorAsync($"Hiba a háztartás váltásakor: {ex.Message}");
        }
    }

    private async Task JoinHouseholdAsync()
    {
        if (_authService.CurrentUsername == string.Empty)
        {
            await DisplayErrorAsync("Háztartáshoz csatlakozás sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        if (HouseholdAccessCode.Trim() == string.Empty)
        {
            await DisplayErrorAsync("Háztartás azonosító megadása kötelező!");
            return;
        }

        bool success = await _databaseService.JoinHouseholdAsync(_authService.CurrentUsername, HouseholdAccessCode);

        if (success)
        {
            if (Shell.Current != null)
                _ = Shell.Current.DisplayAlert("Siker", "Háztartáshoz sikeresen csatlakozva.", "OK");
            HouseholdAccessCode = string.Empty;
            IsJoinHouseholdVisible = false;
            _ = LoadHouseholdsAsync();
        }
        else
            await DisplayErrorAsync("Hiba a háztartáshoz csatlakozás közben.");
    }

    private async Task CreateHouseholdAsync()
    {
        if (_authService.CurrentUsername == string.Empty)
        {
            await DisplayErrorAsync("Új háztartás készítése sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        if (NewHouseholdName.Trim() == string.Empty)
        {
            await DisplayErrorAsync("Háztartásnév megadása kötelező!");
            return;
        }

        bool success = await _databaseService.CreateHouseholdAsync(_authService.CurrentUsername, NewHouseholdName);

        if (success)
        {
            if (Shell.Current != null)
                _ = Shell.Current.DisplayAlert("Siker", "Háztartás sikeresen létrehozva.", "OK");
            NewHouseholdName = string.Empty;
            IsCreateHouseholdVisible = false;
            _ = LoadHouseholdsAsync();
        }
        else
            await DisplayErrorAsync("Hiba a háztartás elkészítése közben.");
    }

    private async Task EditHouseholdAsync()
    {
        bool success = await _databaseService.EditHouseholdAsync(EditHouseholdName, EditHouseholdDate, EditHouseholdBudget);

        if (success)
        {
            if (Shell.Current != null)
                _ = Shell.Current.DisplayAlert("Siker", "Háztartás sikeresen módosítva.", "OK");
            IsEditHouseholdVisible = false;
            _ = LoadHouseholdsAsync();
        }
        else
            await DisplayErrorAsync("Hiba a háztartás módosítása közben.");
    }

    private async Task LeaveHouseholdAsync()
    {
        if (Shell.Current != null)
        {
            if (await Shell.Current.DisplayAlert("Alert", "Biztosan kilép a jelenlegi háztartásból?", "Igen", "Nem") == false)
                return;
        }

        bool success = await _databaseService.LeaveHouseholdAsync(_authService.CurrentUsername);

        if (success)
        {
            if (Shell.Current != null)
                _ = Shell.Current.DisplayAlert("Siker", "Háztartásból sikeresen kilépve.", "OK");
            _ = LoadHouseholdsAsync();
        }
        else
            await DisplayErrorAsync("Hiba a háztartásból kilépés közben.");
    }

    private async Task DisbandHouseholdAsync()
    {
        if (Shell.Current != null)
        {
            if (await Shell.Current.DisplayAlert("Alert", "Biztosan felbontja a jelenlegi háztartást?", "Igen", "Nem") == false)
                return;
        }

        bool success = await _databaseService.LeaveHouseholdAsync(_authService.CurrentUsername);

        if (success)
        {
            if (Shell.Current != null)
                _ = Shell.Current.DisplayAlert("Siker", "Háztartás sikeresen felbontva.", "OK");
            _ = LoadHouseholdsAsync();
        }
        else
            await DisplayErrorAsync("Hiba a háztartás felbontása közben.");
    }

    private async Task ChangePasswordAsync()
    {
        if (_authService.CurrentUsername == string.Empty)
        {
            await DisplayErrorAsync("A jelszó megváltoztatása sikertelen. Kérlek, jelentkezz be újra.");
            await LogoutAsync();
            return;
        }

        if (string.IsNullOrEmpty(OldPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmNewPassword))
        {
            await DisplayErrorAsync("Minden jelszó mezőt ki kell tölteni.");
            return;
        }

        if (OldPassword == NewPassword || OldPassword == ConfirmNewPassword)
        {
            await DisplayErrorAsync("A régi jelszó nem egyezhet az új jelszóval.");
            return;
        }

        if (NewPassword != ConfirmNewPassword)
        {
            await DisplayErrorAsync("Az új jelszavak nem egyeznek.");
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
            await DisplayErrorAsync("A régi és a jelenlegi jelszó nem egyezik!");
    }

    private async Task LogoutAsync()
    {
        IsMenuVisible = false;
        WelcomeMessage = ErrorMessage = string.Empty;
        await _authService.LogoutAsync();
    }

    private async Task DisplayErrorAsync(string message)
    {
        ErrorMessage = message;
        if (Shell.Current != null)
            await Shell.Current.DisplayAlert("Hiba", ErrorMessage, "OK");
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

        foreach (Budget budget in _databaseService.CurrentHouseholdBudgets)
        {
            if (selectedDate == $"{budget.BudgetDate.Year}. {budget.BudgetDate.Month}.")
                EditHouseholdBudget = budget.BudgetAmount;
        }
    }
}