namespace backend.Models;

public class BoardsProps
{
    public Guid id { get; set; }
    public string name { get; set; }
    public Guid space_id { get; set; }
    public Guid owner_id { get; set; }
}