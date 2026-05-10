namespace AdaPET.Models.ViewModels
{
    public class DoctorScheduleViewModel
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorPhone { get; set; } = string.Empty;
        public string DoctorEmail { get; set; } = string.Empty;  // ✅ إضافة الإيميل
        public string Specialization { get; set; } = string.Empty;
        public List<Schedule> Schedules { get; set; } = new();
        public bool IsDoctor { get; set; }
    }
}