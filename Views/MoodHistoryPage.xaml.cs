using MindEase.Data;
using MindEase.Models;
using MindEase.Services;
using System.Security.Cryptography;

namespace MindEase.Views;

public partial class MoodHistoryPage : ContentPage
{
    private readonly AppDatabase _database;

    public MoodHistoryPage()
    {
        InitializeComponent();

        _database = new AppDatabase();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadMoodHistory();
    }

    private async Task LoadMoodHistory()
    {
        if (!UserSession.IsLoggedIn)
            return;

        try
        {
            int userId = UserSession.CurrentUser!.Id;

            var entries = await _database.GetMoodEntriesByUserAsync(userId);

            var displayItems = entries
                .Select(entry => new MoodDisplayItem
                {
                    Mood = entry.Mood,
                    Date = entry.Date,
                    EmojiText = MoodToEmoji(entry.Mood)
                })
                .ToList();

            MoodCollection.ItemsSource = displayItems;

            SummaryLabel.Text = BuildSummary(entries);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not load mood history: {ex.Message}",
                "OK");
        }
    }

    private string BuildSummary(List<MoodEntry> entries)
    {
        if (entries.Count == 0)
            return "";

        var last7Days = entries
            .Where(e => e.Date >= DateTime.Now.AddDays(-7))
            .ToList();

        if (last7Days.Count == 0)
            return $"{entries.Count} total entries logged.";

        var mostCommon = last7Days
            .GroupBy(e => e.Mood)
            .OrderByDescending(g => g.Count())
            .First();

        return $"Last 7 days: {last7Days.Count} check-ins — mostly \"{mostCommon.Key}\" ({mostCommon.Count()}x)";
    }

    private static string MoodToEmoji(string mood)
    {
        return mood switch
        {
            "Happy" => "😊",
            "Neutral" => "😐",
            "Sad" => "😔",
            "Anxious" => "😰",
            "Angry" => "😠",
            "Tired" => "😴",
            _ => "🌿"
        };
    }

    private class MoodDisplayItem
    {
        public string Mood { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string EmojiText { get; set; } = string.Empty;
    }
}