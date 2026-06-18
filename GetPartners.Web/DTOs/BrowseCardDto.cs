namespace GetPartners.Web.DTOs;

public class BrowseCardDto
{
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string City { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public string Religion { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
}