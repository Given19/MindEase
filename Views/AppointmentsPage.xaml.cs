using MindEase.Data;
using MindEase.Services;

namespace MindEase.Views;

public partial class AppointmentsPage : ContentPage
{
    private readonly ApiService _apiService;
    private List<TherapistDto> _therapists = new();
    private TherapistDto? _selectedTherapist;
    private string _selectedSessionType = "In-Person";

    public AppointmentsPage(AppDatabase database, ApiService apiService)
    {
        InitializeComponent();

        _apiService = apiService;

        AppointmentDatePicker.MinimumDate = DateTime.Today;
        AppointmentDatePicker.Date = DateTime.Today;
        AppointmentTimePicker.Time = DateTime.Now.TimeOfDay;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadNearbyTherapists();
        await LoadAppointments();
    }

    private async void RefreshLocation_Clicked(object? sender, EventArgs e)
    {
        await LoadNearbyTherapists();
    }

    private async Task LoadNearbyTherapists()
    {
        LocationStatusLabel.Text = "Finding your location...";

        Location? location = null;

        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
            {
                location = await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Location error: {ex.Message}");
        }

        _therapists = await _apiService.GetNearbyTherapistsAsync(
            location?.Latitude, location?.Longitude);

        LocationStatusLabel.Text = location is null
            ? "Couldn't get your location — showing all therapists."
            : "Showing therapists sorted by distance from you.";

        var displayItems = _therapists
            .Select(t => new TherapistDisplayItem
            {
                Id = t.Id,
                Name = t.Name,
                Specialty = t.Specialty,
                OfficeAddress = t.OfficeAddress,
                DistanceText = t.DistanceKm.HasValue
                    ? $"📍 {t.DistanceKm.Value:0.#} km away"
                    : "Distance unavailable",
                BorderColor = Colors.LightGray
            })
            .ToList();

        NearbyTherapistsCollection.ItemsSource = displayItems;
    }

    private void TherapistCard_Tapped(object? sender, EventArgs e)
    {
        if (sender is not Border border || border.GestureRecognizers.FirstOrDefault() is not TapGestureRecognizer tap)
            return;

        if (tap.CommandParameter is not int therapistId)
            return;

        _selectedTherapist = _therapists.FirstOrDefault(t => t.Id == therapistId);

        if (_selectedTherapist is null)
            return;

        SelectedTherapistLabel.Text = $"Booking with {_selectedTherapist.Name}";
        BioLabel.Text = _selectedTherapist.Bio;

        InPersonPriceLabel.Text = $"R{_selectedTherapist.PriceInPerson:0}";
        OnlinePriceLabel.Text = $"R{_selectedTherapist.PriceOnline:0}";
        PhonePriceLabel.Text = $"R{_selectedTherapist.PricePhone:0}";

        SetSelectedSessionType("In-Person");

        BookingSection.IsVisible = true;
    }

    private void InPersonOption_Tapped(object? sender, EventArgs e) => SetSelectedSessionType("In-Person");
    private void OnlineOption_Tapped(object? sender, EventArgs e) => SetSelectedSessionType("Online");
    private void PhoneOption_Tapped(object? sender, EventArgs e) => SetSelectedSessionType("Phone");

    private void SetSelectedSessionType(string sessionType)
    {
        _selectedSessionType = sessionType;

        var selectedColor = Color.FromArgb("#2F8F6E");

        InPersonBorder.Stroke = sessionType == "In-Person" ? selectedColor : Colors.LightGray;
        OnlineBorder.Stroke = sessionType == "Online" ? selectedColor : Colors.LightGray;
        PhoneBorder.Stroke = sessionType == "Phone" ? selectedColor : Colors.LightGray;
    }

    private double GetPriceForSelectedSession()
    {
        if (_selectedTherapist is null)
            return 0;

        return _selectedSessionType switch
        {
            "In-Person" => _selectedTherapist.PriceInPerson,
            "Online" => _selectedTherapist.PriceOnline,
            "Phone" => _selectedTherapist.PricePhone,
            _ => 0
        };
    }

    private async Task LoadAppointments()
    {
        if (!UserSession.IsLoggedIn)
        {
            AppointmentsCollection.ItemsSource = null;
            return;
        }

        var appointments = await _apiService.GetMyAppointmentsAsync();

        var displayItems = appointments
            .Select(a => new AppointmentDisplayItem
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status,
                CanCancel = a.Status is "Pending" or "Confirmed" && a.AppointmentDate > DateTime.Now,
                TherapistName = a.Therapist?.Name ?? "Unknown Therapist",
                SessionTypeAndPrice = $"{a.SessionType} — R{a.Price:0}"
            })
            .ToList();

        AppointmentsCollection.ItemsSource = displayItems;
    }

    private async void ReviewBooking_Clicked(object? sender, EventArgs e)
    {
        if (!UserSession.IsLoggedIn)
        {
            await DisplayAlertAsync("Not Logged In", "Please log in first.", "OK");
            return;
        }

        if (_selectedTherapist is null)
        {
            await DisplayAlertAsync("Select a Therapist", "Please choose a therapist from the list above.", "OK");
            return;
        }

        DateTime date = AppointmentDatePicker.Date ?? DateTime.Today;
        TimeSpan time = AppointmentTimePicker.Time ?? DateTime.Now.TimeOfDay;
        DateTime appointmentDateTime = date.Date + time;

        if (appointmentDateTime <= DateTime.Now)
        {
            await DisplayAlertAsync("Invalid Time", "Please choose a future date and time.", "OK");
            return;
        }

        double price = GetPriceForSelectedSession();

        string summary =
            $"Therapist: {_selectedTherapist.Name}\n" +
            $"Date: {appointmentDateTime:dddd, dd MMM yyyy}\n" +
            $"Time: {appointmentDateTime:HH:mm}\n" +
            $"Session: {_selectedSessionType}\n" +
            $"Fee: R{price:0} (pending therapist confirmation)";

        bool confirmed = await DisplayAlert("Appointment Summary", summary, "Request Appointment", "Cancel");

        if (!confirmed)
            return;

        var (success, message) = await _apiService.BookAppointmentAsync(
            _selectedTherapist.Id,
            appointmentDateTime,
            _selectedSessionType,
            price,
            MessageEditor.Text?.Trim() ?? "");

        if (!success)
        {
            await DisplayAlertAsync("Booking Failed", message, "OK");
            return;
        }

        MessageEditor.Text = "";
        BookingSection.IsVisible = false;
        _selectedTherapist = null;

        await LoadAppointments();

        await DisplayAlertAsync(
            "Request Sent 📨",
            "Your appointment request has been sent. You'll see the status update once the therapist responds.",
            "OK");
    }

    private async void CancelAppointment_Clicked(object? sender, EventArgs e)
    {
        if (sender is not Button button || button.CommandParameter is not int appointmentId)
            return;

        bool confirm = await DisplayAlertAsync(
            "Cancel Appointment",
            "Are you sure you want to cancel this appointment?",
            "Yes",
            "No");

        if (!confirm)
            return;

        await _apiService.CancelAppointmentAsync(appointmentId);

        await LoadAppointments();

        await DisplayAlertAsync("Cancelled", "Your appointment has been cancelled.", "OK");
    }

    private class TherapistDisplayItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string OfficeAddress { get; set; } = string.Empty;
        public string DistanceText { get; set; } = string.Empty;
        public Color BorderColor { get; set; } = Colors.LightGray;
    }

    private class AppointmentDisplayItem
    {
        public int Id { get; set; }
        public string TherapistName { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool CanCancel { get; set; }
        public string SessionTypeAndPrice { get; set; } = string.Empty;
    }
}