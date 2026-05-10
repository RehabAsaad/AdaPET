using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
    public class Doctor
    {
        [Key] 
        [ForeignKey("User")] //doctor primary key is a foreign key form user table
        [DatabaseGenerated(DatabaseGeneratedOption.None)] 
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Specialization { get; set; }
      //User , Doctor relation 1:1
        [ForeignKey("UserId")]
        public User User { get; set; }
        public ICollection<Clinic> Clinics { get; set; }
        public virtual ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    }
}
