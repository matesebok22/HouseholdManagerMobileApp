using HouseholdManagerMobileApp.Core.ViewModels;

namespace HouseholdManagerMobileApp.Views;

public partial class AnalyticsPage : ContentPage
{
	public AnalyticsPage(AnalyticsViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}