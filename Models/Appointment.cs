using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int DoctorId { get; set; }
        public int ScheduleId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int? TimeSlotId { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // علاقات
        public User User { get; set; }
        public Doctor Doctor { get; set; }
        public Schedule Schedule { get; set; }
        [ForeignKey("TimeSlotId")]
        public virtual TimeSlot? TimeSlot { get; set; }
    }
}