using GetPartners.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GetPartners.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Profile> Profiles => Set<Profile>();
    public DbSet<Preference> Preferences => Set<Preference>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User -> Profile (1 to 1)
        modelBuilder.Entity<Profile>()
            .HasOne(p => p.User)
            .WithOne(u => u.Profile)
            .HasForeignKey<Profile>(p => p.UserId);

        // User -> Preference (1 to 1)
        modelBuilder.Entity<Preference>()
            .HasOne(p => p.User)
            .WithOne(u => u.Preference)
            .HasForeignKey<Preference>(p => p.UserId);

        // Match -> User1 (no cascade to avoid multiple cascade paths)
        modelBuilder.Entity<Match>()
            .HasOne(m => m.User1)
            .WithMany()
            .HasForeignKey(m => m.User1Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Match -> User2
        modelBuilder.Entity<Match>()
            .HasOne(m => m.User2)
            .WithMany()
            .HasForeignKey(m => m.User2Id)
            .OnDelete(DeleteBehavior.Restrict);

        // Message -> Sender (no cascade)
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Message -> Match
        modelBuilder.Entity<Message>()
            .HasOne(m => m.Match)
            .WithMany(m => m.Messages)
            .HasForeignKey(m => m.MatchId);

        // Unique email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}