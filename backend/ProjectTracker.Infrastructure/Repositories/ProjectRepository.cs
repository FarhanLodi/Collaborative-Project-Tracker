using Microsoft.EntityFrameworkCore;
using ProjectTracker.Domain;
using ProjectTracker.Domain.Repositories;

namespace ProjectTracker.Infrastructure.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext dbContext;

    public ProjectRepository(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<Project>> GetMyProjectsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Projects
            .Where(p => p.OwnerId == userId || p.Members.Any(m => m.UserId == userId))
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetAccessibleProjectByIdAsync(Guid projectId, Guid userId, bool includeMembers = false, CancellationToken cancellationToken = default)
    {
        IQueryable<Project> query = dbContext.Projects;
        if (includeMembers)
        {
            query = query
                .Include(p => p.Members)
                .ThenInclude(m => m.User);
        }
        return await query.FirstOrDefaultAsync(
            p => p.Id == projectId && (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)),
            cancellationToken);
    }

    public async Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        dbContext.Projects.Add(project);
        // Ensure owner is also present in members table with ProjectOwner role
        var ownerRoleId = await dbContext.Roles
            .Where(r => r.Name == "Project Owner")
            .Select(r => r.Id)
            .FirstAsync(cancellationToken);
        dbContext.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = project.Id,
            UserId = project.OwnerId,
            RoleId = ownerRoleId
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task<bool> DeleteIfOwnerAsync(Guid projectId, Guid ownerId, CancellationToken cancellationToken = default)
    {
        var project = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
        if (project is null || project.OwnerId != ownerId) return false;
        dbContext.Projects.Remove(project);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<Project?> JoinByInviteCodeAsync(string inviteCode, Guid userId, CancellationToken cancellationToken = default)
    {
        var project = await dbContext.Projects.FirstOrDefaultAsync(p => p.InviteCode == inviteCode, cancellationToken);
        if (project is null) return null;

        var exists = await dbContext.ProjectMembers.AnyAsync(m => m.ProjectId == project.Id && m.UserId == userId, cancellationToken);
        if (!exists)
        {
            var employeeRoleId = await dbContext.Roles
                .Where(r => r.Name == "Employee")
                .Select(r => r.Id)
                .FirstAsync(cancellationToken);
            dbContext.ProjectMembers.Add(new ProjectMember
            {
                ProjectId = project.Id,
                UserId = userId,
                RoleId = employeeRoleId
            });
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return project;
    }
}


