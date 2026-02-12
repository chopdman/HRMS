using backend.Data;
using backend.DTO.Common;
using backend.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
 
namespace backend.Controllers.Common;
 
[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
 
    private readonly UserService _service;
 
    public UsersController(UserService userService)
    {
        _service = userService;
    }
 
    [Authorize(Roles = "HR")]
    [HttpGet]
    public async Task<IActionResult> ListEmployees()
    {
        var results = await _service.GetListOfEmployee();
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = 200,
            Data = results
        });
 
    }
 
    [Authorize(Roles = "HR")]
    [HttpGet("search")]
    public async Task<IActionResult> SearchEmployees([FromQuery] string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Code = 400,
                Error = "Query is required."
            });
        }
 
        var trimmed = query.Trim();
        var results = await _service.SearchEmployee(trimmed);
 
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = 200,
            Data = results
        });
    }
}
 