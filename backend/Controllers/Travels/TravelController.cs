using backend.DTO.Travels;
using backend.Services.Travels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Controllers.Travels;

[ApiController]
[Route("api/v1/travels")]
public class TravelController : ControllerBase
{
    private readonly TravelService _service;

    public TravelController(TravelService service)
    {
        _service = service;
    }

    [Authorize(Roles = "HR")]
    [HttpPost]
    public async Task<IActionResult> CreateTravel([FromBody] TravelCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _service.CreateTravelAsync(dto);
            return CreatedAtAction(nameof(CreateTravel), new { id = result.TravelId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Employee,Manager,HR")]
    [HttpGet("assigned")]
    public async Task<IActionResult> GetAssignedTravels([FromQuery] long? employeeId)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var subValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var userId = 0;
        if (!string.IsNullOrWhiteSpace(subValue))
        {
            _ = int.TryParse(subValue, out userId);
        }

        if (userId == 0)
        {
            return Unauthorized();
        }

        var resolvedEmployeeId = role == "Employee" ? userId : employeeId;
        if (resolvedEmployeeId is null)
        {
            return BadRequest(new { message = "employeeId is required for this role." });
        }

        var result = await _service.GetAssignedTravelsAsync(resolvedEmployeeId.Value);
        return Ok(result);
    }

    [Authorize(Roles = "Employee,Manager,HR")]
    [HttpGet("assignments")]
    public async Task<IActionResult> GetAssignments([FromQuery] long? employeeId)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var subValue = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var userId = 0;
        if (!string.IsNullOrWhiteSpace(subValue))
        {
            _ = int.TryParse(subValue, out userId);
        }

        if (userId == 0)
        {
            return Unauthorized();
        }

        var resolvedEmployeeId = role == "Employee" ? userId : employeeId;
        if (resolvedEmployeeId is null)
        {
            return BadRequest(new { message = "employeeId is required for this role." });
        }

        var result = await _service.GetAssignmentsForEmployeeAsync(resolvedEmployeeId.Value);
        return Ok(result);
    }

}