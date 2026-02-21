using System.ComponentModel.DataAnnotations;

namespace backend.DTO.Referrals
{
    public record JobShareRequestDto(
        [Required, MinLength(1)] List<string> RecipientEmails
    );

    public record JobShareResponseDto(
        long JobId,
        IReadOnlyCollection<string> Recipients,
        DateTime SharedAt
    );
}