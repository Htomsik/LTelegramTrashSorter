using DotNetEnv;
using LTGTrashSorter.AI.Services;
using LTGTrashSorter.AI.Tools;
using LTGTrashSorter.Core.Interfaces;
using LTGTrashSorter.Core.Models;
using LTGTrashSorter.Core.Services;
using LTGTrashSorter.DB.Infrastructure;
using LTGTrashSorter.DB.Repository;
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

// DB
builder.Services.AddTransient<DbInitializer>();
builder.Services.AddTransient<ITelegramChannelRepository, TelegramChannelRepository>();
builder.Services.AddTransient<ITelegramStateService,  TelegramDbStateService>();

// Other
builder.Services.AddTransient<IDataRequestService, ConsoleDataRequestService>();
builder.Services.AddSingleton<IScheduleService, ScheduleService>();

// AI
builder.Services.AddSingleton<IAiService, DeepSeekService>();
builder.Services.AddTransient<IContentAnalyzerService,  ContentAnalyzerService>();

// Telegram 
builder.Services.AddSingleton<BotContext>();
builder.Services.AddTransient<IChannelService, ChannelService>();
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddTransient<IDialogService, DialogService>();
builder.Services.AddTransient<IMessageService, MessageService>();
builder.Services.AddSingleton<IPublishService, PublishService>();

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
        AIFunctionFactory.Create(tools.SaveImportantMessages),
    };
});

builder.Services.AddHostedService<Worker>();


var host = builder.Build();

// DB init
using (var scope = host.Services.CreateScope())
{
    var dbInitializer = scope.ServiceProvider.GetRequiredService<DbInitializer>();
    dbInitializer.Initialize();
}

// TG init
using (var scope = host.Services.CreateScope())
{
    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("Initializing Telegram API");
    var isAuthorise = await authService.AuthorizeAsync();

    if (!isAuthorise)
    {
        logger.LogCritical("Telegram is not authorized.");
        return;
    }
    
    WTelegram.Helpers.Log = (lvl, str) => logger.Log((LogLevel)lvl, "WTelegram: {Message}", str);
}

await host.RunAsync();

