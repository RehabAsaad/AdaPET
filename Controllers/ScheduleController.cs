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

            // جيب الدكتور من جدول Users مع بياناته بالكامل
            var doctor = await _context.Users
                .Include(u => u.Doctor)
                .FirstOrDefaultAsync(u => u.Id == doctorId && u.UserRole == "Doctor");

            if (doctor == null)
            {
                TempData["Error"] = "Doctor not found";
                return RedirectToAction("Index", "Doctor");
            }

            // جيب المواعيد المرتبطة بالدكتور
            var schedules = await _context.Schedules
                .Where(s => s.DoctorId == doctorId)
                .Include(s => s.TimeSlots)
                .OrderBy(s => s.Date)
                .ThenBy(s => s.StartTime)
                .ToListAsync();

            // جيب العيادة
            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.DoctorId == doctorId);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isDoctor = User.IsInRole("Doctor") && currentUserId == doctorId.ToString();
            var specialization = doctor.Doctor?.Specialization ?? "General";

            // تحويل البيانات إلى ViewModel مع الفتحات
            var scheduleViewModels = schedules.Select(s => new ScheduleViewModel
            {
                Id = s.Id,
                Date = s.Date,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                IsAvailable = s.IsAvailable,
                TimeSlots = s.TimeSlots?.Select(ts => new TimeSlotViewModel
                {
                    Id = ts.Id,
                    StartTime = ts.StartTime,
                    EndTime = ts.EndTime,
                    IsBooked = ts.IsBooked,
                    UserId = ts.UserId
                }).ToList() ?? new List<TimeSlotViewModel>()
            }).ToList();

            var viewModel = new DoctorScheduleViewModel
            {
                DoctorId = doctor.Id,
                DoctorName = doctor.Name ?? "Unknown",
                DoctorPhone = clinic?.Phone ?? doctor.phone ?? "Not available",
                DoctorEmail = doctor.Email ?? "Not available",
                Specialization = specialization,
                Schedules = scheduleViewModels,
                IsDoctor = isDoctor
            };

            return View("~/Views/Doctor/Schedule.cshtml", viewModel);
        }

        // إضافة موعد (للدكتور فقط) - مع توليد الفتحات تلقائياً
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

            // التأكد من أن وقت البداية أقل من وقت النهاية
            if (startTime >= endTime)
            {
                TempData["Error"] = "Start time must be before end time";
                return RedirectToAction("Index", new { doctorId });
            }

            var schedule = new Schedule
            {
                DoctorId = doctorId,
                Date = date,
                StartTime = startTime,
                EndTime = endTime,
                IsAvailable = true,
                Status = "Available"
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            // توليد الفتحات الزمنية (نصف ساعة لكل فتحة)
            var slots = GenerateTimeSlots(schedule, 30);
            _context.TimeSlots.AddRange(slots);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Schedule added successfully with {slots.Count} time slots (30 min each)";
            return RedirectToAction("Index", new { doctorId });
        }

        // حذف موعد (يحذف الموعد والحجوزات والفتحات المرتبطة به)
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

            var schedule = await _context.Schedules
                .Include(s => s.TimeSlots)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule != null)
            {
                // ✅ 1. حذف الحجوزات المرتبطة بالـ Schedule أولاً
                var appointments = await _context.Appointments
                    .Where(a => a.ScheduleId == scheduleId)
                    .ToListAsync();

                if (appointments.Any())
                {
                    _context.Appointments.RemoveRange(appointments);
                }

                // ✅ 2. حذف الفتحات المرتبطة
                if (schedule.TimeSlots != null && schedule.TimeSlots.Any())
                {
                    _context.TimeSlots.RemoveRange(schedule.TimeSlots);
                }

                // ✅ 3. حذف الـ Schedule نفسه
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Schedule, all appointments, and time slots deleted successfully";
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

            var schedule = await _context.Schedules
                .Include(s => s.TimeSlots)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule != null)
            {
                // حفظ البيانات القديمة
                var oldStartTime = schedule.StartTime;
                var oldEndTime = schedule.EndTime;

                // تحديث البيانات
                schedule.Date = date;
                schedule.StartTime = startTime;
                schedule.EndTime = endTime;
                schedule.IsAvailable = isAvailable;
                schedule.Status = isAvailable ? "Available" : "Booked";

                // إذا تغير الوقت، أعد توليد الفتحات
                if (oldStartTime != startTime || oldEndTime != endTime)
                {
                    // ✅ حذف الحجوزات المرتبطة أولاً
                    var appointments = await _context.Appointments
                        .Where(a => a.ScheduleId == scheduleId)
                        .ToListAsync();

                    if (appointments.Any())
                    {
                        _context.Appointments.RemoveRange(appointments);
                    }

                    // حذف الفتحات القديمة
                    if (schedule.TimeSlots != null && schedule.TimeSlots.Any())
                    {
                        _context.TimeSlots.RemoveRange(schedule.TimeSlots);
                    }

                    // توليد فتحات جديدة
                    var newSlots = GenerateTimeSlots(schedule, 30);
                    _context.TimeSlots.AddRange(newSlots);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Schedule updated successfully";
            }

            return RedirectToAction("Index", new { doctorId });
        }

        // دالة توليد الفتحات الزمنية
        private List<TimeSlot> GenerateTimeSlots(Schedule schedule, int slotDurationMinutes = 30)
        {
            var slots = new List<TimeSlot>();
            var currentStart = schedule.StartTime;
            var endTime = schedule.EndTime;

            while (currentStart.Add(TimeSpan.FromMinutes(slotDurationMinutes)) <= endTime)
            {
                var slot = new TimeSlot
                {
                    ScheduleId = schedule.Id,
                    Date = schedule.Date,
                    StartTime = currentStart,
                    EndTime = currentStart.Add(TimeSpan.FromMinutes(slotDurationMinutes)),
                    IsBooked = false,
                    UserId = null
                };
                slots.Add(slot);
                currentStart = currentStart.Add(TimeSpan.FromMinutes(slotDurationMinutes));
            }

            return slots;
        }
    }
}