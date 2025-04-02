namespace backend.Models;

public class Task
{
    public Guid Id { get; set; }
    public Guid ColumnId { get; set; }
    public Guid BoardId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "open"; // По умолчанию 'open'
    public int Position { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
