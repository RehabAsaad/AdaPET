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
                        Schedule = c.Schedule,  // ✅ ADD THIS LINE
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

            ModelState.Remove("Doctor");

            if (ModelState.IsValid)
            {
                try
                {
                    existingClinic.Name = clinic.Name;
                    existingClinic.Description = clinic.Description ?? string.Empty;
                    existingClinic.location = clinic.location;
                    existingClinic.Phone = clinic.Phone;
                    existingClinic.Schedule = clinic.Schedule;  // ✅ ADD THIS LINE

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

            return View(clinic);
        }

        private bool ClinicExists(int id)
        {
            return _context.Clinics.Any(e => e.Id == id);
        }
    }
}