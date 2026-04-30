using AdaPET.Models;
using AdaPET.Models.ViewModels;

namespace AdaPET.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string ErrorMessage, User? User)> RegisterAsync(RegisterViewModel model);
        Task<(bool Success, string ErrorMessage, User? User)> LoginAsync(LoginViewModel model);
    }
}