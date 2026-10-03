namespace MindEase.Views;

public partial class BreathingPage : ContentPage
{
    private int _seconds = 60;
    private bool _running;

    public BreathingPage()
    {
        InitializeComponent();
    }

    private async void StartButton_Clicked(object? sender, EventArgs e)
    {
        if (_running)
            return;

        _running = true;
        StartButton.IsEnabled = false;

        while (_seconds > 0)
        {
            InstructionLabel.Text = "Breathe in... 🌿";
            await Task.Delay(4000);

            if (_seconds <= 0)
                break;

            InstructionLabel.Text = "Hold... 🫁";
            await Task.Delay(2000);

            if (_seconds <= 0)
                break;

            InstructionLabel.Text = "Breathe out... 😌";
            await Task.Delay(4000);

            _seconds -= 10;

            if (_seconds < 0)
                _seconds = 0;

            TimerLabel.Text = _seconds.ToString();
        }

        InstructionLabel.Text = "Well done! Take a moment to relax. ❤️";
        StartButton.Text = "Completed";
        StartButton.IsEnabled = true;
        _running = false;
    }
}