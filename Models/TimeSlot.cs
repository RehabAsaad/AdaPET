using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
    public class TimeSlot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ScheduleId { get; set; }  // الفترة الأصلية

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsBooked { get; set; } = false;

        public int? UserId { get; set; }  // اليوزر اللي حجزها

        // علاقات
        [ForeignKey("ScheduleId")]
        public virtual Schedule Schedule { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        // ✅ أضيفي السطر ده - العلاقة مع Appointment
        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}