using LTGTrashSorter.TG.Core;
using LTGTrashSorter.TG.Services.Interfaces;
using WTelegram;

namespace LTGTrashSorter.TG.Services
{
    public class AuthService : IAuthService
    {
        private readonly BotContext _context;
        private readonly Func<string> _verificationCodeAction;
        
        public AuthService(BotContext context, Func<string> verificationCodeAction)
        {
            _context = context;
            _verificationCodeAction = verificationCodeAction;
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
                case "api_id": return _context.AuthorizationData.ApiId;
                case "api_hash": return _context.AuthorizationData.ApiHash;
                case "password":  return _context.AuthorizationData.Password;
                case "phone_number": return _context.AuthorizationData.PhoneNumber;
                case "verification_code": return _verificationCodeAction.Invoke();
                case "proxy": return "http://127.0.0.1:7890";
                default: return null;  
            }
        }
    }
} 