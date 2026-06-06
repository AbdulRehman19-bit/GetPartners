namespace GetPartners.Web.Models;

public class Preference
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 50;
    public string PreferredCity { get; set; } = string.Empty;
    public string PreferredReligion { get; set; } = string.Empty;
    public string LookingForGender { get; set; } = string.Empty;

    public User User { get; set; } = null!;
}