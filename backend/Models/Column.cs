namespace backend.Models;

public class Column
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int? Position { get; set; } // Позиция колонки
    public List<TaskItem> Tasks { get; set; }
}