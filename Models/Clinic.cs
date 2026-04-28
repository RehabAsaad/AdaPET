namespace AdaPET.Models
{
	public class Clinic
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }
		public string location { get; set; }
		public string Phone { get; set; }
		public ICollection<Doctor> Doctors { get; set; }
	}
}
