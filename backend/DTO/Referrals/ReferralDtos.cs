using System.ComponentModel.DataAnnotations;
using backend.Entities.Referrals;

namespace backend.DTO.Referrals
{
    public class ReferralCreateDto
    {
        [Required]
        public string FriendName { get; set; } = string.Empty;

        [EmailAddress]
        public string? FriendEmail { get; set; }

        [Required]
        public IFormFile CvFile { get; set; } = null!;

        [MaxLength(2000)]
        public string? Note { get; set; }
    }

    public record ReferralResponseDto(
        long ReferralId,
        long JobId,
        long ReferredById,
        string FriendName,
        string? FriendEmail,
        string CvFilePath,
        string? Note,
        ReferralStatus Status,
        string? HrRecipients,
        DateTime SubmittedAt,
        DateTime? StatusUpdatedAt,
        long? StatusUpdatedBy
    );

    public record ReferralStatusUpdateDto(
        [Required] ReferralStatus Status,
        [MaxLength(2000)] string? Note
    );

    public record ReferralStatusLogDto(
        long ReferralStatusLogId,
        long ReferralId,
        ReferralStatus? OldStatus,
        ReferralStatus NewStatus,
        string? RecipientsSnapshot,
        string? Note,
        DateTime ChangedAt,
        long? ChangedById
    );
}