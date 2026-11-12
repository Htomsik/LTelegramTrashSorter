using LTGTrashSorter.Core.Interfaces;
using LTGTrashSorter.Core.Models;
using Microsoft.Extensions.Options;

namespace LTGTrashSorter.Worker.Services;

public class ScheduleService : IScheduleService
{
    private readonly ILogger<ScheduleService> _logger;
    private readonly Settings _settings;

    public ScheduleService(IOptions<Settings> settings, ILogger<ScheduleService> logger)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public ScheduleInfo GetNextExecutionInfo()
    {
        var hours = _settings.Worker.ExecutionHours;
        if (hours == null || hours.Length == 0)
        {
            _logger.LogWarning("ExecutionHours is empty or not configured. Falling back to DefaultTimeout ({timeout} ms).", 
                _settings.Worker.DefaultTimeout);
        
            var fallbackDelay = TimeSpan.FromMilliseconds(_settings.Worker.DefaultTimeout);
            var fallbackTime = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _settings.Worker.TimeZone).Add(fallbackDelay);
            return new ScheduleInfo(fallbackDelay, fallbackTime);
        }

        var sortedHours = hours.OrderBy(h => h).ToArray();
        var now = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, _settings.Worker.TimeZone);

        int? targetHour = sortedHours.Cast<int?>().FirstOrDefault(h => h > now.Hour);
        bool isNextDay = false;

        if (!targetHour.HasValue)
        {
            targetHour = sortedHours.First();
            isNextDay = true;
        }

        int hourDifference = targetHour.Value - now.Hour;
        if (isNextDay)
        {
            hourDifference += 24;
        }

        var delay = TimeSpan.FromHours(hourDifference) 
                    - TimeSpan.FromMinutes(now.Minute) 
                    - TimeSpan.FromSeconds(now.Second);

        var targetTime = now.Add(delay);

        return new ScheduleInfo(delay, targetTime);
    }
}