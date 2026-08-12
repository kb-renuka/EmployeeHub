using EmployeeHub.Api.Data;
using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public DepartmentsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetAll()
    {
        var departments = await _db.Departments
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                EmployeeCount = d.Employees.Count
            })
            .ToListAsync();

        return Ok(departments);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DepartmentDto>> GetById(int id)
    {
        var department = await _db.Departments
            .Where(d => d.Id == id)
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                Description = d.Description,
                EmployeeCount = d.Employees.Count
            })
            .FirstOrDefaultAsync();

        if (department == null) return NotFound();
        return Ok(department);
    }

    [HttpGet("{id:int}/employees")]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployeesInDepartment(int id)
    {
        var exists = await _db.Departments.AnyAsync(d => d.Id == id);
        if (!exists) return NotFound(new { message = "Department not found." });

        var employees = await _db.Employees
            .Where(e => e.DepartmentId == id)
            .Include(e => e.Department)
            .Select(e => MapToDto(e))
            .ToListAsync();

        return Ok(employees);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DepartmentDto>> Create(DepartmentCreateDto dto)
    {
        var duplicate = await _db.Departments.AnyAsync(d => d.Name == dto.Name);
        if (duplicate) return Conflict(new { message = "A department with this name already exists." });

        var department = new Department { Name = dto.Name, Description = dto.Description };
        _db.Departments.Add(department);
        await _db.SaveChangesAsync();

        var result = new DepartmentDto { Id = department.Id, Name = department.Name, Description = department.Description, EmployeeCount = 0 };
        return CreatedAtAction(nameof(GetById), new { id = department.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, DepartmentCreateDto dto)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department == null) return NotFound();

        department.Name = dto.Name;
        department.Description = dto.Description;
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department == null) return NotFound();

        var hasEmployees = await _db.Employees.AnyAsync(e => e.DepartmentId == id);
        if (hasEmployees)
            return BadRequest(new { message = "Cannot delete a department that still has employees assigned." });

        _db.Departments.Remove(department);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static EmployeeDto MapToDto(Employee e) => new()
    {
        Id = e.Id,
        FirstName = e.FirstName,
        LastName = e.LastName,
        Email = e.Email,
        Phone = e.Phone,
        JobTitle = e.JobTitle,
        Salary = e.Salary,
        HireDate = e.HireDate,
        DepartmentId = e.DepartmentId,
        DepartmentName = e.Department?.Name
    };
}
