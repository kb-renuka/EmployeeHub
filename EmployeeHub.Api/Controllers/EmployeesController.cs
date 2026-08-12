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
public class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public EmployeesController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int? departmentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _db.Employees.Include(e => e.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term) ||
                (e.JobTitle != null && e.JobTitle.ToLower().Contains(term)));
        }

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var employees = await query
            .OrderBy(e => e.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => MapToDto(e))
            .ToListAsync();

        return Ok(employees);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var employee = await _db.Employees.Include(e => e.Department).FirstOrDefaultAsync(e => e.Id == id);
        if (employee == null) return NotFound();
        return Ok(MapToDto(employee));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmployeeDto>> Create(EmployeeCreateDto dto)
    {
        var departmentExists = await _db.Departments.AnyAsync(d => d.Id == dto.DepartmentId);
        if (!departmentExists) return BadRequest(new { message = "DepartmentId does not refer to an existing department." });

        var duplicateEmail = await _db.Employees.AnyAsync(e => e.Email == dto.Email);
        if (duplicateEmail) return Conflict(new { message = "An employee with this email already exists." });

        var employee = new Employee
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            JobTitle = dto.JobTitle,
            Salary = dto.Salary,
            DepartmentId = dto.DepartmentId,
            HireDate = DateTime.UtcNow
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();
        await _db.Entry(employee).Reference(e => e.Department).LoadAsync();

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, MapToDto(employee));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, EmployeeUpdateDto dto)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        var departmentExists = await _db.Departments.AnyAsync(d => d.Id == dto.DepartmentId);
        if (!departmentExists) return BadRequest(new { message = "DepartmentId does not refer to an existing department." });

        employee.FirstName = dto.FirstName;
        employee.LastName = dto.LastName;
        employee.Email = dto.Email;
        employee.Phone = dto.Phone;
        employee.JobTitle = dto.JobTitle;
        employee.Salary = dto.Salary;
        employee.DepartmentId = dto.DepartmentId;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee == null) return NotFound();

        _db.Employees.Remove(employee);
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
