using HouseholdManagerMobileApp.Core.ViewModels;

namespace HouseholdManagerMobileApp.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}