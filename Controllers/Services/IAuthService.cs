using AdaPET.Models;
using AdaPET.Models.ViewModels;

namespace AdaPET.Services
{
    public interface IAuthService
    {
        // ✅ تغيير نوع الإرجاع ليشمل قائمة الأخطاء
        Task<(bool Success, List<string> Errors, User? User)> RegisterAsync(RegisterViewModel model);
        Task<(bool Success, string ErrorMessage, User? User)> LoginAsync(LoginViewModel model);
    }
}