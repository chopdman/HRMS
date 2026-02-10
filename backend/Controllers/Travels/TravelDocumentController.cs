using backend.DTO.Travels;
using backend.Services.Travels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Controllers.Travels;

[ApiController]
[Route("api/travel-documents")]
public class TravelDocumentController : ControllerBase
{
    private readonly TravelDocumentService _service;

    public TravelDocumentController(TravelDocumentService service)
    {
        _service = service;
    }

    [Authorize(Roles = "HR,Employee")]
    [HttpPost]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> Upload([FromForm] TravelDocumentUploadDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        try
        {
            var result = await _service.UploadAsync(dto, userId.Value, role);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "HR,Employee,Manager")]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] int? travelId, [FromQuery] int? employeeId)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        try
        {
            var result = await _service.ListAsync(userId.Value, role, travelId, employeeId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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