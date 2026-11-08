using LTGTrashSorter.AI.Services;
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

    private readonly BotContext _botContext;
    private readonly Settings _settings;

    public Worker(ILogger<Worker> logger, 
        IOptions<Settings> settings,
        IChannelService channelService,
        IContentAnalyzerService analyzerService,
        IDialogService dialogService,
        BotContext  botContext)
    {
        _logger = logger;
        _channelService = channelService;
        _analyzerService = analyzerService;
        _dialogService = dialogService;
        _botContext = botContext;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("Scrap last dialogs at: {time}", DateTimeOffset.Now);
                
                var loaded= await _dialogService.LoadAsync();
                if (!loaded || _botContext.TargetChannel == null)
                {
                    _logger.LogWarning("Can't load dialogs or current channel (ID: {TargetId}) not found. Skip worker turn", 
                        _settings.Telegram.TargetChannelId);
                    
                    await Task.Delay(_settings.Worker.Timeout, stoppingToken);
                    continue;
                }
                
                _logger.LogInformation("Scrap last messages at: {time}", DateTimeOffset.Now);
                foreach (var channel in _botContext.Channels)
                {
                    await _channelService.LoadLastMessagesAsync(channel.Value.Id);
                }
            
                _logger.LogInformation("Analyze scrapped messages at: {time}", DateTimeOffset.Now);
                await _analyzerService.AnalyzeAndProcessNewMessagesAsync(stoppingToken);
                
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