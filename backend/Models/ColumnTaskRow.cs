namespace backend.Models;

public class ColumnTaskRow
{
    public Guid ColumnId { get; set; }
    public string ColumnTitle { get; set; }
    public int ColumnPosition { get; set; } // Позиция колонки
    public Guid TaskId { get; set; }
    public string TaskTitle { get; set; }
    public Guid BoardId { get; set; }
    public int? TaskPosition { get; set; } // Позиция задачи
}