namespace backend.Models;

public class TaskWithUsersDto
{
    public Guid id { get; set; }
    public Guid column_id { get; set; }
    public Guid board_id { get; set; }
    public string title { get; set; } = string.Empty;
    public string? description { get; set; }
    public string status { get; set; } = "open";
    public int position { get; set; }
    public Guid? created_by { get; set; }
    public Guid? assigned_to { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }

    public string? created_by_username { get; set; }
    public string? assigned_to_username { get; set; }
}