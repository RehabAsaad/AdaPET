using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DoctorUserId { get; set; }  // ✅ تتغير من DoctorId لـ DoctorUserId

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsAvailable { get; set; } = true;

        // ✅ العلاقة مع جدول User (الدكتور)
        [ForeignKey("DoctorUserId")]
        public virtual User? DoctorUser { get; set; }
    }
}