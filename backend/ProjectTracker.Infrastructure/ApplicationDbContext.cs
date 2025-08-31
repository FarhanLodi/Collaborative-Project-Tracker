using Microsoft.EntityFrameworkCore;
using ProjectTracker.Domain;

namespace ProjectTracker.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();
    public DbSet<TaskAttachment> TaskAttachments => Set<TaskAttachment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.UserName).HasMaxLength(256);
            b.Property(u => u.Email).HasMaxLength(256);
            b.Property(u => u.FullName).HasMaxLength(200);
            b.Property(u => u.Designation).HasMaxLength(100);
        });

        builder.Entity<Role>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Name).HasMaxLength(100).IsRequired();
            b.HasIndex(r => r.Name).IsUnique();
        });

        builder.Entity<ProjectMember>().HasKey(pm => new { pm.ProjectId, pm.UserId });
        builder.Entity<Project>()
            .HasOne(p => p.Owner)
            .WithMany()
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Project>()
            .HasIndex(p => p.InviteCode)
            .IsUnique();

        builder.Entity<TaskItem>()
            .HasOne(t => t.Assignee)
            .WithMany()
            .HasForeignKey(t => t.AssigneeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<TaskAttachment>()
            .Property(a => a.SizeBytes)
            .HasConversion<long>();

        builder.Entity<ProjectMember>(b =>
        {
            b.HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId);
            b.HasOne(pm => pm.User)
                .WithMany()
                .HasForeignKey(pm => pm.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(pm => pm.Role)
                .WithMany()
                .HasForeignKey(pm => pm.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Seed roles
        var projectOwnerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var employeeId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        builder.Entity<Role>().HasData(
            new Role { Id = projectOwnerId, Name = "Project Owner" },
            new Role { Id = employeeId, Name = "Employee" }
        );
    }
}
