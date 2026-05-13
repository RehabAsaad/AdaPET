using System;
using System.Collections.Generic;
using AdaPET.Models;

namespace AdaPET.Models.ViewModels
{
    public class DoctorScheduleViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public int SelectedClinicId { get; set; }  // ✅ أضيفي السطر ده

        public string DoctorPhone { get; set; } = string.Empty;
        public string DoctorEmail { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;

        // ✅ قائمة العيادات الخاصة بالدكتور (للقائمة المنسدلة)
        public List<Clinic> Clinics { get; set; } = new();

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

        // ✅ معلومات العيادة (للعرض في الجدول)
        public string ClinicName { get; set; } = string.Empty;
        public string ClinicLocation { get; set; } = string.Empty;
        public string ClinicPhone { get; set; } = string.Empty;

        public List<TimeSlotViewModel> TimeSlots { get; set; } = new();
    }

    public class TimeSlotViewModel
    {
        public int Id { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public bool IsBooked { get; set; }
        public int? UserId { get; set; }
    }
}