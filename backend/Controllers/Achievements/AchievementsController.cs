using backend.DTO.Achievements;
using backend.DTO.Common;
using backend.Repositories.Achievements;
using backend.Services.Achievements;
using backend.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Achievements;

[ApiController]
[Route("api/v1/achievements")]
public class AchievementsController : ControllerBase
{
    private readonly AchievementsService _service;
    private readonly AuthService _auth;

    public AchievementsController(AchievementsService service, AuthService auth)
    {
        _service = service;
        _auth = auth;
    }

    [Authorize]
    [HttpGet("posts")]
    public async Task<IActionResult> GetFeed([FromQuery] AchievementFeedFilterDto filter)
    {
        var currentUserId = _auth.GetUserId(User);
        if (currentUserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Code = 401,
                Error = "Invalid token, user not found."
            });
        }

        var result = await _service.GetFeedAsync(currentUserId.Value, new AchievementFeedFilter(
            filter.AuthorId,
            filter.Author,
            filter.Tag,
            filter.FromDate,
            filter.ToDate
        ));

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = 200,
            Data = result
        });
    }

    [Authorize]
    [HttpPost("posts")]
    public async Task<IActionResult> CreatePost([FromBody] AchievementPostCreateDto dto)
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
                Code = 401,
                Error = "Invalid token, user not found."
            });
        }

        try
        {
            var result = await _service.CreatePostAsync(currentUserId.Value, dto);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Code = 200,
                Data = result
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Code = 400,
                Error = ex.Message
            });
        }
    }

    [Authorize]
    [HttpPut("posts/{postId:long}")]
    public async Task<IActionResult> UpdatePost(long postId, [FromBody] AchievementPostUpdateDto dto)
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
                Code = 401,
                Error = "Invalid token, user not found."
            });
        }

        try
        {
            var result = await _service.UpdatePostAsync(postId, currentUserId.Value, User.IsInRole("HR"), dto);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Code = 200,
                Data = result
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Code = 400,
                Error = ex.Message
            });
        }
    }

    [Authorize]
    [HttpDelete("posts/{postId:long}")]
    public async Task<IActionResult> DeletePost(long postId, [FromQuery] string? reason)
    {
        var currentUserId = _auth.GetUserId(User);
        if (currentUserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Code = 401,
                Error = "Invalid token, user not found."
            });
        }

        try
        {
            await _service.DeletePostAsync(postId, currentUserId.Value, User.IsInRole("HR"), reason);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Code = 200
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Code = 400,
                Error = ex.Message
            });
        }
    }

    [Authorize]
    [HttpPost("posts/{postId:long}/comments")]
    public async Task<IActionResult> AddComment(long postId, [FromBody] AchievementCommentCreateDto dto)
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
                Code = 401,
                Error = "Invalid token, user not found."
            });
        }

        try
        {
            var result = await _service.AddCommentAsync(postId, currentUserId.Value, dto);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Code = 200,
                Data = result
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Code = 400,
                Error = ex.Message
            });
        }
    }

    [Authorize]
    [HttpPut("comments/{commentId:long}")]
    public async Task<IActionResult> UpdateComment(long commentId, [FromBody] AchievementCommentUpdateDto dto)
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
                Code = 401,
                Error = "Invalid token, user not found."
            });
        }

        try
        {
            var result = await _service.UpdateCommentAsync(commentId, currentUserId.Value, User.IsInRole("HR"), dto);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Code = 200,
                Data = result
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Code = 400,
                Error = ex.Message
            });
        }
    }

    [Authorize]
    [HttpDelete("comments/{commentId:long}")]
    public async Task<IActionResult> DeleteComment(long commentId, [FromQuery] string? reason)
    {
        var currentUserId = _auth.GetUserId(User);
        if (currentUserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Code = 401,
                Error = "Invalid token, user not found."
            });
        }

        try
        {
            await _service.DeleteCommentAsync(commentId, currentUserId.Value, User.IsInRole("HR"), reason);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Code = 200
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Code = 400,
                Error = ex.Message
            });
        }
    }

    [Authorize]
    [HttpPost("posts/{postId:long}/likes")]
    public async Task<IActionResult> LikePost(long postId)
    {
        var currentUserId = _auth.GetUserId(User);
        if (currentUserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Code = 401,
                Error = "Invalid token, user not found."
            });
        }

        try
        {
            await _service.LikePostAsync(postId, currentUserId.Value);
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Code = 200
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Code = 400,
                Error = ex.Message
            });
        }
    }

    [Authorize]
    [HttpDelete("posts/{postId:long}/likes")]
    public async Task<IActionResult> UnlikePost(long postId)
    {
        var currentUserId = _auth.GetUserId(User);
        if (currentUserId is null)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Code = 401,
                Error = "Invalid token, user not found."
            });
        }

        await _service.UnlikePostAsync(postId, currentUserId.Value);
        return Ok(new ApiResponse<object>
        {
            Success = true,
            Code = 200
        });
    }
}