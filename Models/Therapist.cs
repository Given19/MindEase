using SQLite;

namespace MindEase.Models;

public class Therapist
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Specialty { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string OfficeAddress { get; set; } = string.Empty;

    public double PriceInPerson { get; set; }

    public double PriceOnline { get; set; }

    public double PricePhone { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}