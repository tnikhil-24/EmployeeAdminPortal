using EmployeeAdminPortal.Models;

namespace EmployeeAdminPortal.Services
{
    public interface IEmployeeService
    {
        Task<IReadOnlyList<EmployeeResponseDto>> GetAllAsync();
        Task<EmployeeResponseDto?> GetByIdAsync(Guid id);
        Task<EmployeeResponseDto> CreateAsync(AddEmployeeDto dto);
        Task<EmployeeResponseDto?> UpdateAsync(Guid id, UpdateEmployeeDto dto);
        Task<bool> DeleteAsync(Guid id);
    }
}
