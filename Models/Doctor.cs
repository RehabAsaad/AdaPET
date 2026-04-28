using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
	public class Doctor
	{
		[Key,ForeignKey("User")]
		public int Id { get; set; }
		public User User { get; set; }
		public string Specialization { get; set; }
		public ICollection<Clinic> Clinics { get; set; } 

	}
}
