using EmployeeAdminPortal.Exceptions;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAdminPortal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            this.employeeService = employeeService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetAllEmployees()
        {
            return Ok(await employeeService.GetAllAsync());
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmployeeResponseDto>> GetEmployeeById(Guid id)
        {
            var employee = await employeeService.GetByIdAsync(id);

            if (employee is null)
            {
                return NotFound();
            }

            return Ok(employee);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EmployeeResponseDto>> AddEmployee(AddEmployeeDto addEmployeeDto)
        {
            try
            {
                var created = await employeeService.CreateAsync(addEmployeeDto);
                return CreatedAtAction(nameof(GetEmployeeById), new { id = created.Id }, created);
            }
            catch (DuplicateEmailException ex)
            {
                return Problem(title: "Email already in use", detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict);
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EmployeeResponseDto>> UpdateEmployee(Guid id, UpdateEmployeeDto updateEmployeeDto)
        {
            try
            {
                var updated = await employeeService.UpdateAsync(id, updateEmployeeDto);

                if (updated is null)
                {
                    return NotFound();
                }

                return Ok(updated);
            }
            catch (DuplicateEmailException ex)
            {
                return Problem(title: "Email already in use", detail: ex.Message,
                    statusCode: StatusCodes.Status409Conflict);
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            if (!await employeeService.DeleteAsync(id))
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}