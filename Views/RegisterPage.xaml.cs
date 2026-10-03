using MindEase.Services;

namespace MindEase.Views;

public partial class RegisterPage : ContentPage
{
    private readonly ApiService _apiService;

    public RegisterPage(ApiService apiService)
    {
        InitializeComponent();

        _apiService = apiService;
    }

    private async void CreateAccountButton_Clicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) ||
            string.IsNullOrWhiteSpace(LastNameEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
            string.IsNullOrWhiteSpace(ConfirmPasswordEntry.Text))
        {
            await DisplayAlertAsync("Error", "Please fill in all fields.", "OK");
            return;
        }

        if (PasswordEntry.Text != ConfirmPasswordEntry.Text)
        {
            await DisplayAlertAsync("Error", "Passwords do not match.", "OK");
            return;
        }

        var (success, message, _) = await _apiService.RegisterAsync(
            FirstNameEntry.Text,
            LastNameEntry.Text,
            EmailEntry.Text,
            PasswordEntry.Text);

        if (success)
        {
            await DisplayAlertAsync("Success", message, "OK");
            await Shell.Current.GoToAsync(nameof(LoginPage));
        }
        else
        {
            await DisplayAlertAsync("Registration Failed", message, "OK");
        }
    }

    private async void BackToLoginButton_Clicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LoginPage));
    }
}