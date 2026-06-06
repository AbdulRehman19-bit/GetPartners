namespace GetPartners.Web.Models;

public class Match
{
    public int Id { get; set; }
    public int User1Id { get; set; }
    public int User2Id { get; set; }
    public bool User1Liked { get; set; } = false;
    public bool User2Liked { get; set; } = false;
    public bool IsMatch { get; set; } = false;
    public DateTime? MatchedAt { get; set; }

    public User User1 { get; set; } = null!;
    public User User2 { get; set; } = null!;
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}