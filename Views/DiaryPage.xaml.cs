using MindEase.Data;
using MindEase.Models;
using MindEase.Services;

namespace MindEase.Views;

public partial class DiaryPage : ContentPage
{
    private readonly AppDatabase _database;

    private string _lastSuggestion = "";
    private string _lastMotivation = "";
    private string _lastMood = "";

    public DiaryPage()
    {
        InitializeComponent();

        _database = new AppDatabase();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadDiaryEntries();
    }

    private async Task LoadDiaryEntries()
    {
        if (!UserSession.IsLoggedIn)
            return;

        try
        {
            int userId = UserSession.CurrentUser!.Id;

            var entries =
                await _database.GetDiaryEntriesByUserAsync(userId);

            DiaryCollection.ItemsSource = entries;
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Error",
                $"Could not load diary entries.\n\n{ex.Message}",
                "OK");
        }
    }

    private async void SaveDiaryButton_Clicked(
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

        string content = DiaryEditor.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(content))
        {
            await DisplayAlertAsync(
                "Empty Diary",
                "Write something before saving.",
                "OK");

            return;
        }

        // If the user hasn't tapped "Get Suggestions" yet for this
        // text, compute it now so it's never saved blank.
        if (_lastSuggestion == "" || _lastMotivation == "")
        {
            var computed = GetMindEaseResponse(content);
            _lastSuggestion = computed.Suggestion;
            _lastMotivation = computed.Motivation;
            _lastMood = computed.Mood;
        }

        var diaryEntry = new DiaryEntry
        {
            UserId = UserSession.CurrentUser!.Id,
            Content = content,
            Mood = _lastMood,
            Suggestion = _lastSuggestion,
            Motivation = _lastMotivation,
            Date = DateTime.Now
        };

        await _database.SaveDiaryEntryAsync(diaryEntry);

        DiaryEditor.Text = "";
        _lastSuggestion = "";
        _lastMotivation = "";
        _lastMood = "";
        ResponseCard.IsVisible = false;

        await LoadDiaryEntries();

        await DisplayAlertAsync(
            "Saved 🌿",
            "Your diary entry has been saved.",
            "OK");
    }

    private async void SuggestionsButton_Clicked(
        object? sender,
        EventArgs e)
    {
        string content = DiaryEditor.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(content))
        {
            await DisplayAlertAsync(
                "Write Something",
                "Write what's on your mind first.",
                "OK");

            return;
        }

        var result = GetMindEaseResponse(content);

        _lastSuggestion = result.Suggestion;
        _lastMotivation = result.Motivation;
        _lastMood = result.Mood;

        SuggestionLabel.Text = result.Suggestion;
        MotivationLabel.Text = result.Motivation;

        ResponseCard.IsVisible = true;
    }

    private (string Mood, string Suggestion, string Motivation)
        GetMindEaseResponse(string text)
    {
        string lowerText = text.ToLower();

        if (ContainsAny(
            lowerText,
            "stress",
            "stressed",
            "pressure",
            "overwhelmed"))
        {
            return (
                "Stressed",
                "It sounds like you're carrying a lot right now. 🌿 Try breaking what's bothering you into one small task at a time. Take a few slow breaths and focus only on what you can control right now.",
                "💪 You don't have to solve everything today. One small step is still progress."
            );
        }

        if (ContainsAny(
            lowerText,
            "sad",
            "cry",
            "crying",
            "hurt",
            "heartbroken"))
        {
            return (
                "Sad",
                "It sounds like you're going through a difficult moment. Give yourself permission to feel what you're feeling. Consider talking to someone you trust and give yourself some time to recover.",
                "❤️ This moment is difficult, but difficult moments don't last forever."
            );
        }

        if (ContainsAny(
            lowerText,
            "angry",
            "mad",
            "furious"))
        {
            return (
                "Angry",
                "Before reacting, give yourself some space. Try taking a few slow breaths and stepping away from the situation for a moment. You can deal with the problem when you're calmer.",
                "🧘 You control your next move, even when you can't control what happened."
            );
        }

        if (ContainsAny(
            lowerText,
            "tired",
            "exhausted",
            "sleep"))
        {
            return (
                "Tired",
                "Your body and mind may need a break. Consider getting some rest, drinking some water, and stepping away from whatever is draining you.",
                "🌙 Rest isn't laziness. Sometimes taking a break is exactly what you need to keep going."
            );
        }

        if (ContainsAny(
            lowerText,
            "exam",
            "assignment",
            "school",
            "college",
            "test"))
        {
            return (
                "Academic Pressure",
                "Academic pressure can feel overwhelming. Try breaking your work into smaller sections and focusing on one section at a time. A short focused session is better than worrying about everything at once.",
                "🎓 You don't need to know everything today. Keep learning, keep showing up, and keep going."
            );
        }

        if (ContainsAny(
            lowerText,
            "lonely",
            "alone",
            "nobody"))
        {
            return (
                "Lonely",
                "Feeling alone can make everything feel heavier. If possible, reach out to someone you trust, even if it's just to talk for a few minutes.",
                "❤️ You matter, and you don't have to carry everything by yourself."
            );
        }

        if (ContainsAny(
            lowerText,
            "happy",
            "great",
            "good",
            "excited",
            "proud"))
        {
            return (
                "Happy",
                "That's great to hear! 🌱 Take a moment to appreciate what's going well and remember what helped you get here.",
                "🔥 Hold onto this feeling. You're allowed to be proud of yourself."
            );
        }

        return (
            "Neutral",
            "Thank you for putting your thoughts into words. Writing things down can help you understand what's happening inside your mind. Think about what you can control today and choose one small positive action.",
            "🌿 Keep going. You don't need to have everything figured out to move forward."
        );
    }

    private bool ContainsAny(
        string text,
        params string[] words)
    {
        return words.Any(text.Contains);
    }
}