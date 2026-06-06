namespace GetPartners.Web.DTOs;

public class SendMessageDto
{
    public int MatchId { get; set; }
    public string Body { get; set; } = string.Empty;
}

public class MessageResponseDto
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
}