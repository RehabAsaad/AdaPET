using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }  // ✅ ده هو UserId من جدول Doctor

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        // ✅ خاصية الحالة (Available, Booked, Cancelled)
        private string _status = "Available";
        public string Status
        {
            get => _status;
            set => _status = value;
        }

        // ✅ خاصية IsAvailable (للتكامل مع الكود القديم)
        [NotMapped]
        public bool IsAvailable
        {
            get => Status == "Available";
            set => Status = value ? "Available" : "Booked";
        }

        // ✅ العلاقة مع جدول Doctor
        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }
        public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();

    }
}