using Microsoft.EntityFrameworkCore;
using RaceDay.API.Models;

namespace RaceDay.API.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<EventEnrolment> EventEnrolments => Set<EventEnrolment>();
    public DbSet<Result> Results => Set<Result>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Event>()
            .HasOne(x => x.Organiser)
            .WithMany(x => x.OrganisedEvents)
            .HasForeignKey(x => x.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Category>()
            .HasOne(x => x.Event)
            .WithMany(x => x.Categories)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EventEnrolment>()
            .HasIndex(x => new { x.ParticipantId, x.EventId })
            .IsUnique();

        modelBuilder.Entity<EventEnrolment>()
            .HasOne(x => x.Participant)
            .WithMany(x => x.Enrolments)
            .HasForeignKey(x => x.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EventEnrolment>()
            .HasOne(x => x.Event)
            .WithMany(x => x.Enrolments)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<EventEnrolment>()
            .HasOne(x => x.Category)
            .WithMany(x => x.Enrolments)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Result>()
            .HasOne(x => x.EventEnrolment)
            .WithOne(x => x.Result)
            .HasForeignKey<Result>(x => x.EventEnrolmentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>().Property(x => x.Role).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Event>().Property(x => x.EventType).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Event>()
    .Property(x => x.DistanceKm)
    .HasPrecision(6, 2);

        modelBuilder.Entity<Category>()
            .Property(x => x.DistanceKm)
            .HasPrecision(6, 2);
    }
}
