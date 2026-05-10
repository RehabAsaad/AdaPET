using Microsoft.EntityFrameworkCore;
using System;
namespace AdaPET.Models

{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext() { }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        
		public DbSet<User> Users { get; set; }
		public DbSet<Animal> Animals { get; set; }
		public DbSet<Doctor> Doctors { get; set; }
		public DbSet<Clinic> Clinics { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        // في دالة OnModelCreating:
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تعطيل Cascade Delete لكل العلاقات
            foreach (var foreignKey in modelBuilder.Model.GetEntityTypes()
                .SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
            }


        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer("Data Source=.\\SQLEXPRESS;Initial Catalog=AdaPETDb;Integrated Security=True;Trust Server Certificate=True");
			base.OnConfiguring(optionsBuilder);
		}


	}
}
