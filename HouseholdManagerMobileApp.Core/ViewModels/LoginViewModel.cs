using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Services;
using HouseholdManagerMobileApp.Core.Helpers;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly IAuthService _authService;

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
        NavigateToRegistrationCommand = new RelayCommand(async _ => await Shell.Current.GoToAsync("//RegistrationPage"));
    }

    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Kérlek tölts ki minden mezőt!");
            return;
        }

        IsLoading = true;
        
        bool success = await _authService.LoginAsync(Username, Password);

        if (success)
        {
            IsLoading = false;
            if (Shell.Current != null) // Tesztelés ne akadjon meg.
                await Shell.Current.GoToAsync("//UserPage");
        }
        else
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Hibás felhasználónév vagy jelszó!");
        
        IsLoading = false;
    }
}