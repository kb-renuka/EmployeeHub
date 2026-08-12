using Microsoft.AspNetCore.Identity;

namespace EmployeeHub.Api.Models;

// Extends ASP.NET Core Identity's built-in user class.
// Identity already handles password hashing (PBKDF2) and role management
// via AspNetUsers / AspNetRoles / AspNetUserRoles tables — no need to
// hand-roll either.
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    // Optional link: a login account can be tied to one Employee record.
    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}
