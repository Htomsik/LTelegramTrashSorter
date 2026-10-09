namespace LTGTrashSorter.Core.Models;

public class Settings
{
    public WorkerSettings Worker { get; set; } = new();
        
    public TelegramSettings Telegram { get; set; } = new();
    
    public OpenAiSettings Ai { get; set; } = new();
    
}

public class WorkerSettings
{
    public int Timeout { get; set; } = 60*1000;
}

public class TelegramSettings
{
    public string ApiId { get; set; }
    
    public string ApiHash { get; set; } = string.Empty;
    
    public string PhoneNumber { get; set; } = string.Empty;
    
    public string Password { get; set; } = string.Empty;
}

public class OpenAiSettings
{
    public string ApiKey { get; set; } = string.Empty;
    
    public string WebToolKey { get; set; } = string.Empty;

    public string BaseURL { get; set; } = "https://api.deepseek.com";

    public string ModelName { get; set; } = "deepseek-chat";
    
    public string SystemPrompt { get; set; } =  $$$"""
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
                         {{{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss (zzz)")}}}.
                         """;
}