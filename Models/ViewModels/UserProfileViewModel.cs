// In Models - UserProfileViewModel.cs
using System.Collections.Generic;

namespace AdaPET.Models
{
    public class UserProfileViewModel
    {
        // User Information
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string UserRole { get; set; }

        // Doctor info if user is a doctor
        public string? Specialization { get; set; }
        public string? ClinicsList { get; set; }  // ✅ Add this
        public string? PhotoURL { get; set; }     // ✅ Add this

        // Animals owned by this user
        public List<Animal> Animals { get; set; } = new List<Animal>();
    }
}