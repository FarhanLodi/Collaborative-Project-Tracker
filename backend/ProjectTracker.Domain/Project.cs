namespace ProjectTracker.Domain;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? Deadline { get; set; }
    public Guid OwnerId { get; set; }
    public ApplicationUser? Owner { get; set; }
    public string InviteCode { get; set; } = string.Empty;

    public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
