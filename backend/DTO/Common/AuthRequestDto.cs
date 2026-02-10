using System.ComponentModel.DataAnnotations;

namespace backend.DTO.Common
{

    public record RegisterRequest(
        [Required, EmailAddress] string Email,
        [Required, MinLength(8)] string Password,
        [Required, MaxLength(200)] string FullName,
        [Required] int RoleId
    );

    public record LoginRequest(
        [Required, EmailAddress] string Email,
        [Required] string Password
    );

    public record RefreshRequest(
        [Required] string RefreshToken
    );

    public record AuthResponse(
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresAt
    );

}