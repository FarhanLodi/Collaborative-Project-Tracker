using TaskStatus = ProjectTracker.Domain.Enums.TaskStatus;

namespace ProjectTracker.Api.Models.Requests;

public class UpdateTaskStatusRequest
{
    public TaskStatus Status { get; set; }
}


