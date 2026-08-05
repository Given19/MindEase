namespace MindEase.Models;

public class MoodEntry
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Mood { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.Now;
}