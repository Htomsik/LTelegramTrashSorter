using LTGTrashSorter.Core.Interfaces;
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
    private readonly IMessageService _messageService;
    private readonly IScheduleService _scheduleService;
    private readonly IPublishService _publishService;
    
    private readonly BotContext _botContext;
    private readonly Settings _settings;
    
    public Worker(ILogger<Worker> logger, 
        IOptions<Settings> settings,
        IChannelService channelService,
        IContentAnalyzerService analyzerService,
        IDialogService dialogService,
        ITelegramStateService tgStateService,
        IMessageService messageService,
        IScheduleService scheduleService,
        IPublishService publishService,
        BotContext  botContext)
    {
        _logger = logger;
        _channelService = channelService;
        _analyzerService = analyzerService;
        _dialogService = dialogService;
        _tgStateService = tgStateService;
        _messageService = messageService;
        _scheduleService = scheduleService;
        _publishService = publishService;
        _botContext = botContext;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var schedule = _scheduleService.GetNextExecutionInfo();
                _logger.LogInformation("Next loop scheduled in {timeSpan} (at {nextTime:HH:mm:ss})", 
                    schedule.Delay, schedule.TargetTime);
                
                #if RELEASE
                await Task.Delay(schedule.Delay, stoppingToken);
                #endif
                
                if (stoppingToken.IsCancellationRequested) 
                    break;
                
                _logger.LogWarning("Start new loop at: {time}", DateTimeOffset.Now);
                _logger.LogInformation("Scrap last dialogs at: {time}", DateTimeOffset.Now);
                var loaded= await _dialogService.LoadAsync();
                if (!loaded || _botContext.TargetChannel == null)
                {
                    _logger.LogWarning("Can't load dialogs or current channel (ID: {TargetId}) not found. Skip worker turn", 
                        _settings.Telegram.TargetChannelId);
                    
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
                var analyzerResult = await _analyzerService.AnalyzeAndProcessNewMessagesAsync(stoppingToken);
                
                // TODO move to logging service
                if (_settings.Telegram.LogChannelId != 0 && !string.IsNullOrEmpty(analyzerResult.Message))
                {
                    var nextSchedule = _scheduleService.GetNextExecutionInfo();
                    await _publishService.PublishBatchAsync(
                        analyzerResult.ChannelsCount,
                        analyzerResult.MessagesCount,
                        schedule.TargetTime
                    );
                    
                    var logText = $"📊 Report ({DateTime.UtcNow:dd.MM.yyyy HH:mm UTC})\n" +
                                  $"📁 Channels processed: {analyzerResult.ChannelsCount}\n" +
                                  $"💬 Messages analyzed: {analyzerResult.MessagesCount}\n" +
                                  $"⏰ Next run: {nextSchedule.TargetTime:dd.MM.yyyy HH:mm}\n" +
                                  $"📝 Analyzer message: {analyzerResult.Message}";
                    
                    
                    long targetId = 0;
                    #if DEBUG
                        targetId = _botContext.Me?.id ?? 0;
                    #else
                        targetId = _settings.Telegram.LogChannelId;
                    #endif
                    
                    await _messageService.SendMessageAsync(targetId, logText);
                }
                
                _logger.LogInformation("Save last dialogs to db at: {time}", DateTimeOffset.Now);
                await _tgStateService.SaveStateFromContextAsync();
                
                _logger.LogWarning("End loop at: {time}", DateTimeOffset.Now);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Worker Error");
                throw;
            }
            
            #if DEBUG
                return;
            #endif
        }
    }
    
}