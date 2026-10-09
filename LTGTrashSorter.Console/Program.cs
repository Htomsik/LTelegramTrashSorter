// See https://aka.ms/new-console-template for more information

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
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;


// No DI Config
Env.TraversePath().Load();
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();
var settings = new Settings();
configuration.GetSection("Settings").Bind(settings);
var  options = Options.Create(settings);

var context = new BotContext();
IWebSearchTool searchTools = new SearchTools(options);
var telegramTools = new TelegramTools(context);
var tools = new AITool[]
{
    AIFunctionFactory.Create(searchTools.SearchWebAsync),
    AIFunctionFactory.Create(telegramTools.GetChannelsList),
    AIFunctionFactory.Create(telegramTools.GetLastChannelsMessages),
    AIFunctionFactory.Create(telegramTools.ForwardMessage),
    AIFunctionFactory.Create(telegramTools.SendTextMessage),
};

IAiService aiSerive = new DeepSeekService(options, tools);
IAuthService authService = new AuthService(context, options, new ConsoleDataRequestService());
IMessageService messageService = new MessageService(context, msg =>
{
    
    var result = aiSerive.ProcessMessageAsync(msg).Result;
    return result;
} );

telegramTools.MessageService = messageService;

IDialogService dialogService = new DialogService(context);
IChannelService channelService = new ChannelService(context);
var ret = await authService.AuthorizeAsync();
if (!ret)
    return;

await dialogService.LoadAsync();

foreach (var channel in context.Channels)
{
    await channelService.LoadLastMessagesAsync(channel.Value.Id);
}


context.TelegramClient.OnUpdates += messageService.ProcessUpdatesAsync;

Console.ReadLine();