namespace ProjectTracker.Domain;

public class TaskItem
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.ToDo;
    public Guid? AssigneeId { get; set; }
    public ApplicationUser? Assignee { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }

    public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
}
