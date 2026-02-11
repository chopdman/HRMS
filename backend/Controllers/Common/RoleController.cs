using backend.DTO.Common;
using backend.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Common;

[ApiController]
[Route("api/v1/roles")]
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
            return Created(string.Empty, new ApiResponse<object>
            {
                Status = 201,
                Success = true,
                Message = "Role created successfully.",
                Data = result
            });
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
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Status = 200,
            Message = "Role fetched successfully.",
            Data = roles
        });
    }

    [AllowAnonymous]
    [HttpGet("public")]
    public async Task<IActionResult> ListPublic()
    {
        var roles = await _service.GetAllRolesAsync();
        return Ok(roles);
    }
}