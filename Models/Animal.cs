using System.ComponentModel.DataAnnotations; 
using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
    public class Animal
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Please enter the animal's name")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Name must contain letters only.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please provide a description")]
        [MinLength(10, ErrorMessage = "Description should be at least 10 characters long")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Please enter the animal's age")]
        [Range(0, 30, ErrorMessage = "Age must be between 0 and 30 years")]
        public int Age { get; set; }

        public bool IsAdopted { get; set; }

        [Required(ErrorMessage = "Please specify the animal type (e.g., Dog, Cat)")]
        [RegularExpression(@"^(Dog|Cat|Bird|Rabbit|Hamster|Turtle|Parrot|Fish|Horse|Monkey)$",
        ErrorMessage = "Invalid animal type selected.")]
        public string Type { get; set; }

        
        public string? ImgURL { get; set; }

        [ForeignKey("Owner")]
        public int OwnerId { get; set; }
        public User? Owner { get; set; }


        [NotMapped]
        [Required(ErrorMessage = "Please upload an image for the pet")]
        [Display(Name = "Upload Image")]
        public IFormFile ImageFile { get; set; }

        public DateTime? AdoptedDate { get; set; }
    }
}