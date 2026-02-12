using backend.DTO.Common;
using backend.DTO.Travels;
using backend.Services.Common;
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

    private readonly AuthService _auth;

    public TravelController(TravelService service, AuthService authService)
    {
        _service = service;
        _auth = authService;
    }

    [Authorize(Roles = "HR")]
    [HttpPost]
    public async Task<IActionResult> CreateTravel([FromBody] TravelCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var currentUserId = _auth.GetUserId(User);
        if (currentUserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Status = 401,
                Message = "Invalid token, user not found."
            });
        }
        try
        {
            var result = await _service.CreateTravelAsync(dto, currentUserId.Value);
            return CreatedAtAction(nameof(CreateTravel), new { id = result.TravelId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // [Authorize(Roles = "Employee,Manager,HR")]
    // [HttpGet("assigned")]
    // public async Task<IActionResult> GetAssignedTravels([FromQuery] long? employeeId)
    // {

    //     var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    //     var userId =  _auth.GetUserId(User);


    //     var resolvedEmployeeId = role == "Employee" ? userId : employeeId;
    //     if (resolvedEmployeeId is null)
    //     {
    //         return BadRequest(new { message = "employeeId is required for this role." });
    //     }

    //     var result = await _service.GetAssignedTravelsAsync(resolvedEmployeeId.Value);
    //     return Ok(result);
    // }

    [Authorize(Roles = "Employee,Manager,HR")]
    [HttpGet("assignments")]
    public async Task<IActionResult> GetAssignments([FromQuery] long? employeeId)
    {
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        var userId = _auth.GetUserId(User);

        var resolvedEmployeeId = role == "Employee" ? userId : employeeId;
        if (resolvedEmployeeId is null)
        {
            return BadRequest(new { message = "employeeId is required for this role." });
        }

        var result = await _service.GetAssignmentsForEmployeeAsync(resolvedEmployeeId.Value);
        return Ok(result);
    }

}