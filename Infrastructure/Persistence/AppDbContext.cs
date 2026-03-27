using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<WorkTask> Tasks => Set<WorkTask>();
    public DbSet<TaskHistory> TaskHistories => Set<TaskHistory>();
    public DbSet<TaskComment> TaskComments => Set<TaskComment>();
    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<WorkTask>(entity =>
        {
            entity.ToTable("Tasks");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.TaskNumber).IsUnique();
            entity.Property(x => x.Description).IsRequired();
            entity.Property(x => x.StatusText).HasMaxLength(250);
        });

        builder.Entity<TaskHistory>(entity =>
        {
            entity.ToTable("TaskHistories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OldValues).HasColumnType("jsonb");
            entity.Property(x => x.NewValues).HasColumnType("jsonb");
        });

        builder.Entity<TaskComment>(entity =>
        {
            entity.ToTable("TaskComments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Text).IsRequired();
        });

        builder.Entity<Equipment>(entity =>
        {
            entity.ToTable("Equipments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        builder.Entity<Club>(entity =>
        {
            entity.ToTable("Clubs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        builder.Entity<Zone>(entity =>
        {
            entity.ToTable("Zones");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.HasOne(x => x.Club).WithMany(x => x.Zones).HasForeignKey(x => x.ClubId);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Token).HasMaxLength(300).IsRequired();
            entity.HasIndex(x => x.Token).IsUnique();
            entity.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
        });
    }
}
