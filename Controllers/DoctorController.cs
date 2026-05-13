using AdaPET.Models;
using AdaPET.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdaPET.Controllers
{
    [Authorize]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUserRole = User.FindFirstValue(ClaimTypes.Role);

            int parsedUserId = 0;
            if (!string.IsNullOrEmpty(currentUserId))
            {
                int.TryParse(currentUserId, out parsedUserId);
            }

            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Clinics)
                .Where(d => d.User != null)
                .Select(d => new DoctorClinicViewModel
                {
                    DoctorId = d.UserId,
                    DoctorName = d.Name,
                    Specialization = d.Specialization,
                    Clinics = d.Clinics.Select(c => new ClinicInfoViewModel
                    {
                        ClinicId = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Location = c.location,
                        Phone = c.Phone,
                        Schedule = c.Schedule,
                        CanEdit = currentUserRole == "Doctor" && currentUserId == d.UserId.ToString()
                    }).ToList()
                })
                .ToListAsync();

            var viewModel = new DoctorsListViewModel
            {
                Doctors = doctors,
                CurrentUserId = parsedUserId,
                CurrentUserRole = currentUserRole ?? ""
            };

            return View(viewModel);
        }

        // ✅ إضافة عيادة جديدة (GET)
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> CreateClinic()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized();
            }

            var clinic = new Clinic
            {
                DoctorId = userId,
                Name = "",
                location = "",
                Phone = "",
                Description = "",
                Schedule = ""
            };

            return View(clinic);
        }

        // ✅ إضافة عيادة جديدة (POST)
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClinic([Bind("Name,Description,location,Phone,Schedule")] Clinic clinic)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized();
            }

            clinic.DoctorId = userId;

            ModelState.Remove("Doctor");
            ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Clinics.Add(clinic);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Clinic added successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["Error"] = $"Error saving clinic: {ex.Message}";
                    return View(clinic);
                }
            }

            return View(clinic);
        }

        // ✅ تعديل عيادة (GET)
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> EditClinic(int id)
        {
            var clinic = await _context.Clinics
                .Include(c => c.Doctor)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clinic == null)
            {
                TempData["Error"] = "Clinic not found.";
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized();
            }

            if (clinic.DoctorId != userId)
            {
                TempData["Error"] = "You can only edit your own clinics.";
                return RedirectToAction(nameof(Index));
            }

            return View(clinic);
        }

        // ✅ تعديل عيادة (POST) - معدل للحفاظ على البيانات
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditClinic(int id, [Bind("Id,Name,Description,location,Phone,Schedule")] Clinic clinic)
        {
            if (id != clinic.Id)
            {
                return NotFound();
            }

            var existingClinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existingClinic == null)
            {
                TempData["Error"] = "Clinic not found.";
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized();
            }

            if (existingClinic.DoctorId != userId)
            {
                TempData["Error"] = "You can only edit your own clinics.";
                return RedirectToAction(nameof(Index));
            }

            // ✅ تحديث جميع الحقول مع الحفاظ على البيانات الموجودة
            existingClinic.Name = string.IsNullOrEmpty(clinic.Name) ? existingClinic.Name : clinic.Name;
            existingClinic.Description = clinic.Description ?? existingClinic.Description;
            existingClinic.location = string.IsNullOrEmpty(clinic.location) ? existingClinic.location : clinic.location;
            existingClinic.Phone = string.IsNullOrEmpty(clinic.Phone) ? existingClinic.Phone : clinic.Phone;
            existingClinic.Schedule = clinic.Schedule ?? existingClinic.Schedule;

            try
            {
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Clinic information updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClinicExists(clinic.Id))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error saving changes: {ex.Message}";
                return View(clinic);
            }
        }

        // ✅ حذف عيادة
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClinic(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);

            if (clinic == null)
            {
                TempData["Error"] = "Clinic not found.";
                return RedirectToAction(nameof(Index));
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(currentUserId) || !int.TryParse(currentUserId, out int userId))
            {
                return Unauthorized();
            }

            if (clinic.DoctorId != userId)
            {
                TempData["Error"] = "You can only delete your own clinics.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Clinics.Remove(clinic);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Clinic deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting clinic: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ClinicExists(int id)
        {
            return _context.Clinics.Any(e => e.Id == id);
        }
    }
}