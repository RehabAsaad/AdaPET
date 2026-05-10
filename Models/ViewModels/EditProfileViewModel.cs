using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace AdaPET.Models
{
    public class EditProfileViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 50 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "Invalid Egyptian phone number")]
        public string Phone { get; set; }

        public string UserRole { get; set; }

        // Doctor-specific fields (nullable for regular users)
        public string? Specialization { get; set; }

        // For clinics - comma separated string for easy editing
        public string? ClinicsList { get; set; }

        // Profile photo
        public IFormFile? PhotoFile { get; set; }
        public string? ExistingPhotoURL { get; set; }
    }
}