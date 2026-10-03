namespace MindEase.Models;

public class DiaryEntry
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = string.Empty;

    public string Mood { get; set; } = string.Empty;

    public string Suggestion { get; set; } = string.Empty;

    public string Motivation { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.Now;
}