namespace backend.Models;

public class Board
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int SpaceId { get; set; }
    public int OwnerId { get; set; }
    public string CreatedAt { get; set; }
}