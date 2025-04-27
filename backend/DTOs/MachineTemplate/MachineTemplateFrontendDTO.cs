using Models;

namespace DTOs.MachineTemplate;

public class MachineTemplateFrontendDTO(ExecutionMachineTemplate template)
{
    public long Id { get; } = template.Id;
    public string Name { get;  } = template.Name;
}