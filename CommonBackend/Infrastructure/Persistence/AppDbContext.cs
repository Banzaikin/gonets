using Microsoft.EntityFrameworkCore;
using CommonBackend.Domain.Entities;
using CommonBackend.Application.Interfaces;

namespace CommonBackend.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<IncomingMessage> IncomingMessages { get; set; }
    public DbSet<OutgoingMessage> OutgoingMessages { get; set; }
    public DbSet<SentMessage> SentMessages { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Конфигурация таблицы входящих сообщений
        modelBuilder.Entity<IncomingMessage>(entity =>
        {
            entity.HasKey(e => e.DbId);
            entity.Property(e => e.MessageId).IsRequired();
            entity.HasIndex(e => e.MessageId).IsUnique();

            entity.Property(e => e.From).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Time).IsRequired();
            entity.Property(e => e.TypeMessage).IsRequired().HasConversion<int>();
        });

        // Конфигурация таблицы исходящих сообщений
        modelBuilder.Entity<OutgoingMessage>(entity =>
        {
            entity.HasKey(e => e.DbId);
            entity.Property(e => e.MessageId).IsRequired();
            entity.HasIndex(e => e.MessageId).IsUnique();

            entity.Property(e => e.To).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(3000);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Time).IsRequired();
            entity.Property(e => e.TypeMessage).IsRequired().HasConversion<int>();
        });

        // Конфигурация таблицы отправленных сообщений
        modelBuilder.Entity<SentMessage>(entity =>
        {
            entity.HasKey(e => e.DbId);
            entity.Property(e => e.MessageId).IsRequired();
            entity.HasIndex(e => e.MessageId).IsUnique();

            entity.Property(e => e.To).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Content).IsRequired().HasMaxLength(3000);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Time).IsRequired();
            entity.Property(e => e.TypeMessage).IsRequired().HasConversion<int>();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
        });

        var passwordHash = BCrypt.Net.BCrypt.HashPassword("0000");
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                PasswordHash = passwordHash
            }
        );

    }
}