namespace ProjectTracker.Domain.Repositories;

public interface ITaskRepository
{
    Task<List<TaskItem>> ListAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default);
    Task<TaskItem?> CreateAsync(Guid projectId, Guid userId, TaskItem task, CancellationToken cancellationToken = default);
    Task<TaskItem?> UpdateAsync(Guid projectId, Guid userId, Guid taskId, Action<TaskItem> update, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid projectId, Guid userId, Guid taskId, CancellationToken cancellationToken = default);
    Task<TaskItem?> UpdateStatusAsync(Guid projectId, Guid userId, Guid taskId, Enums.TaskStatus status, CancellationToken cancellationToken = default);
}


