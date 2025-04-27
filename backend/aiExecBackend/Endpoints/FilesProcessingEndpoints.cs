using AbstractExecution;
using aiExecBackend.Endpoints.Common;
using aiExecBackend.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Enums;

namespace aiExecBackend.Endpoints;

public static class FilesProcessingEndpoints
{
    public static async Task UploadHandler([FromQuery] long executionEnvironmentTemplateId,
        [FromQuery] Guid chatId, HttpContext context, SignInManager<UserInfo> signInManager,
        IExecutionSetup executionSetup, IRequestForwarder forwarder)
    {
        await Forwarder.ForwardRequest(executionEnvironmentTemplateId, chatId, context, signInManager, executionSetup, forwarder,
            "/saveFiles");
    }

    public static async Task DownloadHandler([FromQuery] long executionEnvironmentTemplateId,
        [FromQuery] Guid chatId, [FromQuery] string pathToFile, HttpContext context, SignInManager<UserInfo> signInManager,
        IExecutionSetup executionSetup, IRequestForwarder forwarder)
    {
        await Forwarder.ForwardRequest(executionEnvironmentTemplateId, chatId, context, signInManager, executionSetup, forwarder,
            $"/downloadFile?pathToFile={pathToFile}");
    }

    public static async Task ScanDirectory([FromQuery] long executionEnvironmentTemplateId,   
        [FromQuery] Guid chatId, HttpContext context, SignInManager<UserInfo> signInManager,
        IExecutionSetup executionSetup, IRequestForwarder forwarder)
    {
        await Forwarder.ForwardRequest(executionEnvironmentTemplateId, chatId, context, signInManager, executionSetup, forwarder,
            "/scanDirectory");
    }

    
}