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

    // Called when a user opens a chat window
    // They join a SignalR group named "match_{matchId}"
    // so messages are scoped to only that conversation
    public async Task JoinMatch(int matchId)
    {
        string groupName = $"match_{matchId}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    // Called when a user leaves the chat window
    public async Task LeaveMatch(int matchId)
    {
        string groupName = $"match_{matchId}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }

    // Called when user hits send on a message
    public async Task SendMessage(int matchId, string body)
    {
        // Get the senderId from the JWT claim embedded in the connection
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

        // Save the message to DB first
        var savedMessage = await _messageService.SaveMessageAsync(matchId, senderId, body);

        if (savedMessage == null)
        {
            await Clients.Caller.SendAsync("Error", "Not authorized for this match.");
            return;
        }

        // Broadcast to everyone in the group including the sender
        // so both screens update instantly
        string groupName = $"match_{matchId}";
        await Clients.Group(groupName).SendAsync("ReceiveMessage", savedMessage);
    }

    // Called automatically when a connection drops
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}