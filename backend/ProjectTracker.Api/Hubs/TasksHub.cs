using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ProjectTracker.Api.Hubs;

[Authorize]
public class TasksHub : Hub
{
    public Task JoinProject(Guid projectId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, projectId.ToString());
    }
}


