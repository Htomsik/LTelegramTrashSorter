
using LTGTrashSorter.Core.Interfaces;
using LTGTrashSorter.Core.Models;
using LTGTrashSorter.Core.Models.DTO;
using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace LTGTrashSorter.TG.Services;

public class PublishService : IPublishService
{
    private readonly BotContext _context;
    private readonly IMessageService _messageService;
    private readonly ILogger<PublishService> _logger;
    private readonly Settings _settings;
    
    private readonly List<TelegramImportantMessageDto> _buffer = new();

    public PublishService(
        BotContext context, 
        IMessageService messageService, 
        IOptions<Settings> settings,
        ILogger<PublishService> logger)
    {
        _context = context;
        _messageService = messageService;
        _settings = settings.Value;
        _logger = logger;
    }

    public void AddMessage(TelegramImportantMessageDto message)
    {
        _buffer.Add(message);
    }

    public async Task PublishBatchAsync(int channelsCount, int messagesCount, DateTimeOffset publishedAt)
    {
        // if (_buffer.Count == 0)
        // {
        //     _logger.LogInformation("No important posts to publish in this cycle.");
        //     return;
        // }

        long targetId = 0;
        #if DEBUG
            targetId = _context.Me?.id ?? 0;
        #else
            targetId = _context.TargetChannel?.Id ?? 0;
        #endif

        if (targetId == 0)
        {
            _logger.LogError("Publish aborted: targetId is empty.");
            _buffer.Clear();
            return;
        }
        
        // Send header
        var headerText = $"🌐 Выпуск новостей 🌐 \n\n" +
                         $"📅 Дата: {publishedAt:dd.MM.yyyy}\n" +
                         $"🕒 Время: {publishedAt:HH:mm}\n\n" +
                         $"📁 Обработано каналов: {channelsCount}\n" +
                         $"💬 Проанализировано постов: {messagesCount}\n" +
                         $"📌 Отобрано важных: {_buffer.Count}\n\n" +
                         $"{_settings.Telegram.TargetChannelLinkText}";
        
        await _messageService.SendMessageWithLinkAsync(
            targetId, 
            headerText, 
            linkText: _settings.Telegram.TargetChannelLinkText, 
            url: _settings.Telegram.TargetChannelLinkUrl
        );
        
        // Forwarding without sound 
        int successCount = 0;
        foreach (var messageDto in _buffer)
        {
            var success = await _messageService.ForwardMessageByIdsAsync(
                messageDto.PeerId, 
                targetId, 
                messageDto.Id, 
                clearPeerData: false,
                isSilent: true
            );
            
            if (success) 
                successCount++;
        }

        _logger.LogInformation("Published {success}/{total} important posts.", successCount, _buffer.Count);
        
        _buffer.Clear();
    }
}