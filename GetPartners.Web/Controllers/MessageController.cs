using GetPartners.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GetPartners.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MessageController : ControllerBase
{
    private readonly MessageService _messageService;

    public MessageController(MessageService messageService)
    {
        _messageService = messageService;
    }

    // GET /api/message/{matchId} - load full chat history
    [HttpGet("{matchId:int}")]
    public async Task<IActionResult> GetHistory(int matchId)
    {
        int currentUserId = TokenHelper.GetUserId(User);
        var messages = await _messageService.GetHistoryAsync(matchId, currentUserId);

        if (!messages.Any())
            return Ok(new { message = "No messages yet. Say hello!", messages });

        return Ok(messages);
    }
}