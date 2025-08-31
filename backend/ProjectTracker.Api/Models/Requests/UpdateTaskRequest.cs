using System.ComponentModel.DataAnnotations;
using TaskStatus = ProjectTracker.Domain.Enums.TaskStatus;

namespace ProjectTracker.Api.Models.Requests;

public class UpdateTaskRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public Guid? AssigneeId { get; set; }
    [Required]
    public DateTime? DueDate { get; set; }
    [Required]
    public TaskStatus Status { get; set; }
}


