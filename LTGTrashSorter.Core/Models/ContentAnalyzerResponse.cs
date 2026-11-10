namespace LTGTrashSorter.Core.Models;

public record ContentAnalyzerResult(string Message, bool Result, int ChannelsCount = 0, int MessagesCount = 0);