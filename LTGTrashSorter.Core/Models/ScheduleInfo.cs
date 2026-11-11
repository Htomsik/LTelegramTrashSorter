namespace LTGTrashSorter.Core.Models;

public record ScheduleInfo(
    TimeSpan Delay, 
    DateTimeOffset TargetTime
);