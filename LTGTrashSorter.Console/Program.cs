using LTGTrashSorter.AI.Services;
using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Models;
using LTGTrashSorter.TG.Services;
using LTGTrashSorter.TG.Services.Interfaces;

IAiService aiSerive = new DeepSeekService("");

// Формируем дата/время прямо в момент запроса
string currentTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss (zzz)");

string systemPrompt = $"""
                       Ты — подключенная к Telegram нейросеть с доступом к веб-поиску.

                       Правила:
                       1. Используй результаты поиска как факты. Если ищешь время или курс валют, сопоставляй с текущей датой сервера.
                       2. Если пользователь просит источник, указывай конкретный сайт или ссылку из результатов поиска.
                       3. Отвечай кратко, чётко, по существу и без мета-рассуждений, но не используй /n и прочее, только человекочитаемый текст

                       Формат ответа — СТРОГО JSON:
                           "Message": "текст ответа"

                       Некоторая информация:
                        - Текущие системные дата и время сервера: {currentTime}.
""";

string message = "";

// Authorization information for TG
var authorizationData = new AuthorizationData(
    "", //YOUR_API_ID
    "", //YOUR_API_HASH
    "", //YOUR_PHONE_NUMBER (+111.... Formate)
    "" //YOUR_PASSWORD
);

var context = new BotContext(authorizationData);
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

IUserService userService = new UserService(context);


var ret = await authService.AuthorizeAsync();
if (!ret)
    return;

await userService.LoadUsersAsync();

context.TelegramClient.OnUpdates += messageService.ProcessUpdatesAsync;

Console.ReadLine();