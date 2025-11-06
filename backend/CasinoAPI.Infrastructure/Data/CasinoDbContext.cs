using CasinoAPI.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace CasinoAPI.Infrastructure.Data;

public class CasinoDbContext : DbContext
{
    public CasinoDbContext(DbContextOptions<CasinoDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<GameSession> GameSessions { get; set; }
    public DbSet<GameHistory> GameHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordSalt).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(1000.00m);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Transaction configuration
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId);
            entity.Property(e => e.TransactionType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.BalanceBefore).HasColumnType("decimal(18,2)");
            entity.Property(e => e.BalanceAfter).HasColumnType("decimal(18,2)");
            entity.Property(e => e.GameType).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(255);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedDate);
        });

        // GameSession configuration
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(e => e.SessionId);
            entity.Property(e => e.GameType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TotalWinnings).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(e => e.TotalLosses).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(e => e.TotalBets).HasDefaultValue(0);

            entity.HasOne(e => e.User)
                .WithMany(u => u.GameSessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.UserId);
        });

        // GameHistory configuration
        modelBuilder.Entity<GameHistory>(entity =>
        {
            entity.HasKey(e => e.GameHistoryId);
            entity.Property(e => e.GameType).IsRequired().HasMaxLength(20);
            entity.Property(e => e.BetAmount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.WinAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);

            entity.HasOne(e => e.Session)
                .WithMany(s => s.GameHistories)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.GameHistories)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.PlayedDate);
        });
    }
}
