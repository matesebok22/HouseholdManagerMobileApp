using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HouseholdManagerMobileApp.Core.Commands;
using Supabase;

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

    public LoginViewModel()
    {
        LoginButtonCommand = new RelayCommand(async _ => await LoginAsync(), _ => !IsLoading);
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

        IsLoading = true;
        
        bool success = await SupabaseLoginAsync(Username, Password);

        if (success)
            await Shell.Current.GoToAsync("//UserPage");
        else
            ErrorMessage = "Hibás felhasználónév vagy jelszó!";
        
        IsLoading = false;
    }

    private async Task<bool> SupabaseLoginAsync(string username, string password)
    {
        try
        {
            return true; // Replace with actual Supabase login logic
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Hiba történt a regisztráció során: {ex.Message}";
            return false;
        }
    }
}