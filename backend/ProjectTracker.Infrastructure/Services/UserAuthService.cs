using Microsoft.EntityFrameworkCore;
using ProjectTracker.Domain;
using ProjectTracker.Domain.Repositories;

namespace ProjectTracker.Infrastructure.Services;

public class UserAuthService : IUserAuthService
{
    private readonly ApplicationDbContext dbContext;
    private readonly Pbkdf2PasswordHasher hasher;

    public UserAuthService(ApplicationDbContext dbContext, Pbkdf2PasswordHasher hasher)
    {
        this.dbContext = dbContext;
        this.hasher = hasher;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<ApplicationUser> CreateAsync(string email, string password, string? fullName, string? designation, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FullName = fullName,
            Designation = designation,
            PasswordHash = hasher.Hash(password)
        };
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    public Task<bool> VerifyPasswordAsync(ApplicationUser user, string password, CancellationToken cancellationToken = default)
    {
        if (user.PasswordHash is null) return Task.FromResult(false);
        var ok = hasher.Verify(user.PasswordHash, password);
        return Task.FromResult(ok);
    }
}


