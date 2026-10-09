using DotNetEnv;
using LTGTrashSorter.AI.Services;
using LTGTrashSorter.AI.Tools;
using LTGTrashSorter.Core.Interfaces;
using LTGTrashSorter.Core.Models;
using LTGTrashSorter.Core.Services;
using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Services;
using LTGTrashSorter.TG.Services.Interfaces;
using LTGTrashSorter.TG.Tools;
using LTGTrashSorter.Worker;
using LTGTrashSorter.Worker.Services;
using Microsoft.Extensions.AI;

Env.TraversePath().Load();

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();
builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));

// Other
builder.Services.AddTransient<IDataRequestService, ConsoleDataRequestService>();

// AI
builder.Services.AddSingleton<IAiService, DeepSeekService>();
builder.Services.AddTransient<IContentAnalyzerService,  ContentAnalyzerService>();

// Telegram 
builder.Services.AddSingleton<BotContext>();
builder.Services.AddTransient<IChannelService, ChannelService>();
builder.Services.AddTransient<IAuthService, AuthService>();

// Tools
builder.Services.AddSingleton<ITelegramTools, TelegramTools>();
builder.Services.AddSingleton<IWebSearchTool, SearchTools>();

builder.Services.AddSingleton<IEnumerable<AITool>>(sp =>
{
    var tools = sp.GetRequiredService<ITelegramTools>();

    return new List<AITool>
    {
        AIFunctionFactory.Create(tools.GetChannelsList),
        AIFunctionFactory.Create(tools.GetLastChannelsMessages),
        AIFunctionFactory.Create(tools.ForwardMessage),
    };
});

builder.Services.AddHostedService<Worker>();


var host = builder.Build();
using (var scope = host.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Initializing Telegram API");
    var isAuthorise = await authService.AuthorizeAsync();

    if (!isAuthorise)
    {
        logger.LogCritical("Telegram is not authorized.");
    }
}

await host.RunAsync();