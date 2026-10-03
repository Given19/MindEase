
using MindEase.Data;
using MindEase.Models;
using MindEase.Services;

namespace MindEase.Views;

public partial class JournalPage : ContentPage
{
    private readonly AppDatabase _database;

    public JournalPage()
    {
        InitializeComponent();
        _database = new AppDatabase();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadJournalEntries();
    }

    private async Task LoadJournalEntries()
    {
        try
        {
            if (!UserSession.IsLoggedIn)
                return;

            int userId = UserSession.CurrentUser!.Id;

            var entries = await _database.GetJournalEntriesByUserAsync(userId);

            JournalCollection.ItemsSource = entries;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not load journal entries: {ex.Message}",
                "OK");
        }
    }

    private async void SaveJournal_Clicked(
        object? sender,
        EventArgs e)
    {
        if (!UserSession.IsLoggedIn)
        {
            await DisplayAlertAsync(
                "Not Logged In",
                "Please log in first.",
                "OK");

            return;
        }

        string title = TitleEntry.Text?.Trim() ?? "";
        string content = ContentEditor.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(title) ||
            string.IsNullOrWhiteSpace(content))
        {
            await DisplayAlertAsync(
                "Missing Information",
                "Please enter a title and write something.",
                "OK");

            return;
        }

        var journal = new JournalEntry
        {
            UserId = UserSession.CurrentUser!.Id,
            Title = title,
            Content = content,
            Date = DateTime.Now
        };

        await _database.SaveJournalAsync(journal);

        TitleEntry.Text = "";
        ContentEditor.Text = "";

        await LoadJournalEntries();

        await DisplayAlertAsync(
            "Saved ❤️",
            "Your journal entry has been saved.",
            "OK");
    }
}