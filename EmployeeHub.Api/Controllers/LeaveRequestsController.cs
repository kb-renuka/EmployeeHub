using System.Security.Claims;
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
public class LeaveRequestsController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public LeaveRequestsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet("employee/{employeeId:int}")]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetHistoryForEmployee(int employeeId)
    {
        var exists = await _db.Employees.AnyAsync(e => e.Id == employeeId);
        if (!exists) return NotFound(new { message = "Employee not found." });

        var history = await _db.LeaveRequests
            .Where(l => l.EmployeeId == employeeId)
            .Include(l => l.Employee)
            .OrderByDescending(l => l.CreatedAtUtc)
            .Select(l => MapToDto(l))
            .ToListAsync();

        return Ok(history);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<LeaveRequestDto>>> GetPending()
    {
        var pending = await _db.LeaveRequests
            .Where(l => l.Status == LeaveStatus.Pending)
            .Include(l => l.Employee)
            .OrderBy(l => l.CreatedAtUtc)
            .Select(l => MapToDto(l))
            .ToListAsync();

        return Ok(pending);
    }

    [HttpPost("employee/{employeeId:int}")]
    public async Task<ActionResult<LeaveRequestDto>> RequestLeave(int employeeId, LeaveRequestCreateDto dto)
    {
        var employee = await _db.Employees.FindAsync(employeeId);
        if (employee == null) return NotFound(new { message = "Employee not found." });

        if (dto.EndDate < dto.StartDate)
            return BadRequest(new { message = "EndDate cannot be before StartDate." });

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employeeId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Reason = dto.Reason,
            Status = LeaveStatus.Pending
        };

        _db.LeaveRequests.Add(leaveRequest);
        await _db.SaveChangesAsync();
        await _db.Entry(leaveRequest).Reference(l => l.Employee).LoadAsync();

        return CreatedAtAction(nameof(GetHistoryForEmployee), new { employeeId }, MapToDto(leaveRequest));
    }

    [HttpPut("{id:int}/action")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LeaveRequestDto>> ActionRequest(int id, LeaveRequestActionDto dto)
    {
        if (dto.Status != LeaveStatus.Approved && dto.Status != LeaveStatus.Rejected)
            return BadRequest(new { message = "Status must be Approved or Rejected." });

        var leaveRequest = await _db.LeaveRequests.Include(l => l.Employee).FirstOrDefaultAsync(l => l.Id == id);
        if (leaveRequest == null) return NotFound();

        if (leaveRequest.Status != LeaveStatus.Pending)
            return BadRequest(new { message = "This request has already been actioned." });

        leaveRequest.Status = dto.Status;
        leaveRequest.ActionedAtUtc = DateTime.UtcNow;
        leaveRequest.ActionedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        await _db.SaveChangesAsync();

        return Ok(MapToDto(leaveRequest));
    }

    private static LeaveRequestDto MapToDto(LeaveRequest l) => new()
    {
        Id = l.Id,
        EmployeeId = l.EmployeeId,
        EmployeeName = l.Employee != null ? $"{l.Employee.FirstName} {l.Employee.LastName}" : null,
        StartDate = l.StartDate,
        EndDate = l.EndDate,
        Reason = l.Reason,
        Status = l.Status,
        CreatedAtUtc = l.CreatedAtUtc,
        ActionedAtUtc = l.ActionedAtUtc
    };
}
