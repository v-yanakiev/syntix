namespace Models;

public partial class Chat
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string CreatorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual UserInfo Creator { get; set; } = null!;
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    public virtual ICollection<ExecutionMachine> ExecutionMachines { get; set; } = new List<ExecutionMachine>();
}