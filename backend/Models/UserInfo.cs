using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Models;

public partial class UserInfo : IdentityUser
{
    public DateTime CreatedAt { get; set; }
    public virtual ICollection<Chat> Chats { get; set; } = new List<Chat>();
    public virtual ICollection<ExecutionMachine> ExecutionMachines { get; set; } = new List<ExecutionMachine>();
    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<ExecutionMachineTemplate> CustomExecutionMachineTemplates { get; set; } =
        new List<ExecutionMachineTemplate>();

    public decimal Balance { get; set; }
    [MaxLength(50)] public string? SubscriptionId { get; set; }

    public bool IsInValidTrial()
    {
        var accountIsInTrial = (DateTime.UtcNow - this.CreatedAt).TotalDays <= 1;
        const decimal dollarsTrialUserCanExpend = 2;
        return accountIsInTrial && this.Balance >= -dollarsTrialUserCanExpend;
    }

    public bool HasUnsubscribed()
    {
        return SubscriptionId == "";
    }

    public void SetHasUnsubscribed()
    {
        SubscriptionId = "";
    }
}