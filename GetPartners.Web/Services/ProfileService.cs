using GetPartners.Web.Data;
using GetPartners.Web.DTOs;
using GetPartners.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GetPartners.Web.Services;

public class ProfileService
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public ProfileService(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<ProfileResponseDto?> GetProfileAsync(int userId)
    {
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null) return null;

        return MapToDto(profile);
    }

    public async Task<ProfileResponseDto?> CreateProfileAsync(int userId, CreateProfileDto dto)
    {
        // Prevent duplicate profiles
        bool exists = await _db.Profiles.AnyAsync(p => p.UserId == userId);
        if (exists) return null;

        var profile = new Profile
        {
            UserId = userId,
            Name = dto.Name,
            Age = dto.Age,
            City = dto.City,
            Education = dto.Education,
            Religion = dto.Religion,
            Bio = dto.Bio,
            Gender = dto.Gender,
            PhotoUrl = ""
        };

        // Save preference at the same time
        var preference = new Preference
        {
            UserId = userId,
            MinAge = dto.MinAge,
            MaxAge = dto.MaxAge,
            PreferredCity = dto.PreferredCity,
            PreferredReligion = dto.PreferredReligion,
            LookingForGender = dto.LookingForGender
        };

        _db.Profiles.Add(profile);
        _db.Preferences.Add(preference);
        await _db.SaveChangesAsync();

        return MapToDto(profile);
    }

    public async Task<ProfileResponseDto?> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var profile = await _db.Profiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null) return null;

        // Only update fields that were provided
        if (!string.IsNullOrEmpty(dto.Name)) profile.Name = dto.Name;
        if (dto.Age > 0) profile.Age = dto.Age;
        if (!string.IsNullOrEmpty(dto.City)) profile.City = dto.City;
        if (!string.IsNullOrEmpty(dto.Education)) profile.Education = dto.Education;
        if (!string.IsNullOrEmpty(dto.Religion)) profile.Religion = dto.Religion;
        if (!string.IsNullOrEmpty(dto.Bio)) profile.Bio = dto.Bio;

        await _db.SaveChangesAsync();
        return MapToDto(profile);
    }

    public async Task<string?> UploadPhotoAsync(int userId, IFormFile photo)
{
    var profile = await _db.Profiles
        .FirstOrDefaultAsync(p => p.UserId == userId);

    if (profile == null) return null;

    // Validate file type
    string[] allowed = { ".jpg", ".jpeg", ".png", ".webp" };
    string ext = Path.GetExtension(photo.FileName).ToLower();
    if (!allowed.Contains(ext)) return null;

    // Fallback if WebRootPath is null
    string webRoot = _env.WebRootPath
        ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

    string fileName = $"user_{userId}_{Guid.NewGuid()}{ext}";
    string uploadsFolder = Path.Combine(webRoot, "images");

    Directory.CreateDirectory(uploadsFolder);

    string filePath = Path.Combine(uploadsFolder, fileName);

    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await photo.CopyToAsync(stream);
    }

    profile.PhotoUrl = $"/images/{fileName}";
    await _db.SaveChangesAsync();

    return profile.PhotoUrl;
}

    private static ProfileResponseDto MapToDto(Profile profile) => new()
    {
        UserId = profile.UserId,
        Name = profile.Name,
        Age = profile.Age,
        City = profile.City,
        Education = profile.Education,
        Religion = profile.Religion,
        Bio = profile.Bio,
        PhotoUrl = profile.PhotoUrl,
        Gender = profile.Gender
    };
}