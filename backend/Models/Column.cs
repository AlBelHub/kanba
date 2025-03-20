namespace backend.Models;

public class Column
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string? Position { get; set; } // Позиция колонки
    public List<TaskItem> Tasks { get; set; }
}