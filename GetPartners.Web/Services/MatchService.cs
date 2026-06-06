using GetPartners.Web.Data;
using GetPartners.Web.DTOs;
using GetPartners.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GetPartners.Web.Services;

public class MatchService
{
    private readonly AppDbContext _db;

    public MatchService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<MatchResultDto> LikeAsync(int currentUserId, int targetUserId)
    {
        // Check if the target already liked the current user first
        // meaning there is an existing Match row where target is User1
        // and current is User2 and target already liked
        var existingMatch = await _db.Matches
            .FirstOrDefaultAsync(m =>
                m.User1Id == targetUserId &&
                m.User2Id == currentUserId);

        if (existingMatch != null)
        {
            // Target already liked current user - this is a mutual match
            existingMatch.User2Liked = true;
            existingMatch.IsMatch = true;
            existingMatch.MatchedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new MatchResultDto
            {
                IsMatch = true,
                MatchId = existingMatch.Id,
                Message = "It's a match!"
            };
        }

        // Check if current user already liked target before
        // (prevent duplicate rows)
        bool alreadyLiked = await _db.Matches
            .AnyAsync(m => m.User1Id == currentUserId && m.User2Id == targetUserId);

        if (alreadyLiked)
        {
            return new MatchResultDto
            {
                IsMatch = false,
                MatchId = 0,
                Message = "Already liked this profile."
            };
        }

        // Create a new Match row - current user liked target
        var match = new Match
        {
            User1Id = currentUserId,
            User2Id = targetUserId,
            User1Liked = true,
            User2Liked = false,
            IsMatch = false
        };

        _db.Matches.Add(match);
        await _db.SaveChangesAsync();

        return new MatchResultDto
        {
            IsMatch = false,
            MatchId = match.Id,
            Message = "Like sent! Waiting for them to like back."
        };
    }

    public async Task<bool> PassAsync(int currentUserId, int targetUserId)
    {
        // Record the pass so this profile never shows again in browse
        bool alreadyExists = await _db.Matches
            .AnyAsync(m => m.User1Id == currentUserId && m.User2Id == targetUserId);

        if (alreadyExists) return false;

        var match = new Match
        {
            User1Id = currentUserId,
            User2Id = targetUserId,
            User1Liked = false,
            User2Liked = false,
            IsMatch = false
        };

        _db.Matches.Add(match);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<MatchedProfileDto>> GetMyMatchesAsync(int currentUserId)
    {
        // Get all mutual matches where current user is either User1 or User2
        var matches = await _db.Matches
            .Where(m => m.IsMatch &&
                (m.User1Id == currentUserId || m.User2Id == currentUserId))
            .ToListAsync();

        var result = new List<MatchedProfileDto>();

        foreach (var match in matches)
        {
            // The other person in the match
            int otherUserId = match.User1Id == currentUserId
                ? match.User2Id
                : match.User1Id;

            var profile = await _db.Profiles
                .FirstOrDefaultAsync(p => p.UserId == otherUserId);

            if (profile == null) continue;

            result.Add(new MatchedProfileDto
            {
                MatchId = match.Id,
                UserId = otherUserId,
                Name = profile.Name,
                PhotoUrl = profile.PhotoUrl,
                City = profile.City
            });
        }

        return result;
    }
}