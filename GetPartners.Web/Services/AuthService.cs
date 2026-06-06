using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GetPartners.Web.Data;
using GetPartners.Web.DTOs;
using GetPartners.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace GetPartners.Web.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<string?> RegisterAsync(RegisterDto dto)
    {
        // Check if email already exists
        bool emailExists = await _db.Users.AnyAsync(u => u.Email == dto.Email);
        if (emailExists) return null;

        // Hash the password with BCrypt
        string hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        // Create and save the new user
        var user = new User
        {
            Email = dto.Email,
            PasswordHash = hash
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Return a JWT token
        return GenerateToken(user);
    }

    public async Task<string?> LoginAsync(LoginDto dto)
    {
        // Find user by email
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null) return null;

        // Verify password against stored hash
        bool passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!passwordValid) return null;

        return GenerateToken(user);
    }

    private string GenerateToken(User user)
    {
        // Read JWT config from appsettings.json
        string key = _config["Jwt:Key"]!;
        string issuer = _config["Jwt:Issuer"]!;
        string audience = _config["Jwt:Audience"]!;

        // Claims are data embedded inside the token
        // The frontend can read userId from the token without hitting the DB
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}