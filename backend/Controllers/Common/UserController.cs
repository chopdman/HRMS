using backend.Data;
using backend.DTO.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers.Common;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db)
    {
        _db = db;
    }

    [Authorize(Roles = "HR")]
    [HttpGet]
    public async Task<IActionResult> ListEmployees()
    {
        var results = _db.Users
     .Join(
         _db.Roles,
         u => EF.Property<long?>(u, "RoleId"),
         r => EF.Property<long?>(r, "RoleId"),
         (o, i) => new { User = o, Role = i }
     ).Join(
        _db.Users,
        ur => EF.Property<long?>(ur.User, "ManagerId"),
        m => EF.Property<long?>(m, "UserId"),
        (ur, manager) => new
        {
            ur.User,
            ur.Role,
            ManagerName = manager.FullName
        }
    )
     .Where(u => u.Role != null)
     .AsEnumerable()
     .OrderBy(u => u.User.FullName).Select(u => new UserResponseDto(
         u.User.UserId,
         u.User.FullName,
         u.User.Email,
         u.User.Phone,
         u.User.DateOfBirth,
         u.User.DateOfJoining,
         u.User.ProfilePhotoUrl,
         u.User.Department,
         u.User.Designation,
         u.ManagerName,
         u.Role.Name
     ))
     .ToList();


        return Ok(results);
    }

    [Authorize(Roles = "HR")]
    [HttpGet("search")]
    public async Task<IActionResult> SearchEmployees([FromQuery] string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Ok(Array.Empty<EmployeeLookupDto>());
        }

        var trimmed = query.Trim();

        var results = await _db.Users
     .Join(
         _db.Roles,
         u => (object)EF.Property<long?>(u, "RoleId")!,
         r => (object)EF.Property<long?>(r, "RoleId")!,
         (u, r) => new { User = u, Role = r }
     )
     .Where(ti => ti.Role != null && ti.Role.Name == "Employee")
     .Where(ti => ti.User.FullName.Contains(@trimmed) || ti.User.Email.Contains(@trimmed))
     .OrderBy(ti => ti.User.FullName)
     .Select(ti => new EmployeeLookupDto(ti.User.UserId, ti.User.FullName, ti.User.Email))
     .ToListAsync();

        return Ok(results);
    }
}