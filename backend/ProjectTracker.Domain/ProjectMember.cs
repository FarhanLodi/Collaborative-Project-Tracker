namespace ProjectTracker.Domain;

public class ProjectMember
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
}
