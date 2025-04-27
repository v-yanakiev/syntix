namespace Models;

public partial class ExecutionMachine
{
    public string Id { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastActive { get; set; }
    public Guid? ChatId { get; set; }
    public Chat Chat { get; set; }
    public string? UserId { get; set; }
    public UserInfo User { get; set; }
    public long ExecutionMachineTemplateId { get; set; }
    public ExecutionMachineTemplate? ExecutionMachineTemplate { get; set; }
    public string AppName { get; set; }
    public string AppAddress { get; set; }
    
}