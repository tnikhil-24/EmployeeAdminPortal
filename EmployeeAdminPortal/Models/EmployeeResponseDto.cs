namespace EmployeeAdminPortal.Models
{
    public record EmployeeResponseDto(Guid Id,
        string Name,
        string Email,
        string? Phone,
        decimal Salary
    );
}
