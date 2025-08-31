using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectTracker.Api.Models;
using ProjectTracker.Domain;
using ProjectTracker.Domain.Repositories;
using System.Security.Claims;

namespace ProjectTracker.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/tasks/{taskId:guid}/[controller]")]
[Authorize]
public class AttachmentsController : ControllerBase
{
    private readonly IAttachmentRepository attachments;
    private readonly IProjectRepository projects;
    private readonly IWebHostEnvironment env;

    public AttachmentsController(IAttachmentRepository attachments, IProjectRepository projects, IWebHostEnvironment env)
    {
        this.attachments = attachments;
        this.projects = projects;
        this.env = env;
    }

    [HttpGet]
    public async Task<IActionResult> List(Guid projectId, Guid taskId)
    {
        var userId = GetUserId();
        var proj = await projects.GetAccessibleProjectByIdAsync(projectId, userId);
        if (proj is null) return StatusCode(403, ApiResponse<object>.Fail("Not a member of this project", 403));
        var items = await attachments.ListAsync(projectId, taskId, userId);
        return Ok(ApiResponse<object>.Success(items));
    }

    [HttpPost]
    [RequestSizeLimit(25_000_000)]
    public async Task<IActionResult> Upload(Guid projectId, Guid taskId, IFormFile file)
    {
        var userId = GetUserId();
        if (file is null || file.Length == 0) return BadRequest(ApiResponse<object>.Fail("No file", 400));

        var uploadsDir = Path.Combine(env.ContentRootPath, "uploads");
        Directory.CreateDirectory(uploadsDir);
        var fileId = Guid.NewGuid();
        var safeName = fileId + "_" + Path.GetFileName(file.FileName);
        var path = Path.Combine(uploadsDir, safeName);
        await using (var stream = System.IO.File.Create(path))
        {
            await file.CopyToAsync(stream);
        }

        var attach = new TaskAttachment
        {
            Id = fileId,
            TaskItemId = taskId,
            FileName = file.FileName,
            ContentType = file.ContentType,
            SizeBytes = file.Length,
            StoragePath = safeName
        };
        var created = await attachments.AddAsync(projectId, taskId, userId, attach);
        if (created is null) return StatusCode(403, ApiResponse<object>.Fail("Not allowed", 403));
        return Ok(ApiResponse<TaskAttachment>.Success(created, "Attachment uploaded", 201));
    }

    [HttpGet("{attachmentId:guid}")]
    public async Task<IActionResult> Download(Guid projectId, Guid taskId, Guid attachmentId)
    {
        var userId = GetUserId();
        var attach = await attachments.GetAsync(projectId, taskId, attachmentId, userId);
        if (attach is null) return NotFound(ApiResponse<object>.Fail("Attachment not found", 404));
        var uploadsDir = Path.Combine(env.ContentRootPath, "uploads");
        var path = Path.Combine(uploadsDir, attach.StoragePath);
        if (!System.IO.File.Exists(path)) return NotFound(ApiResponse<object>.Fail("File not found", 404));
        var bytes = await System.IO.File.ReadAllBytesAsync(path);
        return File(bytes, attach.ContentType, attach.FileName);
    }

    private Guid GetUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        return Guid.Parse(sub!);
    }
}


