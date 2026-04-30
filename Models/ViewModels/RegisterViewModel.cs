using System.ComponentModel.DataAnnotations;

namespace AdaPET.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required ")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string UserRole { get; set; } // "Doctor" or "Patient"

        // خصائص إضافية للدكتور
        public string? Specialization { get; set; }
        public List<ClinicViewModel>? Clinics { get; set; }
    }

    public class ClinicViewModel
    {
        public string? Name { get; set; }
        public string? Address { get; set; }
    }
}

