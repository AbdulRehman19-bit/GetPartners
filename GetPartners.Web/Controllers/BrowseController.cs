using GetPartners.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetPartners.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BrowseController : ControllerBase
{
    private readonly BrowseService _browseService;

    public BrowseController(BrowseService browseService)
    {
        _browseService = browseService;
    }

    // GET /api/browse - returns filtered profile cards
    [HttpGet]
    public async Task<IActionResult> GetProfiles()
    {
        int currentUserId = TokenHelper.GetUserId(User);
        var profiles = await _browseService.GetProfilesAsync(currentUserId);

        if (!profiles.Any())
            return Ok(new { message = "No more profiles to show.", profiles });

        return Ok(profiles);
    }
}