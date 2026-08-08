using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HouseholdManagerMobileApp.Core.Commands;
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

    private bool isMenuVisible;
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

    public RelayCommand ToggleMenuButtonCommand { get; }
    public RelayCommand LogoutButtonCommand { get; }

    public UserViewModel(IAuthService authService)
    {
        _authService = authService;

        ToggleMenuButtonCommand = new RelayCommand(async _ => await Task.Run(() => IsMenuVisible = !IsMenuVisible));
        LogoutButtonCommand = new RelayCommand(async _ => await Task.Run(async () =>
        {
            IsMenuVisible = false;
            Nickname = WelcomeMessage = string.Empty;
            await _authService.LogoutAsync();
        }));

        Nickname = _authService.Nickname;
        WelcomeMessage = $"Üdvözöllek, {Nickname}!";
    }
}
