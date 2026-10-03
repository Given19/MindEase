using MindEase.Services;

namespace MindEase.Views;

public partial class CommunityPage : ContentPage
{
    private readonly ApiService _apiService;

    public CommunityPage(ApiService apiService)
    {
        InitializeComponent();

        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadPosts();
    }

    private async Task LoadPosts()
    {
        var posts = await _apiService.GetCommunityPostsAsync();

        var displayItems = posts
            .Select(p => new PostDisplayItem
            {
                Id = p.Id,
                Content = p.Content,
                Meta = $"{p.DisplayName} • {FormatRelativeTime(p.CreatedAt)}",
                CommentCountText = p.CommentCount == 1 ? "1 comment" : $"{p.CommentCount} comments"
            })
            .ToList();

        PostsCollection.ItemsSource = displayItems;
    }

    private async void PostsRefreshView_Refreshing(object? sender, EventArgs e)
    {
        await LoadPosts();
        PostsRefreshView.IsRefreshing = false;
    }

    private async void PostButton_Clicked(object? sender, EventArgs e)
    {
        string content = NewPostEditor.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(content))
        {
            await DisplayAlertAsync("Empty Post", "Write something before posting.", "OK");
            return;
        }

        var (success, message) = await _apiService.CreateCommunityPostAsync(content, AnonymousCheckBox.IsChecked);

        if (!success)
        {
            await DisplayAlertAsync("Error", message, "OK");
            return;
        }

        NewPostEditor.Text = string.Empty;
        AnonymousCheckBox.IsChecked = false;

        await LoadPosts();
    }

    private async void ViewPost_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not int postId)
            return;

        await Shell.Current.GoToAsync($"{nameof(CommunityPostDetailPage)}?postId={postId}");
    }

    private async void ReportPost_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not int postId)
            return;

        bool confirm = await DisplayAlertAsync(
            "Report Post",
            "Report this post for review? Posts reported multiple times are automatically hidden.",
            "Report",
            "Cancel");

        if (!confirm)
            return;

        bool success = await _apiService.ReportPostAsync(postId);

        await DisplayAlertAsync(
            success ? "Reported" : "Error",
            success ? "Thank you for helping keep the community safe." : "Could not report this post.",
            "OK");
    }

    private static string FormatRelativeTime(DateTime utc)
    {
        var span = DateTime.UtcNow - utc.ToUniversalTime();

        if (span.TotalMinutes < 1) return "just now";
        if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes}m ago";
        if (span.TotalHours < 24) return $"{(int)span.TotalHours}h ago";
        return $"{(int)span.TotalDays}d ago";
    }

    private class PostDisplayItem
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Meta { get; set; } = string.Empty;
        public string CommentCountText { get; set; } = string.Empty;
    }
}