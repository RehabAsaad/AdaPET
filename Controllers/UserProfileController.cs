using AdaPET.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AdaPET.Controllers
{
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UserProfileController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: UserProfile/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get user data with their animals and doctor info
            var user = await _context.Users
                .Include(u => u.Animals)
                .Include(u => u.Doctor)
                    .ThenInclude(d => d.Clinics)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Prepare ViewModel
            var userProfile = new UserProfileViewModel
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.phone,
                UserRole = user.UserRole,
                Specialization = user.Doctor != null ? user.Doctor.Specialization : null,
                PhotoURL = user.PhotoURL,
                Animals = user.Animals.ToList()
            };

            // Add clinics list if doctor
            if (user.Doctor != null && user.Doctor.Clinics != null && user.Doctor.Clinics.Any())
            {
                userProfile.ClinicsList = string.Join(", ", user.Doctor.Clinics.Select(c => c.Name));
            }

            return View(userProfile);
        }

        // GET: UserProfile/MyProfile
        public IActionResult MyProfile()
        {
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

        // ========== GET: Edit Profile ==========
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["Error"] = "Please login first.";
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.Users
                .Include(u => u.Doctor)
                    .ThenInclude(d => d.Clinics)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.phone,
                UserRole = user.UserRole,
                ExistingPhotoURL = user.PhotoURL
            };

            if (user.UserRole == "Doctor" && user.Doctor != null)
            {
                model.Specialization = user.Doctor.Specialization;

                if (user.Doctor.Clinics != null && user.Doctor.Clinics.Any())
                {
                    model.ClinicsList = string.Join(", ", user.Doctor.Clinics.Select(c => c.Name));
                }
            }

            return View(model);
        }

        // ========== POST: Edit Profile (Save changes) ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileViewModel model)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["Error"] = "Please login first.";
                return RedirectToAction("Login", "Account");
            }

            if (model.UserId != userId)
            {
                TempData["Error"] = "You can only edit your own profile.";
                return RedirectToAction("Details", new { id = userId });
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .Include(u => u.Doctor)
                    .ThenInclude(d => d.Clinics)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            // ===== 1. Update basic user info =====
            user.Name = model.Name;
            user.Email = model.Email;
            user.phone = model.Phone;

            // ===== 2. Handle profile photo upload =====
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(model.PhotoFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("PhotoFile", "Only image files (jpg, jpeg, png, gif, webp) are allowed.");
                    return View(model);
                }

                // Validate file size (max 5MB)
                if (model.PhotoFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("PhotoFile", "File size must be less than 5MB.");
                    return View(model);
                }

                // Delete old photo if exists (from images folder)
                if (!string.IsNullOrEmpty(user.PhotoURL))
                {
                    string oldPhotoPath = Path.Combine(_webHostEnvironment.WebRootPath,
                        "images", Path.GetFileName(user.PhotoURL));
                    if (System.IO.File.Exists(oldPhotoPath))
                    {
                        System.IO.File.Delete(oldPhotoPath);
                    }
                }

                // Create directory if not exists (images folder)
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Save new photo
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.PhotoFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.PhotoFile.CopyToAsync(fileStream);
                }

                // Store relative path with /images/
                user.PhotoURL = "/images/" + uniqueFileName;
            }

            // ===== 3. If Doctor, update doctor data =====
            if (user.UserRole == "Doctor")
            {
                if (user.Doctor == null)
                {
                    user.Doctor = new Doctor
                    {
                        UserId = user.Id,
                        Specialization = model.Specialization ?? "",
                        Clinics = new List<Clinic>()
                    };
                    _context.Doctors.Add(user.Doctor);
                }
                else
                {
                    user.Doctor.Specialization = model.Specialization ?? "";
                }

                if (user.Doctor.Clinics != null && user.Doctor.Clinics.Any())
                {
                    _context.Clinics.RemoveRange(user.Doctor.Clinics);
                }

                if (!string.IsNullOrEmpty(model.ClinicsList))
                {
                    var clinicNames = model.ClinicsList.Split(',')
                        .Select(c => c.Trim())
                        .Where(c => !string.IsNullOrEmpty(c));

                    foreach (var clinicName in clinicNames)
                    {
                        user.Doctor.Clinics.Add(new Clinic
                        {
                            Name = clinicName,
                            Description = null,
                            location = "",
                            Phone = "",
                            DoctorId = user.Id
                        });
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Details), new { id = userId });
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "An error occurred while saving. Please try again.";
                Console.WriteLine($"Error saving profile: {ex.Message}");
                return View(model);
            }
        }

        // ========== LOGOUT ========== 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign out the user
            await HttpContext.SignOutAsync();

            // Clear session
            HttpContext.Session.Clear();

            // Clear TempData
            TempData.Clear();

            // ✅ Redirect with query parameter to show message on Home page
            return RedirectToAction("Index", "Home", new { logout = "success" });
        }

        // ========== DELETE ACCOUNT ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            // Get logged-in user ID
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                TempData["Error"] = "Please login first.";
                return RedirectToAction("Login", "Account");
            }

            // Get user with all related data
            var user = await _context.Users
                .Include(u => u.Animals)      // حيوانات المستخدم
                .Include(u => u.Doctor)
                    .ThenInclude(d => d.Clinics)  // عيادات الدكتور
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            try
            {
                // 1. Delete profile photo if exists
                if (!string.IsNullOrEmpty(user.PhotoURL))
                {
                    string photoPath = Path.Combine(_webHostEnvironment.WebRootPath,
                        "images", Path.GetFileName(user.PhotoURL));
                    if (System.IO.File.Exists(photoPath))
                    {
                        System.IO.File.Delete(photoPath);
                    }
                }

                // 2. Delete all animal photos
                if (user.Animals != null && user.Animals.Any())
                {
                    foreach (var animal in user.Animals)
                    {
                        if (!string.IsNullOrEmpty(animal.ImgURL))
                        {
                            string animalPhotoPath = Path.Combine(_webHostEnvironment.WebRootPath,
                                "images", Path.GetFileName(animal.ImgURL));
                            if (System.IO.File.Exists(animalPhotoPath))
                            {
                                System.IO.File.Delete(animalPhotoPath);
                            }
                        }
                    }
                }

                // 3. If Doctor, delete clinics first (due to foreign key constraint)
                if (user.UserRole == "Doctor" && user.Doctor != null)
                {
                    if (user.Doctor.Clinics != null && user.Doctor.Clinics.Any())
                    {
                        _context.Clinics.RemoveRange(user.Doctor.Clinics);
                    }
                    _context.Doctors.Remove(user.Doctor);
                }

                // 4. Delete all animals
                if (user.Animals != null && user.Animals.Any())
                {
                    _context.Animals.RemoveRange(user.Animals);
                }

                // 5. Finally delete the user
                _context.Users.Remove(user);

                // Save all changes
                await _context.SaveChangesAsync();

                // 6. Sign out the user
                await HttpContext.SignOutAsync();
                HttpContext.Session.Clear();

                TempData.Clear();
                return RedirectToAction("Index", "Home", new { deleted = "success" });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while deleting your account. Please try again.";
                Console.WriteLine($"Error deleting account: {ex.Message}");
                return RedirectToAction(nameof(Details), new { id = userId });
            }
        }

        private bool IsAuthenticated()
        {
            return User.Identity != null && User.Identity.IsAuthenticated;
        }
    }
}