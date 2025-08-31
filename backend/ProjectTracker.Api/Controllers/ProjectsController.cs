using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectTracker.Api.Models;
using ProjectTracker.Api.Models.Requests;
using ProjectTracker.Domain;
using ProjectTracker.Domain.Repositories;
using System.Security.Claims;

namespace ProjectTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectRepository projects;

    public ProjectsController(IProjectRepository projects)
    {
        this.projects = projects;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyProjects()
    {
        var userId = GetUserId();
        var list = await projects.GetMyProjectsAsync(userId);
        return Ok(ApiResponse<object>.Success(list));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse<object>.Fail("Invalid project data", 400));
        }
        var userId = GetUserId();
        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Deadline = request.Deadline,
            OwnerId = userId,
            InviteCode = Guid.NewGuid().ToString("N").Substring(0, 8)
        };
        var result = await projects.AddAsync(project);
        return Ok(ApiResponse<Project>.Success(result, "Project created", 201));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = GetUserId();
        var project = await projects.GetAccessibleProjectByIdAsync(id, userId, includeMembers: true);
        if (project is null) return NotFound(ApiResponse<object>.Fail("Project not found", 404));
        return Ok(ApiResponse<Project>.Success(project));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var ok = await projects.DeleteIfOwnerAsync(id, userId);
        if (!ok) return StatusCode(403, ApiResponse<object>.Fail("Only owner can delete project", 403));
        return Ok(ApiResponse<object>.Success(null, "Project deleted"));
    }

    [HttpPost("join")]
    public async Task<IActionResult> Join(JoinProjectRequest request)
    {
        var userId = GetUserId();
        var project = await projects.JoinByInviteCodeAsync(request.InviteCode, userId);
        if (project is null) return NotFound(ApiResponse<object>.Fail("Invalid invite code", 404));
        return Ok(ApiResponse<Project>.Success(project, "Joined project"));
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }
}


