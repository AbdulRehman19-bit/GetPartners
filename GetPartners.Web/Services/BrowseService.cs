using GetPartners.Web.Data;
using GetPartners.Web.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GetPartners.Web.Services;

public class BrowseService
{
    private readonly AppDbContext _db;

    public BrowseService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<BrowseCardDto>> GetProfilesAsync(int currentUserId)
    {
        // Get current user's preferences
        var preference = await _db.Preferences
            .FirstOrDefaultAsync(p => p.UserId == currentUserId);

        // Get current user's gender for reverse filtering
        var myProfile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == currentUserId);

        // Get all userIds this user has already interacted with
        // so we never show them again
        var seenUserIds = await _db.Matches
            .Where(m => m.User1Id == currentUserId || m.User2Id == currentUserId)
            .Select(m => m.User1Id == currentUserId ? m.User2Id : m.User1Id)
            .ToListAsync();

        // Always exclude yourself
        seenUserIds.Add(currentUserId);

        // Base query - join Profiles with Users
        var query = _db.Profiles
            .Where(p => !seenUserIds.Contains(p.UserId));

        // Apply preference filters if preferences exist
        if (preference != null)
        {
            if (preference.MinAge > 0)
                query = query.Where(p => p.Age >= preference.MinAge);

            if (preference.MaxAge > 0)
                query = query.Where(p => p.Age <= preference.MaxAge);

            if (!string.IsNullOrEmpty(preference.PreferredCity))
                query = query.Where(p => p.City == preference.PreferredCity);

            if (!string.IsNullOrEmpty(preference.PreferredReligion))
                query = query.Where(p => p.Religion == preference.PreferredReligion);

            if (!string.IsNullOrEmpty(preference.LookingForGender))
                query = query.Where(p => p.Gender == preference.LookingForGender);
        }

        // Return max 20 profiles at a time
        var profiles = await query
            .Take(20)
            .Select(p => new BrowseCardDto
            {
                UserId = p.UserId,
                Name = p.Name,
                Age = p.Age,
                City = p.City,
                Education = p.Education,
                Religion = p.Religion,
                Bio = p.Bio,
                PhotoUrl = p.PhotoUrl,
                Gender = p.Gender
            })
            .ToListAsync();

        return profiles;
    }
}