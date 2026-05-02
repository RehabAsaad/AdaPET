using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
	public class Clinic
	{
		[Key]
		public int Id { get; set; }
		public string Name { get; set; }
		public string? Description { get; set; }
		public string location { get; set; }
		public string Phone { get; set; }
      
        public int DoctorId { get; set; }
		[ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; }
       // public ICollection<Doctor> Doctors { get; set; }
	}
}
