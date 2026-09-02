using Microsoft.Extensions.Logging;
using Supabase;

using HouseholdManagerMobileApp.Core.ViewModels;
using HouseholdManagerMobileApp.Core.Helpers;
using HouseholdManagerMobileApp.Core.Services;
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

            // Supabase szolgáltatás regisztrálása:
            builder.Services.AddSingleton(provider => new Client(Constants.SupabaseUrl, Constants.SupabaseKey, new SupabaseOptions() { AutoConnectRealtime = true }));

            // Szolgáltatások regisztrálása:
            builder.Services.AddSingleton<IUserSessionService, UserSessionService>();
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();

            // ViewModel-ek regisztrálása:
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegistrationViewModel>();
            builder.Services.AddTransient<UserViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<EventLogViewModel>();
            builder.Services.AddTransient<NewEventViewModel>();

            // View-k regisztrálása:
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<UserPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<EventLogPage>();
            builder.Services.AddTransient<NewEventPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            MauiApp app = builder.Build();
            Task.Run(async () =>
            {
                // Supabase kliens inicializálása:
                IDatabaseService dbService = app.Services.GetRequiredService<IDatabaseService>();
                await dbService.InitializeAsync();

                /*// Értesítésekre feliratkozás a Supabase Realtime segítségével:
                INotificationService notificationService = app.Services.GetRequiredService<INotificationService>();
                await notificationService.SubscribeToNotificationsAsync(notification =>
                {
                    // Értesítés fogadásakor
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await Application.Current?.MainPage?.DisplayAlert(notification.Title, notification.Message, "OK");
                    });
                });*/
            });

            //return builder.Build();
            return app;
        }
    }
}
