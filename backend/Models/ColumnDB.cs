namespace backend.Models;

public class ColumnDB
{
    public int Id { get; set; }
    public int BoardId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Position { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}