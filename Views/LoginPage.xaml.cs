using MindEase.Services;

namespace MindEase.Views;

public partial class LoginPage : ContentPage
{
    private readonly ApiService _apiService;

    public LoginPage(ApiService apiService)
    {
        InitializeComponent();

        _apiService = apiService;
    }

    private async void LoginButton_Clicked(object? sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim() ?? string.Empty;
        string password = PasswordEntry.Text ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlertAsync("Error", "Please enter your email and password.", "OK");
            return;
        }

        SetLoading(true);

        var (success, message, user) = await _apiService.LoginAsync(email, password);

        SetLoading(false);

        if (!success || user is null)
        {
            await DisplayAlertAsync("Login Failed", message, "OK");
            return;
        }

        UserSession.Login(user);

        await Shell.Current.GoToAsync(nameof(HomePage));
    }

    private async void RegisterButton_Clicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(RegisterPage));
    }

    private void SetLoading(bool isLoading)
    {
        LoginButton.IsEnabled = !isLoading;
        LoginButton.Text = isLoading ? "" : "Login";
        RegisterButton.IsEnabled = !isLoading;
        EmailEntry.IsEnabled = !isLoading;
        PasswordEntry.IsEnabled = !isLoading;

        LoginSpinner.IsRunning = isLoading;
        LoginSpinner.IsVisible = isLoading;

        LoadingHintLabel.IsVisible = isLoading;
    }
}