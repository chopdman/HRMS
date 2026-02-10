using backend.Services.Travels;
using backend.DTO.Travels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace backend.Controllers.Travels;

[ApiController]
[Route("api/v1/expenses")]
public class ExpenseController : ControllerBase
{
    private readonly ExpenseService _service;

    public ExpenseController(ExpenseService service)
    {
        _service = service;
    }

    [Authorize(Roles = "Employee")]
    [HttpPost]
    public async Task<IActionResult> CreateDraft([FromBody] ExpenseCreateDto dto)
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

        try
        {
            var result = await _service.CreateDraftAsync(dto, userId.Value);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Employee")]
    [HttpPost("{expenseId:int}/proofs")]
    [RequestSizeLimit(20_000_000)]
    public async Task<IActionResult> UploadProof(long expenseId, [FromForm] ExpenseProofUploadDto dto)
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

        try
        {
            await _service.UploadProofAsync(expenseId, dto, userId.Value);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Employee")]
    [HttpPost("{expenseId:int}/submit")]
    public async Task<IActionResult> Submit(long expenseId)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _service.SubmitAsync(expenseId, userId.Value);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "HR")]
    [HttpPost("{expenseId:int}/review")]
    public async Task<IActionResult> Review(long expenseId, [FromBody] ExpenseReviewDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var reviewerId = GetUserId();
        if (reviewerId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _service.ReviewAsync(expenseId, dto, reviewerId.Value);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Employee")]
    [HttpGet("my")]
    public async Task<IActionResult> MyExpenses()
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _service.ListForEmployeeAsync(userId.Value);
        return Ok(result);
    }

    [Authorize(Roles = "HR")]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] long? employeeId, [FromQuery] long? travelId, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? status)
    {
        var result = await _service.ListForHrAsync(employeeId, travelId, from, to, status);
        return Ok(result);
    }

    private long? GetUserId()
    {
        var sub = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (int.TryParse(sub, out var userId))
        {
            return userId;
        }

        return null;
    }
}