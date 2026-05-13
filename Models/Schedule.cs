using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [Required]
        public int ClinicId { get; set; }  // ✅ أضيفي السطر ده

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        private string _status = "Available";
        public string Status
        {
            get => _status;
            set => _status = value;
        }

        [NotMapped]
        public bool IsAvailable
        {
            get => Status == "Available";
            set => Status = value ? "Available" : "Booked";
        }

        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; }

        [ForeignKey("ClinicId")]
        public virtual Clinic? Clinic { get; set; }  // ✅ أضيفي السطر ده

        public virtual ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
    }
}