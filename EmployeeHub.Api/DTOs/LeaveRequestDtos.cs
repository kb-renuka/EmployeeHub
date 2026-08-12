using System.ComponentModel.DataAnnotations;
using EmployeeHub.Api.Models;

namespace EmployeeHub.Api.DTOs;

public class LeaveRequestCreateDto
{
    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }
}

public class LeaveRequestActionDto
{
    [Required]
    public LeaveStatus Status { get; set; } // Approved or Rejected
}

public class LeaveRequestDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? Reason { get; set; }
    public LeaveStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ActionedAtUtc { get; set; }
}
