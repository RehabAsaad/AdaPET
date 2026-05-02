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
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer("Server=DESKTOP-BFVG5BL;Database=AdaPETDb;Trusted_Connection=True;TrustServerCertificate=True;");
			base.OnConfiguring(optionsBuilder);
		}


	}
}
