using DTOs.MachineTemplate;
using Microsoft.EntityFrameworkCore;
using Models;

namespace aiExecBackend.Endpoints;

public static class TemplateEndpoints
{
    public static async Task<IResult> GetTemplatesHandler(PostgresContext postgresContext)
    {
        var flyMachineTemplates =
            (await postgresContext.FlyMachineTemplates.Where(a => a.CreatorId == null).ToListAsync()).Select(a =>
                new MachineTemplateFrontendDTO(a));

        return Results.Ok(flyMachineTemplates);
    }
}