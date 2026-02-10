using backend.Config;
using backend.Data;
using backend.Entities.Common;
using backend.DTO.Common;
using backend.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Controllers.Common;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher _hasher;
    private readonly TokenService _tokenService;
    private readonly IOptions<JwtSettings> _jwtSettings;

    public AuthController(AppDbContext db, PasswordHasher hasher, TokenService tokenService, IOptions<JwtSettings> jwtSettings)
    {
        _db = db;
        _hasher = hasher;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }
        var exists = await _db.Users.AnyAsync(u => u.Email == request.Email);
        if (exists)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Status = 400,
                Errors = new List<string> { "Email already registered." }
            });
        }

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.RoleId == request.RoleId);
        if (role is null)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = "Role not found."
            });
        }

        var (hash, salt) = _hasher.HashPassword(request.Password);
        var user = new User
        {
            Email = request.Email,
            Password = hash,
            PasswordSalt = salt,
            FullName = request.FullName,
            RoleId = role.RoleId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var registerResponse = new RegisterResponse(user.UserId,
         user.Email, user.FullName, role.Name);


        return Created(string.Empty, new ApiResponse<RegisterResponse>
        {
            Success = true,
            Message = "User is created successfully",
            Data = registerResponse
        });

    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var user = await _db.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null || !_hasher.Verify(request.Password, user.Password, user.PasswordSalt))
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Message = "Invalid username or password"
            });
        }

        var (accessToken, accessExpires) = _tokenService.GenerateAccessToken(user);
        var (refreshToken, refreshExpires, refreshHash) = _tokenService.GenerateRefreshToken(_jwtSettings.Value.RefreshTokenDays);

        var refreshEntity = new UserRefreshToken
        {
            UserId = user.UserId,
            TokenHash = refreshHash,
            ExpiresAt = refreshExpires,
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        _db.UserRefreshTokens.Add(refreshEntity);
        await _db.SaveChangesAsync();

        return Ok(new AuthResponse(accessToken, accessExpires, refreshToken, refreshExpires));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var tokenHash = _tokenService.HashToken(request.RefreshToken);
        var stored = await _db.UserRefreshTokens
            .Include(t => t.User)
            .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (stored is null || !stored.IsActive || stored.User is null)
        {
            return Unauthorized();
        }

        stored.RevokedAt = DateTime.UtcNow;

        var (newAccessToken, newAccessExpires) = _tokenService.GenerateAccessToken(stored.User);
        var (newRefreshToken, newRefreshExpires, newRefreshHash) = _tokenService.GenerateRefreshToken(_jwtSettings.Value.RefreshTokenDays);

        stored.ReplacedByTokenHash = newRefreshHash;

        _db.UserRefreshTokens.Add(new UserRefreshToken
        {
            UserId = stored.UserId,
            TokenHash = newRefreshHash,
            ExpiresAt = newRefreshExpires,
            CreatedByIp = HttpContext.Connection.RemoteIpAddress?.ToString()
        });

        await _db.SaveChangesAsync();

        return Ok(new AuthResponse(newAccessToken, newAccessExpires, newRefreshToken, newRefreshExpires));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var tokenHash = _tokenService.HashToken(request.RefreshToken);
        var stored = await _db.UserRefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        if (stored is null)
        {
            return NotFound(new
            {
                Message = "Refresh token not found or already revoked.",
                Token = tokenHash
            });
        }

        stored.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Ok(new
        {
            Message = "Refresh token successfully revoked.",
            RevokedAt = stored.RevokedAt
        });
    }
}