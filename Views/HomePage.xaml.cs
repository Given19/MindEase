using MindEase.Data;
using MindEase.Services;

namespace MindEase.Views;

public partial class HomePage : ContentPage
{
    private readonly AppDatabase _database;

    public HomePage(AppDatabase database)
    {
        InitializeComponent();

        _database = database;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

#if ANDROID || IOS || MACCATALYST

        bool granted =
            await NotificationService
                .RequestPermissionAsync();

        if (granted)
        {
            await NotificationService
                .ScheduleDailyCheckInAsync(
                    20,
                    0);
        }

#endif
    }

    private async void CrisisResources_Tapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CrisisResourcesPage));
    }

    private async void CheckIn_Tapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MoodCheckInPage));
    }

    private async void Diary_Tapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(DiaryPage));
    }

    private async void Journal_Tapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(JournalPage));
    }

    private async void MoodHistory_Tapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(MoodHistoryPage));
    }

    private async void Breathing_Tapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(BreathingPage));
    }

    private async void Community_Tapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CommunityPage));
    }

    private async void Appointments_Tapped(object? sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(AppointmentsPage));
    }

    private async void Logout_Tapped(object? sender, TappedEventArgs e)
    {
        UserSession.Logout();

        await Shell.Current.GoToAsync(
            "//WelcomePage");
    }
}