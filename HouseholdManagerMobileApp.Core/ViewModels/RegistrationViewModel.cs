using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HouseholdManagerMobileApp.Core.Commands;
using HouseholdManagerMobileApp.Core.Models;
using Supabase;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class RegistrationViewModel : ViewModelBase
{
    // Tesztelés service nélkül:


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

    public RegistrationViewModel()
    {
        RegisterButtonCommand = new RelayCommand(async _ => await RegisterAsync(), _ => !IsLoading);
        NavigateToLoginCommand = new RelayCommand(async _ => await Shell.Current.GoToAsync("//LoginPage"));
    }

    private async Task RegisterAsync()
    {

        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
        {
            ErrorMessage = "Kérlek tölts ki minden mezőt!";
            return;
        }
        else if (Password != ConfirmPassword)
        {
            ErrorMessage = "A jelszavak nem egyeznek!";
            return;
        }

        IsLoading = true;

        bool success = await SupabaseRegisterAsync(Username, Password);

        if (success)
            await Shell.Current.GoToAsync("//LoginPage");
        else
            ErrorMessage = "A regisztráció sikertelen. Próbálj meg később!";

        IsLoading = false;
    }

    private async Task<bool> SupabaseRegisterAsync(string username, string password)
    {
        try
        {
            SupabaseOptions options = new() { AutoConnectRealtime = true };

            Client supabase = new(Helpers.Constants.SupabaseUrl, Helpers.Constants.SupabaseKey, options);
            await supabase.InitializeAsync();

            // Új felhasználó létrehozása:
            User newUser = new() { Username = username, Password = password };

            var response = await supabase.From<User>().Insert(newUser);
            
            return response != null && response.Models.Count > 0;
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Hiba történt a regisztráció során: {ex.Message}";
            return false;
        }
    }
}
