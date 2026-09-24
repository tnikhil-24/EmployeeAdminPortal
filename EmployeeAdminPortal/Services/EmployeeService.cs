using EmployeeAdminPortal.Exceptions;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Repositories;

namespace EmployeeAdminPortal.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository repository;

        public EmployeeService(IEmployeeRepository repository)
        {
            this.repository = repository;
        }

        public async Task<IReadOnlyList<EmployeeResponseDto>> GetAllAsync()
        {
            var employees = await repository.GetAllAsync();
            return employees.Select(ToDto).ToList();
        }

        public async Task<EmployeeResponseDto?> GetByIdAsync(Guid id)
        {
            var employee = await repository.GetByIdAsync(id);
            return employee is null ? null : ToDto(employee);
        }

        public async Task<EmployeeResponseDto> CreateAsync(AddEmployeeDto dto)
        {
            if (await repository.EmailExistsAsync(dto.Email))
            {
                throw new DuplicateEmailException(dto.Email);
            }

            var employee = new Employee
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Salary = dto.Salary
            };

            repository.Add(employee);
            await repository.SaveChangesAsync();

            return ToDto(employee);
        }

        public async Task<EmployeeResponseDto?> UpdateAsync(Guid id, UpdateEmployeeDto dto)
        {
            var employee = await repository.GetByIdAsync(id);

            if (employee is null)
            {
                return null;
            }

            if (await repository.EmailExistsAsync(dto.Email, excludeId: id))
            {
                throw new DuplicateEmailException(dto.Email);
            }

            employee.Name = dto.Name;
            employee.Email = dto.Email;
            employee.Phone = dto.Phone;
            employee.Salary = dto.Salary;

            await repository.SaveChangesAsync();

            return ToDto(employee);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var employee = await repository.GetByIdAsync(id);

            if (employee is null)
            {
                return false;
            }

            repository.Remove(employee);
            await repository.SaveChangesAsync();

            return true;
        }

        private static EmployeeResponseDto ToDto(Employee e) =>
            new(e.Id, e.Name, e.Email, e.Phone, e.Salary);
    }
}