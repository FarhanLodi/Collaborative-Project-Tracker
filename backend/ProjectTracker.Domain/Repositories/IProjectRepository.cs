namespace ProjectTracker.Domain.Repositories;

public interface IProjectRepository
{
    Task<List<Project>> GetMyProjectsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Project?> GetAccessibleProjectByIdAsync(Guid projectId, Guid userId, bool includeMembers = false, CancellationToken cancellationToken = default);
    Task<Project> AddAsync(Project project, CancellationToken cancellationToken = default);
    Task<bool> DeleteIfOwnerAsync(Guid projectId, Guid ownerId, CancellationToken cancellationToken = default);
    Task<Project?> JoinByInviteCodeAsync(string inviteCode, Guid userId, CancellationToken cancellationToken = default);
}


