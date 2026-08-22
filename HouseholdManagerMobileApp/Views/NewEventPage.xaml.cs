using HouseholdManagerMobileApp.Core.ViewModels;

namespace HouseholdManagerMobileApp.Views;

public partial class NewEventPage : ContentPage
{
	public NewEventPage(NewEventViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}