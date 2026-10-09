using LTGTrashSorter.Core.Interfaces;
using LTGTrashSorter.Core.Models;
using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Services.Interfaces;
using Microsoft.Extensions.Options;
using WTelegram;

namespace LTGTrashSorter.TG.Services
{
    public class AuthService : IAuthService
    {
        private readonly BotContext _context;
        private readonly IDataRequestService _dataRequestService;
        private readonly Settings _settings;
        
        public AuthService(BotContext context, IOptions<Settings> settings,  IDataRequestService dataRequestService )
        {
            _settings = settings.Value;
            _context = context;
            _dataRequestService = dataRequestService;
        }
        
        public async Task<bool> AuthorizeAsync()
        {
            try
            {
                // Create client if needed
                if (_context.TelegramClient == null)
                    _context.TelegramClient = new Client(Config);
                
                // Connect to Telegram servers
                await _context.TelegramClient.ConnectAsync();
                
                // Authorize
                _context.Me = await _context.TelegramClient.LoginUserIfNeeded();
                
                if (_context.Me == null)
                    return false;
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        // Configuration for Telegram authorization
        private string? Config(string what)
        {
            switch (what)
            {
                case "api_id": return _settings.Telegram.ApiId;
                case "api_hash": return _settings.Telegram.ApiHash;
                case "password":  return _settings.Telegram.Password;
                case "phone_number": return _settings.Telegram.PhoneNumber;
                case "verification_code": return _dataRequestService.GetData("verification_code");
                case "proxy": return "http://127.0.0.1:7890";
                default: return null;  
            }
        }
    }
} 