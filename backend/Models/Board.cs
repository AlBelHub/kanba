namespace backend.Models;

public class Board
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid SpaceId { get; set; }
    public Guid OwnerId { get; set; }
    public string CreatedAt { get; set; }
}