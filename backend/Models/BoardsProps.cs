namespace backend.Models;

public class BoardsProps
{
    public Guid id { get; set; }
    public string Name { get; set; }
    public Guid SpaceId { get; set; }
    public Guid OwnerId { get; set; }
}