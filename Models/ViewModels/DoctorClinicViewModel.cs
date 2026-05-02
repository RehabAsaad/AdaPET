namespace AdaPET.Models.ViewModels
{
    public class DoctorClinicViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public List<ClinicInfoViewModel> Clinics { get; set; } = new();
    }

    public class ClinicInfoViewModel
    {
        public int ClinicId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string Phone { get; set; }
        public bool CanEdit { get; set; }
    }

    public class DoctorsListViewModel
    {
        public List<DoctorClinicViewModel> Doctors { get; set; } = new();
        public int CurrentUserId { get; set; }
        public string CurrentUserRole { get; set; }
    }
}