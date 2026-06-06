using System.ComponentModel.DataAnnotations;

namespace GetPartners.Web.DTOs;

public class CreateProfileDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(18, 80)]
    public int Age { get; set; }

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string Education { get; set; } = string.Empty;

    [Required]
    public string Religion { get; set; } = string.Empty;

    public string Bio { get; set; } = string.Empty;

    [Required]
    public string Gender { get; set; } = string.Empty;

    // Preferences
    public int MinAge { get; set; } = 18;
    public int MaxAge { get; set; } = 50;
    public string PreferredCity { get; set; } = string.Empty;
    public string PreferredReligion { get; set; } = string.Empty;
    public string LookingForGender { get; set; } = string.Empty;
}

public class UpdateProfileDto
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
    public string City { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public string Religion { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}

public class ProfileResponseDto
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