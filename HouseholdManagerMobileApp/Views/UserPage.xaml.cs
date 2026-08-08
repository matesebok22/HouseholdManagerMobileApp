using HouseholdManagerMobileApp.Core.ViewModels;

namespace HouseholdManagerMobileApp.Views;

public partial class UserPage : ContentPage
{
	public UserPage(UserViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}