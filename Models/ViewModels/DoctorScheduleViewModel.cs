using System;
using System.Collections.Generic;

namespace AdaPET.Models.ViewModels
{
    public class DoctorScheduleViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorPhone { get; set; } = string.Empty;
        public string DoctorEmail { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;

        // ✅ غيري ده من List<Schedule> إلى List<ScheduleViewModel>
        public List<ScheduleViewModel> Schedules { get; set; } = new();

        public bool IsDoctor { get; set; }
    }

    public class ScheduleViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsAvailable { get; set; }
        public List<TimeSlotViewModel> TimeSlots { get; set; } = new();
    }

    public class TimeSlotViewModel
    {
        public int Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsBooked { get; set; }
        public int? UserId { get; set; }  // ✅ أضيفي ده لو محتاجة تعرفي مين حجز
    }
}