using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ProjectTracker.Api.Hubs;
using ProjectTracker.Api.Models;
using ProjectTracker.Api.Models.Requests;
using ProjectTracker.Domain;
using ProjectTracker.Domain.Repositories;
using System.Security.Claims;

namespace ProjectTracker.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IProjectRepository projects;
    private readonly ITaskRepository tasksRepo;
    private readonly IHubContext<TasksHub> hub;

    public TasksController(IProjectRepository projects, ITaskRepository tasksRepo, IHubContext<TasksHub> hub)
    {
        this.projects = projects;
        this.tasksRepo = tasksRepo;
        this.hub = hub;
    }

    [HttpGet]
    public async Task<IActionResult> GetTasks(Guid projectId)
    {
        var userId = GetUserId();
        var tasks = await tasksRepo.ListAsync(projectId, userId);
        if (tasks.Count == 0)
        {
            var proj = await projects.GetAccessibleProjectByIdAsync(projectId, userId);
            if (proj is null) return StatusCode(403, ApiResponse<object>.Fail("Not a member of this project", 403));
        }
        return Ok(ApiResponse<object>.Success(tasks));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateTaskRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid task data", 400));
        }
        var userId = GetUserId();
        var task = new TaskItem
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            Title = request.Title,
            Description = request.Description,
            AssigneeId = request.AssigneeId,
            DueDate = request.DueDate,
            Status = request.Status
        };
        var created = await tasksRepo.CreateAsync(projectId, userId, task);
        if (created is null) return StatusCode(403, ApiResponse<object>.Fail("Only project owner can create tasks", 403));
        await hub.Clients.Group(projectId.ToString()).SendAsync("tasksChanged", new { projectId, type = "created", taskId = task.Id });
        return Ok(ApiResponse<TaskItem>.Success(created, "Task created", 201));
    }

    [HttpPut("{taskId:guid}")]
    public async Task<IActionResult> Update(Guid projectId, Guid taskId, UpdateTaskRequest request)
    {
        var userId = GetUserId();
        var updated = await tasksRepo.UpdateAsync(projectId, userId, taskId, t =>
        {
            t.Title = request.Title;
            t.Description = request.Description;
            t.AssigneeId = request.AssigneeId;
            t.DueDate = request.DueDate;
            t.Status = request.Status;
        });
        if (updated is null) return StatusCode(403, ApiResponse<object>.Fail("Only project owner can edit tasks", 403));
        await hub.Clients.Group(projectId.ToString()).SendAsync("tasksChanged", new { projectId, type = "updated", taskId = updated.Id });
        return Ok(ApiResponse<TaskItem>.Success(updated, "Task updated"));
    }

    [HttpPatch("{taskId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid projectId, Guid taskId, UpdateTaskStatusRequest request)
    {
        var userId = GetUserId();
        var updated = await tasksRepo.UpdateStatusAsync(projectId, userId, taskId, request.Status);
        if (updated is null) return StatusCode(403, ApiResponse<object>.Fail("Only project members can move tasks", 403));
        await hub.Clients.Group(projectId.ToString()).SendAsync("tasksChanged", new { projectId, type = "updated", taskId = updated.Id });
        return Ok(ApiResponse<TaskItem>.Success(updated, "Status updated"));
    }

    [HttpDelete("{taskId:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid taskId)
    {
        var userId = GetUserId();
        var ok = await tasksRepo.DeleteAsync(projectId, userId, taskId);
        if (!ok) return StatusCode(403, ApiResponse<object>.Fail("Only project owner can delete tasks", 403));
        await hub.Clients.Group(projectId.ToString()).SendAsync("tasksChanged", new { projectId, type = "deleted", taskId });
        return Ok(ApiResponse<object>.Success(null, "Task deleted"));
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }
}


