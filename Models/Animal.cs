using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
	public class Animal
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public int Age { get; set; }
		public bool IsAdopted { get; set; }

		public string ImgURL { get; set; }

		[ForeignKey("Owner")]
		public int OwnerId { get; set; }
		public User Owner { get; set; }

		public DateTime? AdoptedDate { get; set; }

	}
}
