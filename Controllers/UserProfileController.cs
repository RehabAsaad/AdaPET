using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using AdaPET.Models;
using System.Security.Claims;

namespace AdaPET.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: UserProfile/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get user data with their animals
            var userProfile = await _context.Users
                .Include(u => u.Animals)  // Include animals
                .Include(u => u.Doctor)   // Include doctor info if exists
                .Where(u => u.Id == id)
                .Select(u => new UserProfileViewModel
                {
                    UserId = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Phone = u.phone,
                    UserRole = u.UserRole,
                    Specialization = u.Doctor != null ? u.Doctor.Specialization : null,
                    Animals = u.Animals.ToList()
                })
                .FirstOrDefaultAsync();

            if (userProfile == null)
            {
                return NotFound();
            }

            return View(userProfile);
        }

        // GET: UserProfile/MyProfile
        public IActionResult MyProfile()
        {
            // Get the current logged-in user ID from claims
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdClaim))
            {
                TempData["Error"] = "Please login to view your profile.";
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdClaim, out int userId))
            {
                TempData["Error"] = "Invalid user ID.";
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction(nameof(Details), new { id = userId });
        }
    }
}