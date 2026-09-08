using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.Helpers;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;
    private CancellationTokenSource _connectivityCTS;
    private Task? _connectivityCheckTask;

    private string username = string.Empty;
    public string Username
    {
        get => username;
        set
        {
            if (username == value) return;
            username = value;
            OnPropertyChanged();
        }
    }

    private string password = string.Empty;
    public string Password
    {
        get => password;
        set
        {
            if (password == value) return;
            password = value;
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

    private bool isOnline = true;
    public bool IsOnline
    {
        get => isOnline;
        set
        {
            if (isOnline == value) return;
            isOnline = value;
            OnPropertyChanged();
        }
    }

    // Label miatt szükséges.
    private bool isOffline = false;
    public bool IsOffline
    {
        get => isOffline;
        set
        {
            if (isOffline == value) return;
            isOffline = value;
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

            LoginButtonCommand.RefreshCanExecute();
        }
    }

    public RelayCommand LoginButtonCommand { get; }
    public RelayCommand NavigateToRegistrationCommand { get; }

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;

        LoginButtonCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsLoading);
        NavigateToRegistrationCommand = new RelayCommand(async _ => await NavigateToRegistrationAsync());

        if (Shell.Current != null)
        {
            IsOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
            IsOffline = !IsOnline;
            StartConnectivityCheck();
        }
    }

    private async Task NavigateToRegistrationAsync()
    {
        StopConnectivityCheck();
        await Shell.Current.GoToAsync("//RegistrationPage");
    }

    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Kérlek tölts ki minden mezőt!");
            return;
        }

        if (IsOnline && string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Kérlek tölts ki minden mezőt!");
            return;
        }

        IsLoading = true;

        bool success = await _authService.LoginAsync(Username, Password);

        if (success)
        {
            IsLoading = false;
            StopConnectivityCheck();
            if (Shell.Current != null) // Tesztelés ne akadjon meg.
                await Shell.Current.GoToAsync("//UserPage");
        }
        else
        {
            if (!IsOnline)
            {
                ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Offline bejelentkezéshez kérlek jelentkezz legalább egy fiókba internetkapcsolattal!");
                IsLoading = false;
                return;
            } 
            
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Hibás felhasználónév vagy jelszó!");
        }
        
        IsLoading = false;
    }

    private void StartConnectivityCheck()
    {
        // Jelenleg fut-e az ellenőrzés:
        if (_connectivityCheckTask != null && !_connectivityCheckTask.IsCompleted)
            return;

        _connectivityCTS = new CancellationTokenSource();
        _connectivityCheckTask = CheckConnectivityPeriodicallyAsync(_connectivityCTS.Token);
    }

    private void StopConnectivityCheck() => _connectivityCTS?.Cancel();

    private async Task CheckConnectivityPeriodicallyAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // Internetkapcsolat ellenőrzés 2 másodpercenként.
                await Task.Delay(2000, cancellationToken);

                bool isCurrentlyOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                if (IsOnline != isCurrentlyOnline)
                {
                    IsOnline = isCurrentlyOnline;
                    IsOffline = !IsOnline;
                }
                    
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Internetkapcsolat ellenőrzés hiba: {ex.Message}");
            }
        }
    }
}