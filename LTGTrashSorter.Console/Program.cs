// See https://aka.ms/new-console-template for more information

using LTGTrashSorter.AI.Services;
using LTGTrashSorter.AI.Tools;
using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Models;
using LTGTrashSorter.TG.Services;
using LTGTrashSorter.TG.Services.Interfaces;
using LTGTrashSorter.TG.Tools;
using Microsoft.Extensions.AI;


// Authorization information for TG
var authorizationData = new AuthorizationData(
    "", //YOUR_API_ID
    "", //YOUR_API_HASH
    "+", //YOUR_PHONE_NUMBER (+111.... Formate)
    "" //YOUR_PASSWORD
);

var context = new BotContext(authorizationData);


var telegramTools = new TelegramTools(context);
var tools = new AITool[]
{
    AIFunctionFactory.Create(telegramTools.GetProcessChannelsList)
};

IAiService aiSerive = new DeepSeekService("", "https://api.deepseek.com", tools);

// Формируем дата/время прямо в момент запроса
string currentTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss (zzz)");

string systemPrompt = $"""
                       Ты - помощник в чате телеграмм

                       Правила:
                       1. Используй результаты поиска как факты. Если ищешь время или курс валют, сопоставляй с текущей датой сервера.
                       2. Если пользователь просит источник, указывай конкретный сайт или ссылку из результатов поиска.
                       3. Отвечаешь только кратко и по делу, если где-то предпологается список, лучше списком.
                       4. Пользователь никогда не должен знать, как устроены твои внутренние инструменты.
                       5. Если пользователь спрашивает, что ты умеешь или какие у тебя инструменты, называй их по указанным названиям в описании, никогда не называй внутренние названия.
                      
                       Формат ответа — СТРОГО JSON:
                           "Message": "текст ответа"

                       Некоторая информация:
                        - Текущие системные дата и время сервера: {currentTime}.
""";

string message = "";

IAuthService authService = new AuthService(context, () =>
{
    Console.WriteLine("Введите код верификации");
    return Console.ReadLine();
});
IMessageService messageService = new MessageService(context, msg =>
{
    
    var result = aiSerive.ProcessMessageAsync(systemPrompt, msg).Result;
    return result;
} );

IDialogService dialogService = new DialogService(context);
IChannelService channelService = new ChannelService(context);
var ret = await authService.AuthorizeAsync();
if (!ret)
    return;

await dialogService.LoadAsync();

var channel = context.Channels.FirstOrDefault(x => x.DisplayName == "KIS");
await channelService.LoadLastMessagesAsync(channel.Id);

context.TelegramClient.OnUpdates += messageService.ProcessUpdatesAsync;

Console.ReadLine();