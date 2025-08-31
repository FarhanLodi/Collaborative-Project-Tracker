using Microsoft.EntityFrameworkCore;
using ProjectTracker.Domain;
using ProjectTracker.Domain.Repositories;

namespace ProjectTracker.Infrastructure.Repositories;

public class AttachmentRepository : IAttachmentRepository
{
    private readonly ApplicationDbContext dbContext;

    public AttachmentRepository(ApplicationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<TaskAttachment>> ListAsync(Guid projectId, Guid taskId, Guid userId, CancellationToken cancellationToken = default)
    {
        var canAccess = await dbContext.Projects.AnyAsync(p => p.Id == projectId && (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)), cancellationToken);
        if (!canAccess) return new List<TaskAttachment>();
        return await dbContext.TaskAttachments.Where(a => a.TaskItemId == taskId).ToListAsync(cancellationToken);
    }

    public async Task<TaskAttachment?> AddAsync(Guid projectId, Guid taskId, Guid userId, TaskAttachment attachment, CancellationToken cancellationToken = default)
    {
        var canAccess = await dbContext.Projects.AnyAsync(p => p.Id == projectId && (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)), cancellationToken);
        if (!canAccess) return null;
        dbContext.TaskAttachments.Add(attachment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return attachment;
    }

    public async Task<TaskAttachment?> GetAsync(Guid projectId, Guid taskId, Guid attachmentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var canAccess = await dbContext.Projects.AnyAsync(p => p.Id == projectId && (p.OwnerId == userId || p.Members.Any(m => m.UserId == userId)), cancellationToken);
        if (!canAccess) return null;
        return await dbContext.TaskAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId && a.TaskItemId == taskId, cancellationToken);
    }
}


