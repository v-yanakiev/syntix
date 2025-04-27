namespace Models;

public partial class Message
{
    public long Id { get; set; }
    public string Content { get; set; } = null!;
    public Guid ChatId { get; set; }
    public string Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual Chat Chat { get; set; } = null!;
    public virtual UserInfo? Sender { get; set; }
}