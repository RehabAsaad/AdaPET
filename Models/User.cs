using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
	public class User
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string phone {  get; set; }
		public string Password { get; set; }

		[NotMapped]
		public string ConfirmPass { get; set; }
		public List<Animal>? OwnedAnimals { get; set; }


	}
}
