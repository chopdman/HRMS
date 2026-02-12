using backend.Services.Travels;
using backend.DTO.Common;
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
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Code = 401,
                Error = "Invalid token, user not found."
            });
        }
 
        var result = await _service.GetTeamMembersAsync(managerId.Value);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = 200,
            Data = result
        });
    }
 
    [Authorize(Roles = "Manager")]
    [HttpGet("team-expenses")]
    public async Task<IActionResult> TeamExpenses([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var managerId = GetUserId();
        if (managerId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Code = 401,
                Error = "Invalid token, user not found."
            });
        }
 
        var result = await _service.GetTeamExpensesAsync(managerId.Value, from, to);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = 200,
            Data = result
        });
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
 