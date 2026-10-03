using MindEase.Data;
using MindEase.Models;
using MindEase.Services;

namespace MindEase.Views;

public partial class MoodCheckInPage : ContentPage
{
    private readonly AppDatabase _database;
    private readonly ApiService _apiService;

    private string _selectedMood = string.Empty;
    private readonly List<ChatMessageDto> _conversation = new();
    private Border? _typingBubble;

    private static readonly Dictionary<string, string> FollowUpQuestions = new()
    {
        { "Happy", "That's lovely to hear. What made you happy today?" },
        { "Sad", "I'm sorry you're feeling that way. What's been weighing on you?" },
        { "Anxious", "That sounds tough. What's making you feel anxious right now?" },
        { "Angry", "That's a valid feeling. What triggered it?" },
        { "Tired", "Makes sense. What's been draining your energy lately?" },
        { "Neutral", "Thanks for checking in. Anything on your mind today, even if it's small?" }
    };

    public MoodCheckInPage(AppDatabase database, ApiService apiService)
    {
        InitializeComponent();

        _database = database;
        _apiService = apiService;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        MessagesStack.Children.Clear();
        _conversation.Clear();
        _selectedMood = string.Empty;

        MoodChipsLayout.IsVisible = true;
        ReplyInputLayout.IsVisible = false;
        FinishButton.IsVisible = false;

        AddBotBubble("Hi there 🌿 How are you feeling right now?");
    }

    private void MoodChip_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not string mood)
            return;

        _selectedMood = mood;

        AddUserBubble(button.Text);

        MoodChipsLayout.IsVisible = false;

        string question = FollowUpQuestions.TryGetValue(mood, out var q)
            ? q
            : "What's on your mind?";

        AddBotBubble(question);
        _conversation.Add(new ChatMessageDto { Role = "assistant", Text = question });

        ReplyInputLayout.IsVisible = true;
        FinishButton.IsVisible = true;

        ReplyEntry.Focus();
    }

    private async void SendButton_Clicked(object? sender, EventArgs e)
    {
        string answer = ReplyEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(answer))
        {
            await DisplayAlertAsync("Empty Reply", "Type something before sending.", "OK");
            return;
        }

        AddUserBubble(answer);
        _conversation.Add(new ChatMessageDto { Role = "user", Text = answer });

        ReplyEntry.Text = string.Empty;
        SendButton.IsEnabled = false;
        ReplyEntry.IsEnabled = false;

        ShowTypingBubble();

        var (success, reply) = await _apiService.GetChatReplyAsync(_selectedMood, _conversation);

        RemoveTypingBubble();

        AddBotBubble(reply);

        if (success)
            _conversation.Add(new ChatMessageDto { Role = "assistant", Text = reply });

        SendButton.IsEnabled = true;
        ReplyEntry.IsEnabled = true;
        ReplyEntry.Focus();
    }

    private async void FinishButton_Clicked(object? sender, EventArgs e)
    {
        if (UserSession.IsLoggedIn && UserSession.CurrentUser is not null)
        {
            string transcript = string.Join(
                "\n",
                _conversation.Select(m => $"{(m.Role == "user" ? "You" : "MindEase")}: {m.Text}"));

            var entry = new MoodEntry
            {
                UserId = UserSession.CurrentUser.Id,
                Mood = _selectedMood,
                Notes = transcript,
                Date = DateTime.Now
            };

            try
            {
                await _database.SaveMoodAsync(entry);
            }
            catch
            {
                // Saving locally best-effort; conversation still ends gracefully either way.
            }
        }

        await Shell.Current.GoToAsync(nameof(HomePage));
    }

    private void ShowTypingBubble()
    {
        _typingBubble = new Border
        {
            BackgroundColor = Color.FromArgb("#E4EFE9"),
            Padding = new Thickness(14, 10),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = new CornerRadius(16, 16, 16, 4)
            },
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = "...",
                TextColor = Color.FromArgb("#1E2B24"),
                FontAttributes = FontAttributes.Italic,
                FontSize = 15
            }
        };

        MessagesStack.Children.Add(_typingBubble);
        ScrollToLatest(_typingBubble);
    }

    private void RemoveTypingBubble()
    {
        if (_typingBubble is not null)
        {
            MessagesStack.Children.Remove(_typingBubble);
            _typingBubble = null;
        }
    }

    private void AddBotBubble(string text)
    {
        var bubble = new Border
        {
            BackgroundColor = Color.FromArgb("#E4EFE9"),
            Padding = new Thickness(14, 10),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = new CornerRadius(16, 16, 16, 4)
            },
            HorizontalOptions = LayoutOptions.Start,
            MaximumWidthRequest = 260,
            Content = new Label
            {
                Text = text,
                TextColor = Color.FromArgb("#1E2B24"),
                FontSize = 15
            }
        };

        MessagesStack.Children.Add(bubble);
        ScrollToLatest(bubble);
    }

    private void AddUserBubble(string text)
    {
        var bubble = new Border
        {
            BackgroundColor = Color.FromArgb("#2F8F6E"),
            Padding = new Thickness(14, 10),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = new CornerRadius(16, 16, 4, 16)
            },
            HorizontalOptions = LayoutOptions.End,
            MaximumWidthRequest = 260,
            Content = new Label
            {
                Text = text,
                TextColor = Colors.White,
                FontSize = 15
            }
        };

        MessagesStack.Children.Add(bubble);
        ScrollToLatest(bubble);
    }

    private async void ScrollToLatest(View bubble)
    {
        await Task.Delay(50);
        await ChatScrollView.ScrollToAsync(bubble, ScrollToPosition.End, true);
    }
}