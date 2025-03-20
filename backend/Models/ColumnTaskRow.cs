namespace backend.Models;

public class ColumnTaskRow
{
    public int ColumnId { get; set; }
    public string ColumnTitle { get; set; }
    public string ColumnPosition { get; set; } // Позиция колонки
    public int? TaskId { get; set; }
    public string TaskTitle { get; set; }
    public int BoardId { get; set; }
    public string TaskPosition { get; set; } // Позиция задачи
}