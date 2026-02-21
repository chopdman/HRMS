using backend.DTO.Referrals;
using backend.Entities.Referrals;
using backend.Repositories.Referrals;
using backend.Services.Common;

namespace backend.Services.Referrals
{
    public class JobOpeningService
    {
        private readonly IJobOpeningRepository _repository;
        private readonly CloudinaryService _cloudinary;

        public JobOpeningService(IJobOpeningRepository repository, CloudinaryService cloudinary)
        {
            _repository = repository;
            _cloudinary = cloudinary;
        }

        public async Task<IReadOnlyCollection<JobOpeningListDto>> GetOpeningsAsync(bool activeOnly)
        {
            var openings = await _repository.GetActiveAsync(activeOnly);
            return openings.Select(MapToListDto).ToList();
        }

        public async Task<JobOpeningDetailDto> GetByIdAsync(long jobId)
        {
            var opening = await _repository.GetByIdAsync(jobId);
            if (opening is null)
            {
                throw new ArgumentException("Job opening not found.");
            }

            return MapToDetailDto(opening);
        }

        public async Task<JobOpeningDetailDto> CreateAsync(JobOpeningCreateDto dto, long postedById)
        {
            string? jobDescriptionPath = dto.JobDescriptionPath;
            if (dto.JobDescriptionFile is not null)
            {
                var uploadResult = await _cloudinary.UploadAsync(dto.JobDescriptionFile, "referrals/jd");
                jobDescriptionPath = uploadResult.Url;
            }

            var opening = new JobOpening
            {
                JobTitle = dto.JobTitle,
                Department = dto.Department,
                Location = dto.Location,
                JobType = dto.JobType,
                ExperienceRequired = dto.ExperienceRequired,
                JobSummary = dto.JobSummary,
                JobDescriptionPath = jobDescriptionPath,
                HrOwnerEmail = dto.HrOwnerEmail,
                CvReviewerEmails = SerializeEmails(dto.CvReviewerEmails),
                IsActive = dto.IsActive,
                PostedBy = postedById
            };

            var created = await _repository.AddAsync(opening);
            return MapToDetailDto(created);
        }

        public async Task<JobOpeningDetailDto> UpdateAsync(long jobId, JobOpeningUpdateDto dto)
        {
            var opening = await _repository.GetByIdAsync(jobId);
            if (opening is null)
            {
                throw new ArgumentException("Job opening not found.");
            }

            opening.JobTitle = dto.JobTitle;
            opening.Department = dto.Department;
            opening.Location = dto.Location;
            opening.JobType = dto.JobType;
            opening.ExperienceRequired = dto.ExperienceRequired;
            opening.JobSummary = dto.JobSummary;
            opening.JobDescriptionPath = dto.JobDescriptionPath;
            opening.HrOwnerEmail = dto.HrOwnerEmail;
            opening.CvReviewerEmails = SerializeEmails(dto.CvReviewerEmails);
            opening.IsActive = dto.IsActive;
            opening.UpdatedAt = DateTime.UtcNow;

            await _repository.SaveAsync();
            return MapToDetailDto(opening);
        }

        private static JobOpeningListDto MapToListDto(JobOpening opening)
        {
            return new JobOpeningListDto(
                opening.JobId,
                opening.JobTitle ?? string.Empty,
                opening.Department,
                opening.Location,
                opening.JobType,
                opening.ExperienceRequired,
                opening.JobSummary,
                opening.IsActive,
                opening.HrOwnerEmail,
                ParseEmails(opening.CvReviewerEmails)
            );
        }

        private static JobOpeningDetailDto MapToDetailDto(JobOpening opening)
        {
            return new JobOpeningDetailDto(
                opening.JobId,
                opening.JobTitle ?? string.Empty,
                opening.Department,
                opening.Location,
                opening.JobType,
                opening.ExperienceRequired,
                opening.JobSummary,
                opening.JobDescriptionPath,
                opening.IsActive,
                opening.HrOwnerEmail,
                ParseEmails(opening.CvReviewerEmails),
                opening.PostedBy,
                opening.PostedAt,
                opening.UpdatedAt
            );
        }

        public static IReadOnlyCollection<string> ParseEmails(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return Array.Empty<string>();
            }

            return raw
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(email => email.Trim())
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .ToList();
        }

        public static string? SerializeEmails(IReadOnlyCollection<string>? emails)
        {
            if (emails is null || emails.Count == 0)
            {
                return null;
            }

            var cleaned = emails
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Select(email => email.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var email in cleaned)
            {
                _ = new System.Net.Mail.MailAddress(email);
            }

            return cleaned.Count == 0 ? null : string.Join(";", cleaned);
        }
    }
}