namespace LTGTrashSorter.TG.Models.DTO;


public record ChannelMessagesDto(
    long ChannelId, 
    string ChannelName, 
    List<TelegramMessageDto> Messages
);

public record TelegramMessageDto(
    long Id, 
    string Text
);
