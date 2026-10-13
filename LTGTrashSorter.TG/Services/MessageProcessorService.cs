using System.Text.RegularExpressions;
using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Services.Interfaces;
using TL;

namespace LTGTrashSorter.TG.Services;

public class MessageProcessorService : IMessageProcessorService
{
    private readonly BotContext _context;
    
    private readonly IMessageService _messageService;

    public Func<string, string>? CreateMessageToUserAction { get; set; }

    public MessageProcessorService(BotContext context, IMessageService messageService)
    {
        _context = context;
        _messageService = messageService;
    }

    public async Task ProcessUpdatesAsync(UpdatesBase updates)
    {
        if (updates.UpdateList == null)
            return;
            
        foreach (var update in updates.UpdateList)
        {
            if (update is UpdateNewMessage unm)
            {
                await ProcessMessageAsync(unm);
            }
        }
    }
    
    private async Task ProcessMessageAsync(UpdateNewMessage unm)
    {
        if (unm.message is not Message msg)
            return;
            
        long targetUserId = 0;
            
        if (unm.message.From == null)
        {
            {
                targetUserId = unm.message.Peer.ID;
            }
        }
        else
        {
            targetUserId = unm.message.From.ID;
        }

        if (targetUserId == 0)
            return;
            
        // Find user in our list
        var ret = _context.Users.TryGetValue(targetUserId, out var user);
        if (!ret || user == null)
        {
            return;
        }
            
        string messageText = msg.message;
        if (string.IsNullOrWhiteSpace(messageText))
            return;
            
        if (messageText.ToLower().StartsWith("/v"))
        {
            messageText = messageText.Replace("/v", "", StringComparison.OrdinalIgnoreCase).Trim();
            
            if (CreateMessageToUserAction != null)
            {
                // Вызываем и получаем результат
                string jsonAnswer = CreateMessageToUserAction(messageText);
                string formattedJson = Regex.Unescape(jsonAnswer);
                    
                Console.WriteLine("formattedJson: " + formattedJson);
                await _messageService.SendMessageAsync(user.Id, formattedJson);
            }
        }
    }
}