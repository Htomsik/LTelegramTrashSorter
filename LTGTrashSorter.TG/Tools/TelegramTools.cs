using System.ComponentModel;
using LTGTrashSorter.Core.Interfaces;
using LTGTrashSorter.Core.Models;
using LTGTrashSorter.Core.Models.DTO;
using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Models;
using LTGTrashSorter.TG.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LTGTrashSorter.TG.Tools;

public class TelegramTools : ITelegramTools
{
    private readonly BotContext _context;
    private readonly ILogger<TelegramTools>? _logger;
    private readonly IMessageService _messageSender;

    public TelegramTools(BotContext context,  ILogger<TelegramTools>? logger , IMessageService messageSender)
    {
        _context = context;
        _logger = logger;
        _messageSender = messageSender;
    }
    
    [Description("Получить список всех каналов")]
    public async Task<string> GetChannelsList()
    {
        if (_context.TelegramClient == null)
            return "Telegram клиент не авторизован.";

        string json;
        try
        {
            json = JsonConvert.SerializeObject(_context.Channels);
        }
        catch (Exception ex)
        {
            return "Ошибка чтения TG";
        }
        
        return json;
    }
    
    [Description("Получить список последних постов во всех каналах для последующего анализа.")]
    public async Task<string> GetLastChannelsMessages()
    {
        if (_context.TelegramClient == null)
            return "Telegram клиент не авторизован.";

        List<TelegramMessagesDto> channelMessagesDto = new ();

        foreach (var channel in _context.Channels.Values)
        {
            if(channel.LastMessages.Count == 0 )
                continue;
            
            var messageDto = new TelegramMessagesDto(
                channel.Id, 
                channel.DisplayName, 
                channel.LastMessages.Select(x => new TelegramMessageDto(x.Id, x.Message)).ToList());
            
            channelMessagesDto.Add(messageDto);
        }
        
        string json;
        try
        {
            json = JsonConvert.SerializeObject(channelMessagesDto);
        }
        catch (Exception ex)
        {
            return "Ошибка чтения TG";
        }
        
        return json;
    }
    
    [Description("Переслать конкретные сообщения в канал. Обязательно вызывай этот инструмент для найденных постов подходящих под параметры.")]
    public async Task<string> ForwardMessage(List<TelegramImportantMessageDto> importantMessages)
    {
        _logger?.LogInformation($"{nameof(ForwardMessage)} Started. {importantMessages.Count} Messaged");

        long targetId = _context.TargetChannel?.Id ?? 0;
        if (targetId == 0)
        {
            _logger?.LogError($"{nameof(ForwardMessage)} aborted: {nameof(targetId)} is empty");
            return "Ошибка: непредвиденная серверная ошибка, функционал недоступен";
        }
        
        if (importantMessages.Count == 0)
        {
            _logger?.LogWarning($"{nameof(ForwardMessage)} aborted: {nameof(importantMessages)} is empty");
            return "Список сообщений для пересылки пуст.";
        }
        
        var errors = new List<string>();
        int successCount = 0;

        foreach (var messageDto in importantMessages)
        {
            if (!_context.Channels.TryGetValue(messageDto.PeerId, out var channel))
            {
                var err = $"Ошибка: Канал с ID {messageDto.PeerId} не найден в списке каналов.";
                errors.Add(err);
                continue;
            }

            var messageExists = channel.LastMessages.Any(m => m.Id == messageDto.Id);
            if (!messageExists)
            {
                var err = $"Ошибка: Сообщение с ID {messageDto.Id} отсутствует в канале {channel.Id}).";
                errors.Add(err);
                continue;
            }

            var success = await _messageSender.ForwardMessageByIdsAsync(messageDto.PeerId, targetId, messageDto.Id);
            if (!success)
            {
                var err = $"Техническая ошибка при пересылке сообщения {messageDto.Id} из канала {messageDto.PeerId}.";
                errors.Add(err);
                continue;
            }
            successCount++;
        }

        _logger?.LogInformation($"{nameof(ForwardMessage)} finished. Processed {successCount}/{importantMessages.Count}");
        return $"Переслано {successCount} из {importantMessages.Count}";
    }

    [Description("Отправить текстовый ответ или пояснение пользователю в чат.")]
    public async Task<string> SendTextMessage(
        [Description("Текст сообщения для отправки")] string text)
    {

        long targetId = _context.Me?.id ?? 0;
        if (targetId == 0)
        {
            return $"Ошибка при пересылке сообщения.";
        }
        
        await _messageSender.SendMessageAsync(targetId, text);
    
        return "Сообщение отправлено.";
    }
}