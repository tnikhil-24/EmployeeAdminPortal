using EmployeeAdminPortal.Models.Entities;

namespace EmployeeAdminPortal.Repositories
{
    public interface IEmployeeRepository
    {
        Task<IReadOnlyList<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(Guid id);
        Task<bool> EmailExistsAsync(string email, Guid? excludeId = null);
        void Add(Employee employee);
        void Remove(Employee employee);
        Task SaveChangesAsync();
    }
}
