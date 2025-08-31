namespace ProjectTracker.Domain;

public class TaskAttachment
{
    public Guid Id { get; set; }
    public Guid TaskItemId { get; set; }
    public TaskItem? TaskItem { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string StoragePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
