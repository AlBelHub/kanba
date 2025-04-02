namespace backend.Models;

public class TaskItem
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public int? Position { get; set; } // Позиция задачи
}