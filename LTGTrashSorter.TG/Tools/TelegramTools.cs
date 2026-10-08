using System.ComponentModel;
using LTGTrashSorter.Core.Interfaces;
using LTGTrashSorter.TG.Core;
using Newtonsoft.Json;

namespace LTGTrashSorter.TG.Tools;

public class TelegramTools : ITelegramTools
{
    private readonly BotContext _context;

    public TelegramTools(BotContext context)
    {
        _context = context;
    }
    
    [Description("Название: Список каналов Функция: Получение списка всех каналов на которые подписан пользователь")]
    public async Task<string> GetProcessChannelsList()
    {
        if (_context.TelegramClient == null)
            return "Telegram клиент не авторизован.";

        string json;
        try
        {
            json = JsonConvert.SerializeObject(_context.Channels);
            Console.WriteLine(json);
        }
        catch (Exception ex)
        {
            return "Ошибка чтения TG";
        }
        
        return json;
    }
}