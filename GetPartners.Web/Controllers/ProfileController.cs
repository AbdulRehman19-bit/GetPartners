using GetPartners.Web.DTOs;
using GetPartners.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetPartners.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly ProfileService _profileService;

    public ProfileController(ProfileService profileService)
    {
        _profileService = profileService;
    }

    // GET /api/profile - get your own profile
    [HttpGet]
    public async Task<IActionResult> GetMyProfile()
    {
        int userId = TokenHelper.GetUserId(User);
        var profile = await _profileService.GetProfileAsync(userId);

        if (profile == null)
            return NotFound(new { message = "Profile not found. Please create one." });

        return Ok(profile);
    }

    // GET /api/profile/{userId} - get any profile by userId (for browse cards)
    [HttpGet("{userId:int}")]
    public async Task<IActionResult> GetProfile(int userId)
    {
        var profile = await _profileService.GetProfileAsync(userId);

        if (profile == null)
            return NotFound(new { message = "Profile not found." });

        return Ok(profile);
    }

    // POST /api/profile - create your profile
    [HttpPost]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        int userId = TokenHelper.GetUserId(User);
        var profile = await _profileService.CreateProfileAsync(userId, dto);

        if (profile == null)
            return BadRequest(new { message = "Profile already exists." });

        return Ok(profile);
    }

    // PUT /api/profile - update your profile
    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
    {
        int userId = TokenHelper.GetUserId(User);
        var profile = await _profileService.UpdateProfileAsync(userId, dto);

        if (profile == null)
            return NotFound(new { message = "Profile not found." });

        return Ok(profile);
    }

    // POST /api/profile/photo - upload profile photo
    [HttpPost("photo")]
    public async Task<IActionResult> UploadPhoto(IFormFile photo)
    {
        if (photo == null || photo.Length == 0)
            return BadRequest(new { message = "No file uploaded." });

        // Limit file size to 5MB
        if (photo.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "File too large. Max 5MB." });

        int userId = TokenHelper.GetUserId(User);
        string? photoUrl = await _profileService.UploadPhotoAsync(userId, photo);

        if (photoUrl == null)
            return BadRequest(new { message = "Invalid file type or profile not found." });

        return Ok(new { photoUrl });
    }
}