using AdaPET.Models;
using AdaPET.Models.ViewModels;
using AdaPET.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdaPET.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;

        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }

        // ==================== REGISTER ====================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // محاولة التسجيل
                var result = await _authService.RegisterAsync(model);

                if (result.Success)
                {
                    TempData["Success"] = "Account created successfully!";
                    return RedirectToAction("Login"); // ✅ مسار 1
                }

                ModelState.AddModelError("", result.ErrorMessage);
                return View(model); // ✅ مسار 2
            }

            return View(model); // ✅ مسار 3 (لما الـ ModelState يكون Invalid)
        }

        // ==================== LOGIN ====================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _authService.LoginAsync(model);

            if (result.Success)
            {
                // TODO: إضافة Session أو Cookie أو JWT هنا
                TempData["Success"] = "Login successful!";
                return RedirectToAction("homeFeed", "Home");
            }

            ModelState.AddModelError("", result.ErrorMessage);
            return View(model);
        }

        // ==================== FORGOT PASSWORD ====================
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        
    }
}