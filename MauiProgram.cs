using Microsoft.Extensions.Logging;
using MindEase.Data;
using MindEase.Services;
using MindEase.Views;
using Plugin.LocalNotification;

namespace MindEase;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseLocalNotification()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont(
                    "OpenSans-Regular.ttf",
                    "OpenSansRegular");

                fonts.AddFont(
                    "OpenSans-Semibold.ttf",
                    "OpenSansSemibold");
            });

        // Database
        builder.Services.AddSingleton<AppDatabase>();

        // Authentication
        builder.Services.AddSingleton<AuthenticationService>();
        builder.Services.AddSingleton<ApiService>();

        // Pages
        builder.Services.AddTransient<WelcomePage>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<DiaryPage>();
        builder.Services.AddTransient<MoodCheckInPage>();
        builder.Services.AddTransient<JournalPage>();
        builder.Services.AddTransient<BreathingPage>();
        builder.Services.AddTransient<MoodHistoryPage>();
        builder.Services.AddTransient<CrisisResourcesPage>();
        builder.Services.AddTransient<AppointmentsPage>();
        builder.Services.AddTransient<CommunityPage>();
        builder.Services.AddTransient<CommunityPostDetailPage>();
        builder.Services.AddTransient<AboutPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}