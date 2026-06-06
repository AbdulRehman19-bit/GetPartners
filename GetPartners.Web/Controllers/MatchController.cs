using GetPartners.Web.DTOs;
using GetPartners.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetPartners.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MatchController : ControllerBase
{
    private readonly MatchService _matchService;

    public MatchController(MatchService matchService)
    {
        _matchService = matchService;
    }

    // POST /api/match/like
    [HttpPost("like")]
    public async Task<IActionResult> Like([FromBody] LikeDto dto)
    {
        int currentUserId = TokenHelper.GetUserId(User);

        if (dto.TargetUserId == currentUserId)
            return BadRequest(new { message = "You cannot like yourself." });

        var result = await _matchService.LikeAsync(currentUserId, dto.TargetUserId);
        return Ok(result);
    }

    // POST /api/match/pass
    [HttpPost("pass")]
    public async Task<IActionResult> Pass([FromBody] LikeDto dto)
    {
        int currentUserId = TokenHelper.GetUserId(User);
        await _matchService.PassAsync(currentUserId, dto.TargetUserId);
        return Ok(new { message = "Passed." });
    }

    // GET /api/match/list - get all your mutual matches
    [HttpGet("list")]
    public async Task<IActionResult> GetMatches()
    {
        int currentUserId = TokenHelper.GetUserId(User);
        var matches = await _matchService.GetMyMatchesAsync(currentUserId);
        return Ok(matches);
    }
}