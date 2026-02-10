using backend.DTO.Common;
using backend.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Common;

[ApiController]
[Route("api/roles")]
public class RoleController : ControllerBase
{
    private readonly RoleService _service;

    public RoleController(RoleService service)
    {
        _service = service;
    }

    [Authorize(Roles = "HR")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RoleCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var result = await _service.CreateRoleAsync(dto);
            return CreatedAtAction(nameof(Create), new { id = result.RoleId }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var roles = await _service.GetAllRolesAsync();
        return Ok(roles);
    }

//not needed just for testing
    [AllowAnonymous]
    [HttpGet("public")]
    public async Task<IActionResult> ListPublic()
    {
        var roles = await _service.GetAllRolesAsync();
        return Ok(roles);
    }
}