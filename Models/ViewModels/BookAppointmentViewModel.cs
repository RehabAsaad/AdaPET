namespace AdaPET.Models.ViewModels
{
    public class BookAppointmentViewModel
    {
        public int TimeSlotId { get; set; }
        public int ScheduleId { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string Notes { get; set; }
    }
}