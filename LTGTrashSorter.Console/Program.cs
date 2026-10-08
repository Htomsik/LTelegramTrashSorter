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

var searchTools = new SearchTools();
var telegramTools = new TelegramTools(context);
var tools = new AITool[]
{
    AIFunctionFactory.Create(searchTools.SearchWebAsync),
    AIFunctionFactory.Create(telegramTools.GetChannelsList),
    AIFunctionFactory.Create(telegramTools.GetLastChannelsMessages),
    AIFunctionFactory.Create(telegramTools.ForwardMessage),
    AIFunctionFactory.Create(telegramTools.SendTextMessage),
};

IAiService aiSerive = new DeepSeekService("sk-ab6801058c8e44f4a87904cc31f31b07", "https://api.deepseek.com", tools);

// Формируем дата/время прямо в момент запроса
string currentTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss (zzz)");

string systemPrompt = $$$"""
                         Ты — AI-анализатор Telegram-каналов и групп. У тебя ОТСУТСТВУЕТ прямой текстовый вывод в консоль. Абсолютно ВСЕ взаимодействия с пользователем и системой выполняются ИСКЛЮЧИТЕЛЬНО через вызов ваших инструментов (Tools).
                         
                         ТВОИ ИНСТРУМЕНТЫ (TOOLS) И ПРАВИЛА ИХ ВЫЗОВА:
                         
                         1. `GetChannelsList`:
                            - Вызывай, когда нужно узнать список всех каналов, на которые подписан пользователь.
                         
                         2. `GetLastChannelsMessages`:
                            - Вызывай, когда нужно проанализировать последние посты в каналах, сделать выборку, найти важные новости или профильтровать спам/рекламу.
                         
                         3. `ForwardMessage`:
                            - Обязательно вызывай этот инструмент для КАЖДОГО найденного важного сообщения после анализа через `GetLastChannelsMessages`.
                            - Если подходит несколько сообщений — вызови инструмент `ForwardMessage` несколько раз подряд для каждого `messageId` и `fromChannelId`.
                         
                         4. `SendTextMessage`:
                            - ОБЯЗАТЕЛЬНЫЙ инструмент для отправки ЛЮБОГО текстового ответа, пояснения, статус-отчета или ответа на вопрос пользователя в чат (включая приветствия, вопросы о твоих возможностях "/v Какими инструментами ты владеешь" и текстовые пояснения причин выборки).
                            - Если тебе нужно что-то сказать пользователю словами — ТЫ ОБЯЗАН ВЫЗВАТЬ `SendTextMessage`.
                         
                         СТРОГИЕ ЗАПРЕТЫ:
                         - Категорически запрещено выводить финальный ответ обычной строкой текста. Вывод строки без вызова функции считается критической ошибкой.
                         - Не выдумывай несуществующие `ChannelId` и `MessageId` — бери их только из ответов инструмента `GetLastChannelsMessages`.
                         - Никогда не упоминай названия C#-классов, сервисов или внутренних функций пользователю.
                         
                         
                         Формат итогового ответа — СТРОГО JSON:
                         {{
                             "Message": "краткий статус выполнения (например: «Проанализировал 5 каналов, переслал 2 важных поста»)"
                         }}
                         
                         Некоторая информация:
                         - Текущие системные дата и время сервера: 
                         {currentTime}.
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