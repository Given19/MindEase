using MindEase.Views;

namespace MindEase;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(
            nameof(LoginPage),
            typeof(LoginPage));

        Routing.RegisterRoute(
            nameof(RegisterPage),
            typeof(RegisterPage));

        Routing.RegisterRoute(
            nameof(HomePage),
            typeof(HomePage));

        Routing.RegisterRoute(
    nameof(Views.MoodCheckInPage),
    typeof(Views.MoodCheckInPage));

        Routing.RegisterRoute(
            nameof(MoodHistoryPage),
            typeof(MoodHistoryPage));

        Routing.RegisterRoute(
            nameof(DiaryPage),
            typeof(DiaryPage));

        Routing.RegisterRoute(
            nameof(JournalPage),
            typeof(JournalPage));

        Routing.RegisterRoute(
            nameof(BreathingPage),
            typeof(BreathingPage));

        Routing.RegisterRoute(
            nameof(CrisisResourcesPage),
            typeof(CrisisResourcesPage));

        Routing.RegisterRoute(
            nameof(AppointmentsPage),
            typeof(AppointmentsPage));

        Routing.RegisterRoute(
    nameof(Views.CommunityPage),
    typeof(Views.CommunityPage));

        Routing.RegisterRoute(
            nameof(Views.CommunityPostDetailPage),
            typeof(Views.CommunityPostDetailPage));

        Routing.RegisterRoute(
    nameof(Views.AboutPage),
    typeof(Views.AboutPage));

    }
}