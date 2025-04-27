using AbstractExecution;
using aiExecBackend.Endpoints;
using aiExecBackend.Extensions;
using FlyExecution;
using LocalExecution;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Extensions;
using OpenAIIntegration;
using Stripe;
using Tooling;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddHttpForwarder();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = null;
});
builder.Services.AddSingleton<IRequestForwarder, RequestForwarder>();

builder.Services.AddScoped<FlyKeyTool>();
builder.Services.AddScoped<CodeExecutor>();
builder.Services.AddScoped<FlyAppCleanupService>();

builder.Services.AddDbContext<PostgresContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Neon").NormalizeConnectionString();
        // var connectionString = builder.Configuration.GetConnectionString("Local").NormalizeConnectionString();
        options.UseNpgsql(connectionString);
    }, optionsLifetime: ServiceLifetime.Singleton); 

builder.Services.AddDbContextFactory<PostgresContext>();

builder.Services.AddHostedService<FlyMachineCleanupService>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigin",
            sp => sp.WithOrigins(["http://localhost:5173"])
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials());
    });
    // builder.Services.AddScoped<IExecutionSetup, LocalAgentSetup>();
    builder.Services.AddScoped<IExecutionSetup, FlyMachineSetup>(); 
}
else
{
    builder.Services.AddScoped<IExecutionSetup, FlyMachineSetup>();
}

builder.Services.AddScoped<FlyMachineSetup>();
builder.Services.AddScoped<UserEnvironmentBuilder.Builder>();

builder.Services.AddIdentity<UserInfo, IdentityRole>(options =>
    {
        if (!builder.Environment.IsDevelopment()) return;
        options.User.RequireUniqueEmail = false;
        options.Password.RequireDigit = false;
        options.Password.RequiredLength = 1;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<PostgresContext>();
builder.Services.AddDataProtection().PersistKeysToDbContext<PostgresContext>();

builder.Services.AddScoped<OpenAIResponseGetter>();

builder.Services.AddSingleton(new HttpClient() 
{ 
    Timeout = TimeSpan.FromMinutes(10) 
});

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google_ClientId"] ??
                           throw new NullReferenceException("Google_ClientId not found!");
        options.ClientSecret = builder.Configuration["Google_ClientSecret"] ??
                               throw new NullReferenceException("Google_ClientSecret not found!");
        options.Events = new OAuthEvents
        {
            OnTicketReceived = context =>
            {
                context.Properties!.RedirectUri =
                    builder.Environment.IsDevelopment() ? "http://localhost:5173" : "https://app.syntix.pro";
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();

StripeConfiguration.ApiKey = builder.Configuration["StripeKey_API"];

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UsePathBase("/api/");
}

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/upload"))
    {
        var feature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
        if (feature is { IsReadOnly: false })
        {
            feature.MaxRequestBodySize = null;
        }
    }
    await next();
});

app.UseForwardedHeaders();
app.UseRouting();   

if (builder.Environment.IsDevelopment()) app.UseCors("AllowSpecificOrigin");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/user", UserEndpoints.GetClaimsHandler);
app.MapPost("/logout", LogoutEndpoint.Handler).RequireAuthorization();
app.MapPost("/deleteAccount", UserEndpoints.DeleteUserHandler).RequireAuthorization();
app.MapGet("/unsubscribe", UserEndpoints.UnsubscribeUserHandler).RequireAuthorization();
app.MapGet("/getUserState", UserEndpoints.GetStateHandler).RequireAuthorization();

app.MapPost("/upload", FilesProcessingEndpoints.UploadHandler).RequireAuthorization();
app.MapGet("/download", FilesProcessingEndpoints.DownloadHandler).RequireAuthorization();
app.MapGet("/scanDirectory", FilesProcessingEndpoints.ScanDirectory).RequireAuthorization();

app.MapGet("/googleLogin", OAuthEndpoints.GoogleFromFrontendHandler);
app.MapGet("/googleCallback", OAuthEndpoints.GoogleCallbackHandler);

app.MapGet("/templates", TemplateEndpoints.GetTemplatesHandler).RequireAuthorization();

var chatEndpoints = app.MapGroup("/chat");
chatEndpoints.MapPost("/create/{chatName}", ChatEndpoints.CreateChatHandler).RequireAuthorization();
chatEndpoints.MapPost("/startMachine", ChatEndpoints.StartMachineForChatHandler).RequireAuthorization();
chatEndpoints.MapDelete("/delete/{chatId}", ChatEndpoints.DeleteChatHandler).RequireAuthorization();
chatEndpoints.MapGet("/get", ChatEndpoints.GetUserChatsHandler).RequireAuthorization();

var messageEndpoints = app.MapGroup("/message");
messageEndpoints.MapPost("/getCompletion", MessageEndpoints.GetCompletion).RequireAuthorization();
messageEndpoints.MapGet("/getAllChatMessages/{chatId}", MessageEndpoints.GetAllMessagesInChatHandler)
    .RequireAuthorization();

var stripeEndpoints = app.MapGroup("/stripe");
stripeEndpoints.MapPost("/onSessionCompleted", StripeWebhookEndpoints.SessionCompletedHandler);

var customEnvironmentEndpoints= app.MapGroup("/environment");
customEnvironmentEndpoints.MapPost("/startEnvironmentBuilder", CustomEnvironmentsEndpoints.StartEnvironmentBuilderHandler);
customEnvironmentEndpoints.MapGet("/scanEnvironmentBuilderDirectory", CustomEnvironmentsEndpoints.ScanDirectory).RequireAuthorization();
customEnvironmentEndpoints.MapPost("/upload", CustomEnvironmentsEndpoints.UploadHandler);
customEnvironmentEndpoints.MapPost("/build", CustomEnvironmentsEndpoints.BuildEnvironmentHandler);
customEnvironmentEndpoints.MapGet("/list", CustomEnvironmentsEndpoints.GetAllCustomEnvironments);
customEnvironmentEndpoints.MapDelete("/delete/{customEnvironmentId:long}", CustomEnvironmentsEndpoints.DeleteCustomEnvironment);
customEnvironmentEndpoints.MapPut("/edit", CustomEnvironmentsEndpoints.EditCustomEnvironment);

var codeExecutionEndpoints = app.MapGroup("/execution");
codeExecutionEndpoints.MapPost("/new", CodeExecutionEndpoints.GetExecutionResult);

app.Run();