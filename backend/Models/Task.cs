namespace backend.Models;

public class Task
{
    public int Id { get; set; }
    public int ColumnId { get; set; }
    public int BoardId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "open"; // По умолчанию 'open'
    public int Position { get; set; }
    public int? CreatedBy { get; set; }
    public int? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
