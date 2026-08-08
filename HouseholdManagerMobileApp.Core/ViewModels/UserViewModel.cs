using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HouseholdManagerMobileApp.Core.Services;

namespace HouseholdManagerMobileApp.Core.ViewModels;

public class UserViewModel : ViewModelBase
{
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

    public UserViewModel(IAuthService authService)
    {
        _authService = authService;

        WelcomeMessage = $"Üdvözöllek, {_authService.CurrentUsername}!";
    }
}
