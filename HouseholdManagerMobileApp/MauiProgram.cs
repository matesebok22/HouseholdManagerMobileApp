using Microsoft.Extensions.Logging;
using HouseholdManagerMobileApp.Core.ViewModels;
using HouseholdManagerMobileApp.Views;

namespace HouseholdManagerMobileApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // ViewModel-ek regisztrálása:
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegistrationViewModel>();

            // View-k regisztrálása:
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegistrationPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
