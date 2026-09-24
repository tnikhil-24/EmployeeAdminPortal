using EmployeeAdminPortal.Exceptions;
using EmployeeAdminPortal.Models;
using EmployeeAdminPortal.Models.Entities;
using EmployeeAdminPortal.Repositories;
using EmployeeAdminPortal.Services;
using Moq;

namespace EmployeeAdminPortal.Tests
{
    public class EmployeeServiceTests
    {
        private readonly Mock<IEmployeeRepository> repositoryMock = new();
        private readonly EmployeeService service;

        public EmployeeServiceTests()
        {
            service = new EmployeeService(repositoryMock.Object);
        }

        private static Employee CreateEmployee(string email = "jane@turner.com") => new()
        {
            Id = Guid.NewGuid(),
            Name = "Jane Doe",
            Email = email,
            Phone = "555-0100",
            Salary = 85000m
        };

        // ---------- GetAll ----------

        [Fact]
        public async Task GetAllAsync_ReturnsAllEmployeesMappedToDtos()
        {
            // Arrange
            var employees = new List<Employee> { CreateEmployee("a@turner.com"), CreateEmployee("b@turner.com") };
            repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(employees);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(employees[0].Id, result[0].Id);
            Assert.Equal("b@turner.com", result[1].Email);
        }

        // ---------- GetById ----------

        [Fact]
        public async Task GetByIdAsync_EmployeeExists_ReturnsDto()
        {
            var employee = CreateEmployee();
            repositoryMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);

            var result = await service.GetByIdAsync(employee.Id);

            Assert.NotNull(result);
            Assert.Equal(employee.Name, result!.Name);
            Assert.Equal(employee.Email, result.Email);
        }

        [Fact]
        public async Task GetByIdAsync_EmployeeMissing_ReturnsNull()
        {
            repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Employee?)null);

            var result = await service.GetByIdAsync(Guid.NewGuid());

            Assert.Null(result);
        }

        // ---------- Create ----------

        [Fact]
        public async Task CreateAsync_EmailIsUnique_AddsAndSavesEmployee()
        {
            var dto = new AddEmployeeDto { Name = "Jane Doe", Email = "jane@turner.com", Phone = "555-0100", Salary = 85000m };
            repositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, It.IsAny<Guid?>())).ReturnsAsync(false);

            Employee? added = null;
            repositoryMock.Setup(r => r.Add(It.IsAny<Employee>())).Callback<Employee>(e => added = e);

            var result = await service.CreateAsync(dto);

            Assert.NotNull(added);
            Assert.Equal(dto.Email, added!.Email);
            Assert.Equal(dto.Name, result.Name);
            repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_EmailAlreadyExists_ThrowsAndDoesNotSave()
        {
            var dto = new AddEmployeeDto { Name = "Jane Doe", Email = "jane@turner.com", Phone = null, Salary = 85000m };
            repositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, It.IsAny<Guid?>())).ReturnsAsync(true);

            await Assert.ThrowsAsync<DuplicateEmailException>(() => service.CreateAsync(dto));

            repositoryMock.Verify(r => r.Add(It.IsAny<Employee>()), Times.Never);
            repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        // ---------- Update ----------

        [Fact]
        public async Task UpdateAsync_EmployeeMissing_ReturnsNullAndDoesNotSave()
        {
            var dto = new UpdateEmployeeDto { Name = "New Name", Email = "new@turner.com", Phone = null, Salary = 90000m };
            repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Employee?)null);

            var result = await service.UpdateAsync(Guid.NewGuid(), dto);

            Assert.Null(result);
            repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_EmailUsedByAnotherEmployee_Throws()
        {
            var employee = CreateEmployee();
            var dto = new UpdateEmployeeDto { Name = "Jane Doe", Email = "taken@turner.com", Phone = null, Salary = 85000m };
            repositoryMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);
            repositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, employee.Id)).ReturnsAsync(true);

            await Assert.ThrowsAsync<DuplicateEmailException>(() => service.UpdateAsync(employee.Id, dto));

            repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ValidRequest_UpdatesFieldsAndSaves()
        {
            var employee = CreateEmployee();
            var dto = new UpdateEmployeeDto { Name = "Jane Smith", Email = "jane.smith@turner.com", Phone = "555-0199", Salary = 95000m };
            repositoryMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);
            repositoryMock.Setup(r => r.EmailExistsAsync(dto.Email, employee.Id)).ReturnsAsync(false);

            var result = await service.UpdateAsync(employee.Id, dto);

            Assert.NotNull(result);
            Assert.Equal("Jane Smith", employee.Name);
            Assert.Equal(95000m, employee.Salary);
            Assert.Equal(dto.Email, result!.Email);
            repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        // ---------- Delete ----------

        [Fact]
        public async Task DeleteAsync_EmployeeMissing_ReturnsFalseAndDoesNotRemove()
        {
            repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Employee?)null);

            var result = await service.DeleteAsync(Guid.NewGuid());

            Assert.False(result);
            repositoryMock.Verify(r => r.Remove(It.IsAny<Employee>()), Times.Never);
            repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_EmployeeExists_RemovesSavesAndReturnsTrue()
        {
            var employee = CreateEmployee();
            repositoryMock.Setup(r => r.GetByIdAsync(employee.Id)).ReturnsAsync(employee);

            var result = await service.DeleteAsync(employee.Id);

            Assert.True(result);
            repositoryMock.Verify(r => r.Remove(employee), Times.Once);
            repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}