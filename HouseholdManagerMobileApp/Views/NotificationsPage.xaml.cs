namespace HouseholdManagerMobileApp.Views;

using HouseholdManagerMobileApp.Core.ViewModels;

public partial class NotificationsPage : ContentPage
{
	public NotificationsPage(NotificationsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}