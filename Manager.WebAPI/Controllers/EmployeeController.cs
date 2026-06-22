using Microsoft.AspNetCore.Mvc;
using Manager.BusinessLogic.Interfaces;
using Manager.DataAccess.Entities;
using Manager.BusinessLogic.DTOs;

namespace Manager.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees([FromQuery] string? search)
    {
        /// Get all employees from service, handle null search string
        var employees = await _employeeService.GetAllEmployeesAsync(search ?? "");
        return Ok(employees);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmployeeCreateDto dto)
    {
        /// Map DTO to entity manually before saving
        var employee = new Employee
        {
            Name = dto.Name,
            LastName = dto.LastName,
            MiddleName = dto.MiddleName,
            Email = dto.Email
        };

        await _employeeService.CreateEmployeeAsync(employee);
        
        /// Return 201 status with the new entity including its generated ID
        return StatusCode(201, employee);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Employee employee)
    {
        /// Check if route ID matches body ID to prevent data inconsistency
        if (id != employee.Id)
        {
            return BadRequest("Employee ID mismatch");
        }

        await _employeeService.UpdateEmployeeAsync(employee);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        /// Try to delete and check if it actually existed
        var isDeleted = await _employeeService.DeleteEmployeeAsync(id);
        if (!isDeleted)
        {
            return NotFound("Employee not found");
        }
        return NoContent();
    }
}