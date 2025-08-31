namespace ProjectTracker.Domain;

public class ApplicationUser
{
    public Guid Id { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? FullName { get; set; }
    public string? Designation { get; set; }
}
