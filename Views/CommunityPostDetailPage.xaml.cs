using MindEase.Services;

namespace MindEase.Views;

[QueryProperty(nameof(PostId), "postId")]
public partial class CommunityPostDetailPage : ContentPage
{
    private readonly ApiService _apiService;

    public int PostId { get; set; }

    public CommunityPostDetailPage(ApiService apiService)
    {
        InitializeComponent();

        _apiService = apiService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadPostAndComments();
    }

    private async Task LoadPostAndComments()
    {
        var posts = await _apiService.GetCommunityPostsAsync();
        var post = posts.FirstOrDefault(p => p.Id == PostId);

        if (post is not null)
        {
            PostContentLabel.Text = post.Content;
            PostMetaLabel.Text = $"{post.DisplayName} • {post.CreatedAt:dd MMM yyyy, HH:mm}";
        }

        var comments = await _apiService.GetCommentsAsync(PostId);

        var displayItems = comments
            .Select(c => new CommentDisplayItem
            {
                Id = c.Id,
                Content = c.Content,
                Meta = $"{c.DisplayName} • {c.CreatedAt:dd MMM yyyy, HH:mm}"
            })
            .ToList();

        CommentsCollection.ItemsSource = displayItems;
    }

    private async void SendComment_Clicked(object? sender, EventArgs e)
    {
        string content = NewCommentEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(content))
        {
            await DisplayAlertAsync("Empty Comment", "Write something before sending.", "OK");
            return;
        }

        var (success, message) = await _apiService.CreateCommentAsync(
            PostId, content, CommentAnonymousCheckBox.IsChecked);

        if (!success)
        {
            await DisplayAlertAsync("Error", message, "OK");
            return;
        }

        NewCommentEntry.Text = string.Empty;
        CommentAnonymousCheckBox.IsChecked = false;

        await LoadPostAndComments();
    }

    private async void ReportComment_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not int commentId)
            return;

        bool confirm = await DisplayAlertAsync(
            "Report Comment",
            "Report this comment for review?",
            "Report",
            "Cancel");

        if (!confirm)
            return;

        bool success = await _apiService.ReportCommentAsync(commentId);

        await DisplayAlertAsync(
            success ? "Reported" : "Error",
            success ? "Thank you for helping keep the community safe." : "Could not report this comment.",
            "OK");
    }

    private class CommentDisplayItem
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string Meta { get; set; } = string.Empty;
    }
}