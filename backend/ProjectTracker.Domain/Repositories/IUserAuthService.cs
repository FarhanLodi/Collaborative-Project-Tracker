namespace ProjectTracker.Domain.Repositories;

public interface IUserAuthService
{
    Task<ProjectTracker.Domain.ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ProjectTracker.Domain.ApplicationUser> CreateAsync(string email, string password, string? fullName, string? designation, CancellationToken cancellationToken = default);
    Task<bool> VerifyPasswordAsync(ProjectTracker.Domain.ApplicationUser user, string password, CancellationToken cancellationToken = default);
}


