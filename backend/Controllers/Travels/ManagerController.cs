using backend.Services.Travels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Controllers.Travels;

[ApiController]
[Route("api/v1/manager")]
public class ManagerController : ControllerBase
{
    private readonly ManagerService _service;

    public ManagerController(ManagerService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Manager")]
    [HttpGet("team-members")]
    public async Task<IActionResult> TeamMembers()
    {
        var managerId = GetUserId();
        if (managerId is null)
        {
            return Unauthorized();
        }

        var result = await _service.GetTeamMembersAsync(managerId.Value);
        return Ok(result);
    }

    [Authorize(Roles = "Manager")]
    [HttpGet("team-expenses")]
    public async Task<IActionResult> TeamExpenses([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var managerId = GetUserId();
        if (managerId is null)
        {
            return Unauthorized();
        }

        var result = await _service.GetTeamExpensesAsync(managerId.Value, from, to);
        return Ok(result);
    }

    private int? GetUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (int.TryParse(sub, out var userId))
        {
            return userId;
        }

        return null;
    }
}