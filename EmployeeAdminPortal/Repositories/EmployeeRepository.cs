using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext dbContext;

        public EmployeeRepository(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IReadOnlyList<Employee>> GetAllAsync()
        {
            return await dbContext.Employees
                .AsNoTracking()
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(Guid id)
        {
            return await dbContext.Employees.FindAsync(id);
        }

        public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null)
        {
            return await dbContext.Employees
                .AnyAsync(e => e.Email == email && (excludeId == null || e.Id != excludeId));
        }

        public void Add(Employee employee) => dbContext.Employees.Add(employee);

        public void Remove(Employee employee) => dbContext.Employees.Remove(employee);

        public async Task SaveChangesAsync() => await dbContext.SaveChangesAsync();
    }
}