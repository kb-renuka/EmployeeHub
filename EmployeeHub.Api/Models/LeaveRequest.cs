using System.ComponentModel.DataAnnotations;

namespace EmployeeHub.Api.Models;

public enum LeaveStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public class LeaveRequest
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [MaxLength(500)]
    public string? Reason { get; set; }

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

    // Who actioned it (approved/rejected) — nullable until decided.
    public string? ActionedByUserId { get; set; }
    public DateTime? ActionedAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
