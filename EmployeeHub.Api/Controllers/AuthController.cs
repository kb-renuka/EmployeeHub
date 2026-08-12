using EmployeeHub.Api.DTOs;
using EmployeeHub.Api.Models;
using EmployeeHub.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenService _tokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        var existing = await _userManager.FindByEmailAsync(dto.Email);
        if (existing != null)
            return Conflict(new { message = "A user with this email already exists." });

        var role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role;
        if (role != "Admin" && role != "User")
            return BadRequest(new { message = "Role must be either 'Admin' or 'User'." });

        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole(role));

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await _userManager.AddToRoleAsync(user, role);

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAtUtc) = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized(new { message = "Invalid email or password." });

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
            return Unauthorized(new { message = "Invalid email or password." });

        var roles = await _userManager.GetRolesAsync(user);
        var (token, expiresAtUtc) = _tokenService.CreateToken(user, roles);

        return Ok(new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = expiresAtUtc,
            Email = user.Email!,
            FullName = user.FullName,
            Roles = roles
        });
    }
}
