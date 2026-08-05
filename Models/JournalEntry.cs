namespace MindEase.Models;

public class JournalEntry
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime Date { get; set; } = DateTime.Now;
}