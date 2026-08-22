using HouseholdManagerMobileApp.Core.ViewModels;

namespace HouseholdManagerMobileApp.Views;

public partial class EventLogPage : ContentPage
{
	public EventLogPage(EventLogViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}