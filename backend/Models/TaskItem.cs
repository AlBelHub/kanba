namespace backend.Models;

public class TaskItem
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string? Position { get; set; } // Позиция задачи
}