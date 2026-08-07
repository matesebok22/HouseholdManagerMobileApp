using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HouseholdManagerMobileApp.Core.Commands;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class LoginViewModel : ViewModelBase
{
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

    public RelayCommand LoginButtonCommand { get; }
    public RelayCommand NavigateToRegistrationCommand { get; }

    public LoginViewModel()
    {
        LoginButtonCommand = new RelayCommand(async _ => await LoginAsync());
        NavigateToRegistrationCommand = new RelayCommand(async _ => await Shell.Current.GoToAsync("//RegistrationPage"));
    }

    private async Task LoginAsync()
    {
        
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Kérlek tölts ki minden mezőt!";
            return;
        }
        /*
        bool success = await _authService.LoginAsync(Username, Password);

        if (success)
            await Shell.Current.GoToAsync("//UserPage");
        else
            ErrorMessage = "Hibás felhasználónév vagy jelszó!";
        */
    }
}