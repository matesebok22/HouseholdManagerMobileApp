using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Helpers;
using HouseholdManagerMobileApp.Core.Services;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class RegistrationViewModel : ViewModelBase
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

    private string nickname = string.Empty;
    public string Nickname
    {
        get => nickname;
        set
        {
            if (nickname == value) return;
            nickname = value;
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

    private string confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => confirmPassword;
        set
        {
            if (confirmPassword == value) return;
            confirmPassword = value;
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

    private bool isLoading = false;
    public bool IsLoading
    {
        get => isLoading;
        set
        {
            if (isLoading == value) return;
            isLoading = value;
            OnPropertyChanged();
            RegisterButtonCommand.RefreshCanExecute();
        }
    }

    public RelayCommand RegisterButtonCommand { get; }
    public RelayCommand NavigateToLoginCommand { get; }

    public RegistrationViewModel(IAuthService authService)
    {
        _authService = authService;

        RegisterButtonCommand = new RelayCommand(async _ => await RegisterAsync(), _ => !IsLoading);
        NavigateToLoginCommand = new RelayCommand(async _ => await Shell.Current.GoToAsync("//LoginPage"));

        if (Shell.Current != null)
            IsOnline = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
    }

    private async Task RegisterAsync()
    {
        ErrorMessage = string.Empty;

        if (!IsOnline)
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Nincs internetkapcsolat, próbáld meg később!");
            return;
        }

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Nickname) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("Kérlek tölts ki minden mezőt!");
            return;
        }
        else if (Password != ConfirmPassword)
        {
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("A jelszavak nem egyeznek!");
            return;
        }

        IsLoading = true;

        bool success = await _authService.RegisterAsync(Username, Nickname, Password);

        if (success)
        {
            if (Shell.Current != null)
                await Shell.Current.GoToAsync("//LoginPage");
        }
        else
            ErrorMessage = await DisplayErrorHelper.DisplayErrorAsync("A regisztráció sikertelen. Próbáld meg később!");

        IsLoading = false;
    }
}
