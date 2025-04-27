using Models.Enums;

namespace Models;

public partial class ExecutionMachineTemplate
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ImageUrl { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; }

    public string? AppName { get; set; } // The name of the app which is used as a "repository" for the created docker image. 
    
    // The below 4 will only have values in Custom Environments
    public string? RootDirectory { get; set; }
    public string? CodeFile { get; set; }
    public string? AfterChangesValidationCommand { get; set; }
    public string? DependencyInstallingTerminalCall { get; set; }
    
    public string? ProgrammingLanguage { get; set; }
    public ICollection<ExecutionMachine> ExecutionMachines { get; set; } = new List<ExecutionMachine>();

    public string? CreatorId { get; set; }
    public UserInfo? Creator { get; set; }
}