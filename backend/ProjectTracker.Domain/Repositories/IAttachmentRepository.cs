namespace ProjectTracker.Domain.Repositories;

public interface IAttachmentRepository
{
    Task<List<TaskAttachment>> ListAsync(Guid projectId, Guid taskId, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskAttachment?> AddAsync(Guid projectId, Guid taskId, Guid userId, TaskAttachment attachment, CancellationToken cancellationToken = default);
    Task<TaskAttachment?> GetAsync(Guid projectId, Guid taskId, Guid attachmentId, Guid userId, CancellationToken cancellationToken = default);
}


