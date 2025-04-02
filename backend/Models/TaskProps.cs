namespace backend.Models;

public class TaskProps
{
    public Guid column_id { get; set; }
    public Guid board_id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string status { get; set; }
    public int position { get; set; }
    public Guid created_by { get; set; }
}