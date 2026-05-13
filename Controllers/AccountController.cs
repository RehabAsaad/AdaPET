using AdaPET.Models;
using AdaPET.Models.ViewModels;
using AdaPET.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdaPET.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        

        public AccountController(IAuthService authService)
        {
            _authService = authService;
            
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                
                var result = await _authService.RegisterAsync(model);

                if (result.Success && result.User != null)
                {

                    await SignInUser(result.User, false);
                    HttpContext.Session.SetInt32("UserId", result.User.Id);
                    TempData["Success"] = "Registration Success!";
                    return RedirectToAction("Index", "Animals");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
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

            if (result.Success && result.User != null)
            {
                await SignInUser(result.User, model.RememberMe);
                HttpContext.Session?.SetInt32("UserId", result.User.Id);

                TempData["Success"] = "Welcome back! You have successfully logged in.";

                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Animals");
            }

            // ✅ إضافة رسائل خطأ محددة لكل حالة
            if (result.ErrorMessage.Contains("Email not found"))
            {
                ModelState.AddModelError("Email", result.ErrorMessage);
                ModelState.AddModelError("", "Email not registered. Would you like to create an account?");
            }
            else if (result.ErrorMessage.Contains("Incorrect password"))
            {
                ModelState.AddModelError("Password", result.ErrorMessage);
                ModelState.AddModelError("", "Incorrect password. Please try again.");
            }
            else
            {
                ModelState.AddModelError("", result.ErrorMessage ?? "Invalid email or password");
            }

            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["Success"] = "Logout success!";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInUser(User user, bool isPersistent = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.UserRole ?? "User")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(isPersistent ? 1440 : 30)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );
        }
    }
}