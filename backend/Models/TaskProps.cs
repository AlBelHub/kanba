namespace backend.Models;

public class TaskProps
{
    public int column_id { get; set; }
    public int board_id { get; set; }
    public string title { get; set; }
    public string description { get; set; }
    public string status { get; set; }
    public int position { get; set; }
    public int created_by { get; set; }
}