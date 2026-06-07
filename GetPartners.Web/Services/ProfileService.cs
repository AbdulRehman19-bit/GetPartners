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

        // Add this using at the top of ProfileService.cs
using Supabase;

// Add Supabase client to constructor
private readonly AppDbContext _db;
private readonly Supabase.Client _supabase;

public ProfileService(AppDbContext db, Supabase.Client supabase)
{
    _db = db;
    _supabase = supabase;
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

    // Read file into byte array
    using var ms = new MemoryStream();
    await photo.CopyToAsync(ms);
    byte[] fileBytes = ms.ToArray();

    // Build unique file path inside the bucket
    string fileName = $"user_{userId}_{Guid.NewGuid()}{ext}";
    string bucketPath = $"profile-photos/{fileName}";

    // Upload to Supabase Storage
    await _supabase.Storage
        .From("profile-photos")
        .Upload(fileBytes, fileName, new Supabase.Storage.FileOptions
        {
            ContentType = photo.ContentType,
            Upsert = true
        });

    // Build the public URL
    string publicUrl = _supabase.Storage
        .From("profile-photos")
        .GetPublicUrl(fileName);

    // Save URL to DB
    profile.PhotoUrl = publicUrl;
    await _db.SaveChangesAsync();

    return publicUrl;
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