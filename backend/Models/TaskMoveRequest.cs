namespace backend.Models;

public class TaskMoveRequest
{
    public Guid OldColumnId { get; set; }
    public Guid NewColumnId { get; set; }
    public int TaskOldPos { get; set; }
    public int TaskNewPos { get; set; }
    public Guid BoardId { get; set; }
    public Guid TaskId { get; set; }
}