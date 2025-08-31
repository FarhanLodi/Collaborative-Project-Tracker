namespace ProjectTracker.Api.Models.Requests;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Designation { get; set; }
}


