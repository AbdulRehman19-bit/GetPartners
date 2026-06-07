using GetPartners.Web.Data;
using GetPartners.Web.DTOs;
using GetPartners.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GetPartners.Web.Services;

public class MessageService
{
    private readonly AppDbContext _db;

    public MessageService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<MessageResponseDto>> GetHistoryAsync(int matchId, int currentUserId)
    {
        bool isPartOfMatch = await _db.Matches
            .AnyAsync(m => m.Id == matchId &&
                (m.User1Id == currentUserId || m.User2Id == currentUserId));

        if (!isPartOfMatch) return new List<MessageResponseDto>();

        var messages = await _db.Messages
            .Where(m => m.MatchId == matchId)
            .OrderBy(m => m.SentAt)
            .Join(_db.Profiles,
                msg => msg.SenderId,
                profile => profile.UserId,
                (msg, profile) => new MessageResponseDto
                {
                    Id = msg.Id,
                    MatchId = msg.MatchId,
                    SenderId = msg.SenderId,
                    SenderName = profile.Name,
                    Body = msg.Body,
                    SentAt = msg.SentAt,
                    IsRead = msg.IsRead
                })
            .ToListAsync();

        var unread = await _db.Messages
            .Where(m => m.MatchId == matchId &&
                m.SenderId != currentUserId &&
                !m.IsRead)
            .ToListAsync();

        unread.ForEach(m => m.IsRead = true);
        await _db.SaveChangesAsync();

        return messages;
    }

    public async Task<MessageResponseDto?> SaveMessageAsync(int matchId, int senderId, string body)
    {
        bool isPartOfMatch = await _db.Matches
            .AnyAsync(m => m.Id == matchId &&
                m.IsMatch &&
                (m.User1Id == senderId || m.User2Id == senderId));

        if (!isPartOfMatch) return null;

        var message = new Message
        {
            MatchId = matchId,
            SenderId = senderId,
            Body = body,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _db.Messages.Add(message);
        await _db.SaveChangesAsync();

        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == senderId);

        return new MessageResponseDto
        {
            Id = message.Id,
            MatchId = message.MatchId,
            SenderId = message.SenderId,
            SenderName = profile?.Name ?? "Unknown",
            Body = message.Body,
            SentAt = message.SentAt,
            IsRead = message.IsRead
        };
    }
}