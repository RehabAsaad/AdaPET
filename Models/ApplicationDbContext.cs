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
			optionsBuilder.UseSqlServer("Data Source=DESKTOP-BFVG5BL;Initial Catalog=AdaPETDb;Integrated Security=True;Trust Server Certificate=True");
			base.OnConfiguring(optionsBuilder);
		}


	}
}
