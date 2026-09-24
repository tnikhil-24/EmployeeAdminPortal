using EmployeeAdminPortal.Data;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAdminPortal.Controllers
{
    //localhost:xxxx/api/employees
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public EmployeesController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetAllEmployees()
        {
            var employees = await dbContext.Employees
                .AsNoTracking()
                .OrderBy(e => e.Name)
                .Select(e => new EmployeeResponseDto(e.Id, e.Name, e.Email, e.Phone, e.Salary))
                .ToListAsync();

            return Ok(employees);

        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmployeeResponseDto>> GetEmployeeById(Guid id)
        {
            var employee = await dbContext.Employees.FindAsync(id);

            if (employee is null)
            {
                return NotFound();
            }

            return Ok(ToDto(employee));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EmployeeResponseDto>> AddEmployee(AddEmployeeDto addEmployeeDto)
        {
            if (await dbContext.Employees.AnyAsync(e => e.Email == addEmployeeDto.Email))
            {
                return Problem(
                    title: "Email already in use",
                    detail: $"An employee with email '{addEmployeeDto.Email}' already exists.",
                    statusCode: StatusCodes.Status409Conflict);
            }
            var employee = new Employee()
            {
                Name = addEmployeeDto.Name,
                Email = addEmployeeDto.Email,
                Phone = addEmployeeDto.Phone,
                Salary = addEmployeeDto.Salary
            };

            dbContext.Employees.Add(employee);
            await dbContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.Id }, ToDto(employee));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EmployeeResponseDto>> UpdateEmployee(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            var employee = await dbContext.Employees.FindAsync(id);

            if(employee is null)
            {
                return NotFound();
            }

            if (await dbContext.Employees.AnyAsync(e => e.Email == updateEmployeeDto.Email && e.Id != id))
            {
                return Problem(
                    title: "Email already in use",
                    detail: $"An employee with email '{updateEmployeeDto.Email}' already exists.",
                    statusCode: StatusCodes.Status409Conflict);
            }

            employee.Name = updateEmployeeDto.Name;
            employee.Email = updateEmployeeDto.Email;
            employee.Phone = updateEmployeeDto.Phone;
            employee.Salary = updateEmployeeDto.Salary;

            await dbContext.SaveChangesAsync();

            return Ok(ToDto(employee));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteEmployee(Guid id) 
        {

            var employee = await dbContext.Employees.FindAsync(id);

            if (employee is null)
            {
                return NotFound();
            }

            dbContext.Employees.Remove(employee);
            await dbContext.SaveChangesAsync();

            return NoContent();
        }
        private static EmployeeResponseDto ToDto(Employee e) =>
            new(e.Id, e.Name, e.Email, e.Phone, e.Salary);
    }
}
