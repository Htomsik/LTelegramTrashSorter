using LTGTrashSorter.Core.Models;

namespace LTGTrashSorter.Core.Interfaces;

public interface IScheduleService
{
    ScheduleInfo GetNextExecutionInfo();
}