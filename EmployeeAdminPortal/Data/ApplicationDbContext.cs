using EmployeeAdminPortal.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>().HasKey(e => e.Id);
            modelBuilder.Entity<Employee>().Property(e => e.Name).IsRequired().HasMaxLength(100);
            modelBuilder.Entity<Employee>().Property(e => e.Email).IsRequired().HasMaxLength(256);
            modelBuilder.Entity<Employee>().Property(e => e.Phone).HasMaxLength(25);
            modelBuilder.Entity<Employee>().Property(e => e.Salary).HasPrecision(18, 2);
            modelBuilder.Entity<Employee>().HasIndex(e => e.Email).IsUnique();
        }
    }
}