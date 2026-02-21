using System.ComponentModel.DataAnnotations;
using backend.Entities.Referrals;

namespace backend.DTO.Referrals
{
    public record JobOpeningListDto(
        long JobId,
        string JobTitle,
        string? Department,
        string? Location,
        JobType JobType,
        string? ExperienceRequired,
        string? JobSummary,
        bool IsActive,
        string? HrOwnerEmail,
        IReadOnlyCollection<string> CvReviewerEmails
    );

    public record JobOpeningDetailDto(
        long JobId,
        string JobTitle,
        string? Department,
        string? Location,
        JobType JobType,
        string? ExperienceRequired,
        string? JobSummary,
        string? JobDescriptionPath,
        bool IsActive,
        string? HrOwnerEmail,
        IReadOnlyCollection<string> CvReviewerEmails,
        long? PostedById,
        DateTime PostedAt,
        DateTime UpdatedAt
    );

    public class JobOpeningCreateDto
    {
        [Required, MaxLength(255)]
        public string JobTitle { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Department { get; set; }

        [MaxLength(255)]
        public string? Location { get; set; }

        public JobType JobType { get; set; } = JobType.FullTime;

        [MaxLength(50)]
        public string? ExperienceRequired { get; set; }

        [MaxLength(2000)]
        public string? JobSummary { get; set; }

        [MaxLength(500)]
        public string? JobDescriptionPath { get; set; }

        public IFormFile? JobDescriptionFile { get; set; }

        [EmailAddress]
        public string? HrOwnerEmail { get; set; }

        public List<string>? CvReviewerEmails { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public record JobOpeningUpdateDto(
        [Required, MaxLength(255)] string JobTitle,
        [MaxLength(100)] string? Department,
        [MaxLength(255)] string? Location,
        JobType JobType,
        [MaxLength(50)] string? ExperienceRequired,
        [MaxLength(2000)] string? JobSummary,
        [MaxLength(500)] string? JobDescriptionPath,
        [EmailAddress] string? HrOwnerEmail,
        List<string>? CvReviewerEmails,
        bool IsActive
    );
}