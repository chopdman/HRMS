using backend.Data;
using backend.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/users")]
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
         u => EF.Property<int?>(u, "RoleId"),
         r => EF.Property<int?>(r, "RoleId"),
         (o, i) => new { User = o, Role = i } 
     )
     .Where(u => u.Role != null && u.Role.Name == "Employee")
     .AsEnumerable()
     .OrderBy(u => new EmployeeLookupDto(
         u.User.UserId,
         u.User.FullName,
         u.User.Email
     ).FullName)
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
        u => (object)EF.Property<int?>(u, "RoleId")!,
        r => (object)EF.Property<int?>(r, "RoleId")!,
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