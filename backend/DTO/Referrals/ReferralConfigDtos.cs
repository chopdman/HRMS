using System.ComponentModel.DataAnnotations;

namespace backend.DTO.Referrals
{
    public record DefaultHrEmailUpdateDto(
        [Required, EmailAddress] string Email
    );

    public record DefaultHrEmailResponseDto(
        string? Email
    );

    public record AnjumHrEmailUpdateDto(
        [Required, EmailAddress] string Email
    );

    public record AnjumHrEmailResponseDto(
        string? Email
    );
}