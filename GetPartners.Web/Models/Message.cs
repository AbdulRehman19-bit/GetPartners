namespace GetPartners.Web.Models;

public class Message
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int SenderId { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;

    public Match Match { get; set; } = null!;
    public User Sender { get; set; } = null!;
}