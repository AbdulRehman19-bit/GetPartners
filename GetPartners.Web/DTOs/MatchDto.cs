namespace GetPartners.Web.DTOs;

public class LikeDto
{
    public int TargetUserId { get; set; }
}

public class MatchResultDto
{
    public bool IsMatch { get; set; }
    public int MatchId { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class MatchedProfileDto
{
    public int MatchId { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}