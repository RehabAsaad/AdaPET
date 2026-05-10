using AdaPET.Models;
using AdaPET.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AdaPET.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض صفحة الحجز لفتحة زمنية معينة
        [HttpGet]
        public async Task<IActionResult> Book(int timeSlotId)
        {
            Console.WriteLine($"===== GET DEBUG: timeSlotId received = {timeSlotId} =====");

            var timeSlot = await _context.TimeSlots
                .Include(t => t.Schedule)
                .ThenInclude(s => s.Doctor)
                .FirstOrDefaultAsync(t => t.Id == timeSlotId);

            if (timeSlot == null)
            {
                Console.WriteLine($"===== GET DEBUG: timeSlot is NULL for ID {timeSlotId} =====");
            }
            else
            {
                Console.WriteLine($"===== GET DEBUG: timeSlot found - IsBooked={timeSlot.IsBooked}, ScheduleId={timeSlot.ScheduleId} =====");
            }

            if (timeSlot == null || timeSlot.IsBooked)
            {
                TempData["Error"] = "هذا الموعد غير متاح للحجز.";
                return RedirectToAction("Index", "Doctor");
            }

            var model = new BookAppointmentViewModel
            {
                TimeSlotId = timeSlot.Id,
                ScheduleId = timeSlot.ScheduleId,
                DoctorId = timeSlot.Schedule.DoctorId,
                DoctorName = timeSlot.Schedule.Doctor?.Name ?? "طبيب",
                AppointmentDate = timeSlot.Date,
                StartTime = timeSlot.StartTime,
                EndTime = timeSlot.EndTime
            };

            return View("~/Views/Appointment/Book.cshtml", model);
        }

        // تأكيد الحجز
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookAppointmentViewModel model)
        {
            Console.WriteLine($"===== POST DEBUG 1: TimeSlotId received = {model.TimeSlotId} =====");
            Console.WriteLine($"===== POST DEBUG 2: DoctorId = {model.DoctorId} =====");
            Console.WriteLine($"===== POST DEBUG 3: AppointmentDate = {model.AppointmentDate} =====");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Console.WriteLine($"===== POST DEBUG 4: UserId = {userId} =====");

            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine($"===== POST DEBUG 5: Unauthorized - no userId =====");
                return Unauthorized();
            }

            var timeSlot = await _context.TimeSlots
                .Include(t => t.Schedule)
                .FirstOrDefaultAsync(t => t.Id == model.TimeSlotId);

            Console.WriteLine($"===== POST DEBUG 6: timeSlot found = {timeSlot != null} =====");

            if (timeSlot != null)
            {
                Console.WriteLine($"===== POST DEBUG 7: timeSlot.IsBooked = {timeSlot.IsBooked} =====");
                Console.WriteLine($"===== POST DEBUG 8: timeSlot.ScheduleId = {timeSlot.ScheduleId} =====");
            }

            if (timeSlot == null || timeSlot.IsBooked)
            {
                Console.WriteLine($"===== POST DEBUG 9: FAILED - timeSlot null or booked =====");
                TempData["Error"] = "الموعد غير متاح.";
                return RedirectToAction("Index", "Doctor");
            }

            // حجز الفتحة الزمنية
            timeSlot.IsBooked = true;
            timeSlot.UserId = int.Parse(userId);

            // إنشاء الحجز
            var appointment = new Appointment
            {
                UserId = int.Parse(userId),
                DoctorId = model.DoctorId,
                ScheduleId = timeSlot.ScheduleId,
                TimeSlotId = timeSlot.Id,
                AppointmentDate = timeSlot.Date,
                StartTime = timeSlot.StartTime,
                EndTime = timeSlot.EndTime,
                Status = "Confirmed",
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            Console.WriteLine($"===== POST DEBUG 10: SUCCESS - Appointment created =====");
            TempData["SuccessMessage"] = "تم حجز الموعد بنجاح!";
            return RedirectToAction("MyAppointments");
        }

        // عرض حجوزات المستخدم الحالي
        public async Task<IActionResult> MyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .ThenInclude(d => d.User)
                .Include(a => a.TimeSlot)
                .Where(a => a.UserId == int.Parse(userId))
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View("~/Views/Appointment/MyAppointments.cshtml", appointments);
        }

        // إلغاء الحجز
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.TimeSlot)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (appointment.UserId.ToString() != userId)
            {
                return Unauthorized();
            }

            appointment.Status = "Cancelled";

            if (appointment.TimeSlot != null)
            {
                appointment.TimeSlot.IsBooked = false;
                appointment.TimeSlot.UserId = null;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "تم إلغاء الحجز بنجاح.";
            return RedirectToAction("MyAppointments");
        }
    }
}