using Microsoft.EntityFrameworkCore;
using ProjectTracker.Domain;
using ProjectTracker.Domain.Repositories;

namespace ProjectTracker.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly ApplicationDbContext dbContext;

    public TaskRepository(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<TaskItem>> ListAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
    {
        var project = await dbContext.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
        if (project is null) return new List<TaskItem>();
        var isOwner = project.OwnerId == userId;
        var isMember = isOwner || project.Members.Any(m => m.UserId == userId);
        if (!isMember) return new List<TaskItem>();
        if (isOwner)
        {
            return await dbContext.TaskItems.Where(t => t.ProjectId == projectId).ToListAsync(cancellationToken);
        }
        return await dbContext.TaskItems.Where(t => t.ProjectId == projectId && t.AssigneeId == userId).ToListAsync(cancellationToken);
    }

    public async Task<TaskItem?> CreateAsync(Guid projectId, Guid userId, TaskItem task, CancellationToken cancellationToken = default)
    {
        // Only owner can create tasks
        var isOwner = await dbContext.Projects.AnyAsync(p => p.Id == projectId && p.OwnerId == userId, cancellationToken);
        if (!isOwner) return null;
        dbContext.TaskItems.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<TaskItem?> UpdateAsync(Guid projectId, Guid userId, Guid taskId, Action<TaskItem> update, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId, cancellationToken);
        if (task is null) return null;

        var project = await dbContext.Projects.FirstAsync(p => p.Id == projectId, cancellationToken);
        var isOwner = project.OwnerId == userId;
        if (!isOwner) return null;

        update(task);
        await dbContext.SaveChangesAsync(cancellationToken);
        return task;
    }

    public async Task<bool> DeleteAsync(Guid projectId, Guid userId, Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId, cancellationToken);
        if (task is null) return false;
        var project = await dbContext.Projects.FirstAsync(p => p.Id == projectId, cancellationToken);
        var isOwner = project.OwnerId == userId;
        if (!isOwner) return false;
        dbContext.TaskItems.Remove(task);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<TaskItem?> UpdateStatusAsync(Guid projectId, Guid userId, Guid taskId, Domain.Enums.TaskStatus status, CancellationToken cancellationToken = default)
    {
        // Allow any project member (or owner) to move task between columns
        var project = await dbContext.Projects
            .Include(p => p.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
        if (project is null) return null;
        var isMember = project.OwnerId == userId || project.Members.Any(m => m.UserId == userId);
        if (!isMember) return null;

        var task = await dbContext.TaskItems.FirstOrDefaultAsync(t => t.Id == taskId && t.ProjectId == projectId, cancellationToken);
        if (task is null) return null;
        task.Status = status;
        await dbContext.SaveChangesAsync(cancellationToken);
        return task;
    }
}


