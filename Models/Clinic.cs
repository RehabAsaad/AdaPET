using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
    public class Clinic
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Clinic phone is required")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "Invalid Egyptian phone number")]
        public string Phone { get; set; } = string.Empty;

        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; } = null!;

        // ✅ ONLY ADD THIS ONE LINE
        public string? Schedule { get; set; }
    }
}