using System.ComponentModel;
using LTGTrashSorter.Core.Interfaces;
using LTGTrashSorter.Core.Models.DTO;
using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LTGTrashSorter.TG.Tools;

public class TelegramTools : ITelegramTools
{
    private readonly BotContext _context;
    private readonly ILogger<TelegramTools>? _logger;
    private readonly IMessageService _messageSender;
    private readonly IPublishService _publishService;

    public TelegramTools(BotContext context,  ILogger<TelegramTools>? logger , IMessageService messageSender, IPublishService publishService)
    {
        _context = context;
        _logger = logger;
        _messageSender = messageSender;
        _publishService = publishService;
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

        var allChannels = _context.Channels.Values.ToList();
        if (_context.TargetChannel != null)
        {
            allChannels.Add(_context.TargetChannel);
        }

        List<TelegramMessagesDto> channelMessagesDto = new();

        foreach (var channel in allChannels)
        {
            if (channel?.LastMessages == null || channel.LastMessages.Count == 0)
                continue;
        
            var messageDto = new TelegramMessagesDto(
                channel.Id, 
                channel.DisplayName,
                channel.IsTargetChannel, 
                channel.LastMessages.Select(x => new TelegramMessageDto(x.Id, x.Message)).ToList()
            );
        
            channelMessagesDto.Add(messageDto);
        }
        
        if (_context.TargetChannel != null && _context.TargetChannel.LastMessages.Count > 0)
        {
            var targetMessageDto = new TelegramMessagesDto(
                _context.TargetChannel.Id,
                _context.TargetChannel.DisplayName,
                true, 
                _context.TargetChannel.LastMessages.Select(x => new TelegramMessageDto(x.Id, x.Message)).ToList()
            );

            channelMessagesDto.Add(targetMessageDto);
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
    
    [Description("Сохранить ID важных сообщений для последующей пакетной публикации. Обязательно вызывай этот инструмент для найденных постов, подходящих под параметры.")]
    public async Task<string> SaveImportantMessages(List<TelegramImportantMessageDto> importantMessages)
    {
        _logger?.LogInformation($"{nameof(SaveImportantMessages)} Started. {importantMessages.Count} messages requested.");

        if (importantMessages.Count == 0)
        {
            _logger?.LogWarning($"{nameof(SaveImportantMessages)} aborted: {nameof(importantMessages)} is empty");
            return "Список сообщений для сохранения пуст.";
        }
        
        var errors = new List<string>();
        int savedCount = 0;

        foreach (var messageDto in importantMessages)
        {
            if (_context.TargetChannel != null && messageDto.PeerId == _context.TargetChannel.Id)
            {
                var err = $"Ошибка: Попытка сохранить сообщение из целевого канала ({messageDto.PeerId}). Операция запрещена.";
                _logger?.LogWarning(err);
                errors.Add(err);
                continue;
            }
            
            // Проверка: существует ли такой канал в контексте
            if (!_context.Channels.TryGetValue(messageDto.PeerId, out var channel))
            {
                var err = $"Ошибка: Канал с ID {messageDto.PeerId} не найден в списке каналов.";
                errors.Add(err);
                continue;
            }

            // Проверка: есть ли такое сообщение в загруженной истории канала
            var messageExists = channel.LastMessages.Any(m => m.Id == messageDto.Id);
            if (!messageExists)
            {
                var err = $"Ошибка: Сообщение с ID {messageDto.Id} отсутствует в канале {channel.Id}.";
                errors.Add(err);
                continue;
            }
            
            _publishService.AddMessage(messageDto);
            savedCount++;
        }

        _logger?.LogInformation($"{nameof(SaveImportantMessages)} finished. Saved {savedCount}/{importantMessages.Count}");
        
        if (errors.Count > 0)
        {
            return $"; Сохранено: {savedCount} из {importantMessages.Count}. Ошибки: {string.Join("; ", errors)}";
        }

        return $"Успешно сохранено для публикации: {savedCount} из {importantMessages.Count}";
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