namespace backend.Models;

public class ColumnsProps
{
    public Guid CreatedBy { get; set; }
    public string Title { get; set; }
    public Guid BoardId { get; set; }
    public int Position { get; set; }
}