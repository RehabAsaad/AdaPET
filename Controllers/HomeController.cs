using AdaPET.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AdaPET.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(bool? logout, bool? deleted)
        {
            // ✅ Check if this is a logout redirect
            if (logout == true)
            {
                ViewBag.LogoutMessage = "You have been logged out successfully.";
            }

            // ✅ Check if this is a delete account redirect
            if (deleted == true)
            {
                ViewBag.LogoutMessage = "Your account has been permanently deleted.";
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult homeFeed()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}