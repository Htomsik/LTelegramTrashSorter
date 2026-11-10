using LTGTrashSorter.Core.Models;
using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Services.Interfaces;
using LTGTrashSorter.Worker.Services;
using Microsoft.Extensions.Options;

namespace LTGTrashSorter.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IChannelService _channelService;
    private readonly IContentAnalyzerService _analyzerService;
    private readonly IDialogService _dialogService;
    private readonly ITelegramStateService _tgStateService;

    private readonly BotContext _botContext;
    private readonly Settings _settings;

    public Worker(ILogger<Worker> logger, 
        IOptions<Settings> settings,
        IChannelService channelService,
        IContentAnalyzerService analyzerService,
        IDialogService dialogService,
        ITelegramStateService tgStateService,
        BotContext  botContext)
    {
        _logger = logger;
        _channelService = channelService;
        _analyzerService = analyzerService;
        _dialogService = dialogService;
        _tgStateService = tgStateService;
        _botContext = botContext;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogWarning("Start new loop at: {time}", DateTimeOffset.Now);
                
                _logger.LogInformation("Scrap last dialogs at: {time}", DateTimeOffset.Now);
                var loaded= await _dialogService.LoadAsync();
                
                if (!loaded || _botContext.TargetChannel == null)
                {
                    _logger.LogWarning("Can't load dialogs or current channel (ID: {TargetId}) not found. Skip worker turn", 
                        _settings.Telegram.TargetChannelId);
                    
                    await Task.Delay(_settings.Worker.Timeout, stoppingToken);
                    continue;
                }
                
                _logger.LogInformation("Scrap last dialogs state from db at: {time}", DateTimeOffset.Now);
                await _tgStateService.LoadStateIntoContextAsync();
                
                _logger.LogInformation("Scrap last messages at: {time}", DateTimeOffset.Now);
                // Target channel messages
                await _channelService.LoadLastMessagesAsync(_botContext.TargetChannel.Id);
                // All channels
                foreach (var channel in _botContext.Channels)
                {
                    await _channelService.LoadLastMessagesAsync(channel.Value.Id, channel.Value.LastProcessedMessageId);
                }
                
                _logger.LogInformation("Analyze scrapped messages at: {time}", DateTimeOffset.Now);
                await _analyzerService.AnalyzeAndProcessNewMessagesAsync(stoppingToken);
                
                _logger.LogInformation("Save last dialogs to db at: {time}", DateTimeOffset.Now);
                await _tgStateService.SaveStateFromContextAsync();
                
                _logger.LogWarning("End loop at: {time}", DateTimeOffset.Now);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Worker Error");
                throw;
            }
            await Task.Delay(_settings.Worker.Timeout, stoppingToken);
        }
    }
}