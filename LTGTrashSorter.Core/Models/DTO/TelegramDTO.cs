using System.ComponentModel;

namespace LTGTrashSorter.Core.Models.DTO;


public record TelegramMessagesDto(
    [Description("ID источника")] long PeerId, 
    [Description("Наименование источника")] string PeerName, 
    [Description("Флаг целевого канала (true — это канал, который надо использовать для дедупликации; false — канал-источник новостей)")] bool IsTargetChannel, 
    [Description("Сообщения из источника")] List<TelegramMessageDto> Messages
);

public record TelegramMessageDto(
    [Description("ID сообщения")] int Id,  
    [Description("Текст сообщения")] string Text  
);

public record TelegramImportantMessageDto(
    [Description("ID источника")] long PeerId, 
    [Description("ID сообщения в источнике")] int Id
);
