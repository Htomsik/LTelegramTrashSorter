using LTGTrashSorter.Core.Interfaces;
using LTGTrashSorter.DB.Models;
using LTGTrashSorter.DB.Repository;
using LTGTrashSorter.TG.Core;
using Microsoft.Extensions.Logging;

namespace LTGTrashSorter.Worker.Services;

public class TelegramDbStateService : ITelegramStateService
{
    private readonly BotContext _botContext;
    private readonly ITelegramChannelRepository _channelRepository;
    private readonly ILogger<TelegramDbStateService>? _logger;

    public TelegramDbStateService(
        BotContext botContext, 
        ITelegramChannelRepository channelRepository,
        ILogger<TelegramDbStateService>? logger = null)
    {
        _botContext = botContext;
        _channelRepository = channelRepository;
        _logger = logger;
    }

    public async Task<bool> LoadStateIntoContextAsync()
    {
        try
        {
            var dbRecords = await _channelRepository.GetAllAsync();
            var dbChannels = dbRecords.ToList();
            
            if (!dbChannels.Any())
                return true; 
            
            foreach (var channel in _botContext.Channels.Values)
            {
                var record = dbChannels.FirstOrDefault(x => x.Id == channel.Id);
                if (record == null)
                    continue;
                
                // TODO Add mapper
                channel.LastProcessedMessageId = record.LastProcessedMessageId;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to load TG state into context from DB.");
            return false;
        }
    }

    public async Task<bool> SaveStateFromContextAsync()
    {
        try
        {
            foreach (var channel in _botContext.Channels.Values)
            {
                // TODO Add mapper
                await _channelRepository.SaveAsync(new TelegramChannelEntity
                {
                    Id = channel.Id,
                    LastProcessedMessageId = channel.LastProcessedMessageId
                });
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to save TG state from context to DB.");
            return false;
        }
    }
}