using System.Diagnostics;

namespace MindEase.Views;

public partial class CrisisResourcesPage : ContentPage
{
    public CrisisResourcesPage()
    {
        InitializeComponent();
    }

    private async void CallSadag_Clicked(object? sender, EventArgs e)
    {
        await MakePhoneCall("0800567567");
    }

    private async void CallSuicideLine_Clicked(object? sender, EventArgs e)
    {
        await MakePhoneCall("0800567567");
    }

    private async void CallEmergency_Clicked(object? sender, EventArgs e)
    {
        await MakePhoneCall("10111");
    }

    private async Task MakePhoneCall(string number)
    {
        try
        {
            PhoneDialer.Open(number);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Dialer error: {ex.Message}");

            await DisplayAlertAsync(
                "Unable to Dial",
                $"Please dial {number} manually.",
                "OK");
        }
    }

    private async void BackToHome_Clicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(HomePage));
    }
}