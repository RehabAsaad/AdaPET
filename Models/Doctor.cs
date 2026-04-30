using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AdaPET.Models
{
    public class Doctor
    {
        [Key] // ده معناه إن الحقل ده هو الـ Primary Key
        [ForeignKey("User")] // وده معناه إنه في نفس الوقت Foreign Key لجدول اليوزر
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // مهم جداً: بنقول للداتابيز متزوديش الرقم ده من عندك
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Specialization { get; set; }
        // ربط الدكتور باليوزر (علاقة One-to-One)

        public User User { get; set; }
        public ICollection<Clinic> Clinics { get; set; }

    }
}
