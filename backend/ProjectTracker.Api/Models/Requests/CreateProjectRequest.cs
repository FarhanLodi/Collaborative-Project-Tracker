using System.ComponentModel.DataAnnotations;

namespace ProjectTracker.Api.Models.Requests;

public class CreateProjectRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public DateTime? Deadline { get; set; }
}


