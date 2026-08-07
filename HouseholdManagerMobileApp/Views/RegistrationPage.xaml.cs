using HouseholdManagerMobileApp.Core.ViewModels;

namespace HouseholdManagerMobileApp.Views;

public partial class RegistrationPage : ContentPage
{
	public RegistrationPage(RegistrationViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}