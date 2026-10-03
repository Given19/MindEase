using SQLite;

namespace MindEase.Models;

public class Appointment
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int UserId { get; set; }

    public int TherapistId { get; set; }

    public DateTime AppointmentDate { get; set; }

    public string SessionType { get; set; } = "In-Person";

    public double Price { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";
}