namespace LTGTrashSorter.TG.Services.Interfaces;

public interface IAuthService
{
    Task<bool> AuthorizeAsync();
}