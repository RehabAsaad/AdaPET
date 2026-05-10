using AdaPET.Models;
using AdaPET.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdaPET.Controllers
{
    [Authorize]
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض مواعيد دكتور معين
        [HttpGet]
        public async Task<IActionResult> Index(int doctorId)
        {
            if (doctorId <= 0)
            {
                TempData["Error"] = "Invalid doctor ID";
                return RedirectToAction("Index", "Doctor");
            }

            // ✅ جيب الدكتور من جدول Users مع بياناته بالكامل
            var doctor = await _context.Users
                .Include(u => u.Doctor)  // ✅ Load الـ Doctor المرتبط
                .FirstOrDefaultAsync(u => u.Id == doctorId && u.UserRole == "Doctor");

            if (doctor == null)
            {
                TempData["Error"] = "Doctor not found";
                return RedirectToAction("Index", "Doctor");
            }

            // ✅ جيب المواعيد المرتبطة بالدكتور (باستخدام DoctorUserId)
            var schedules = await _context.Schedules
                .Where(s => s.DoctorUserId == doctorId)  // ✅ DoctorUserId مش DoctorId
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            // ✅ جيب رقم العيادة (لو موجود)
            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.DoctorId == doctorId);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isDoctor = User.IsInRole("Doctor") && currentUserId == doctorId.ToString();

            // ✅ التخصص يجي من جدول Doctor
            var specialization = doctor.Doctor?.Specialization ?? "General";

            var viewModel = new DoctorScheduleViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.Name ?? "Unknown",
                DoctorPhone = clinic?.Phone ?? doctor.phone ?? "Not available",
                DoctorEmail = doctor.Email ?? "Not available",
                Specialization = specialization,
                Schedules = schedules ?? new List<Schedule>(),
                IsDoctor = isDoctor
            };

            return View("~/Views/Doctor/Schedule.cshtml", viewModel);
        }

        // إضافة موعد (للدكتور فقط)
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AddSchedule(int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (currentUserId != doctorId)
            {
                TempData["Error"] = "You can only add schedules for yourself";
                return RedirectToAction("Index", new { doctorId });
            }

            var schedule = new Schedule
            {
                DoctorUserId = doctorId,  // ✅ DoctorUserId مش DoctorId
                Date = date,
                StartTime = startTime,
                EndTime = endTime,
                IsAvailable = true
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Schedule added successfully";
            return RedirectToAction("Index", new { doctorId });
        }

        // حذف موعد
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> DeleteSchedule(int scheduleId, int doctorId)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (currentUserId != doctorId)
            {
                TempData["Error"] = "You can only delete your own schedules";
                return RedirectToAction("Index", new { doctorId });
            }

            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Schedule deleted successfully";
            }

            return RedirectToAction("Index", new { doctorId });
        }

        // تعديل موعد
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> EditSchedule(int scheduleId, int doctorId, DateTime date, TimeSpan startTime, TimeSpan endTime, bool isAvailable)
        {
            var currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (currentUserId != doctorId)
            {
                TempData["Error"] = "You can only edit your own schedules";
                return RedirectToAction("Index", new { doctorId });
            }

            var schedule = await _context.Schedules.FindAsync(scheduleId);
            if (schedule != null)
            {
                schedule.Date = date;
                schedule.StartTime = startTime;
                schedule.EndTime = endTime;
                schedule.IsAvailable = isAvailable;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Schedule updated successfully";
            }

            return RedirectToAction("Index", new { doctorId });
        }
    }
}