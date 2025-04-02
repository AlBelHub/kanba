namespace backend.Models;

public class Space
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public Guid UserId { get; set; }
    public string created_at { get; set; }
}