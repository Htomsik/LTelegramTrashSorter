using System.ComponentModel;
using LTGTrashSorter.Core.Interfaces;
using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Models.DTO;
using LTGTrashSorter.TG.Services.Interfaces;
using Newtonsoft.Json;

namespace LTGTrashSorter.TG.Tools;

public class TelegramTools : ITelegramTools
{
    private readonly BotContext _context;
    private readonly IMessageService _messageSender;

    public TelegramTools(BotContext context, IMessageService messageSender)
    {
        _context = context;
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
    
    [Description("Получить список последних сообщений во всех каналах для последующего анализа.")]
    public async Task<string> GetLastChannelsMessages()
    {
        if (_context.TelegramClient == null)
            return "Telegram клиент не авторизован.";

        List<ChannelMessagesDto> channelMessagesDto = new ();

        foreach (var channel in _context.Channels.Values)
        {
            if(channel.LastMessages.Count == 0 )
                continue;
            
            var messageDto = new ChannelMessagesDto(
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
    
    [Description("Переслать конкретное сообщение из канала пользователю в чат. Обязательно вызывай этот инструмент для каждого найденного важного сообщения.")]
    public async Task<string> ForwardMessage(
        [Description("ID канала-источника")] long fromChannelId, 
        [Description("ID сообщения в канале")] int messageId)
    {
        long targetId = _context.Me?.id ?? 0;
        if (targetId == 0)
        {
            return $"Ошибка при пересылке сообщения.";
        }
        
        bool success = await _messageSender.ForwardMessageByIdsAsync(fromChannelId, targetId, messageId);
    
        return success 
            ? $"Сообщение успешно переслано." 
            : $"Ошибка при пересылке сообщения";
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