using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "name required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "the name should be between 3 and 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "email required")]
        [EmailAddress(ErrorMessage = "invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "phone required")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "invalid Egyptian phone number")]
        public string phone { get; set; }

        [Required(ErrorMessage = "password required")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "password must be at least 6 characters")]
        public string Password { get; set; }

        public string UserRole { get; set; } // "Patient" or "Doctor"

        [NotMapped]
        [Compare("Password", ErrorMessage = "passwords do not match")]
        public string ConfirmPass { get; set; }

        public List<Animal>? OwnedAnimals { get; set; }
    }
}