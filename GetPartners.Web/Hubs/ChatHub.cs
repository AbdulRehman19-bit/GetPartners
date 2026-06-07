using System.Security.Claims;
using GetPartners.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GetPartners.Web.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly MessageService _messageService;

    public ChatHub(MessageService messageService)
    {
        _messageService = messageService;
    }

    public async Task JoinMatch(int matchId)
    {
        string groupName = $"match_{matchId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task LeaveMatch(int matchId)
    {
        string groupName = $"match_{matchId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task SendMessage(int matchId, string body)
    {
        string? userIdStr = Context.User?
            .FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdStr == null)
        {
            await Clients.Caller.SendAsync("Error", "Unauthorized.");
            return;
        }

        int senderId = int.Parse(userIdStr);

        if (string.IsNullOrWhiteSpace(body))
        {
            await Clients.Caller.SendAsync("Error", "Message cannot be empty.");
            return;
        }

        var savedMessage = await _messageService
            .SaveMessageAsync(matchId, senderId, body);

        if (savedMessage == null)
        {
            await Clients.Caller.SendAsync("Error", "Not authorized for this match.");
            return;
        }

        string groupName = $"match_{matchId}";
        await Clients.Group(groupName).SendAsync("ReceiveMessage", savedMessage);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}