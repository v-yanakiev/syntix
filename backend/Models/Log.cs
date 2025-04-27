namespace Models;

public class Log
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string Content { get; set; } = null!;
}