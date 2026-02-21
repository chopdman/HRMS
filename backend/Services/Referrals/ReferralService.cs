using System.Net.Mail;
using backend.Data;
using backend.Config;
using backend.DTO.Referrals;
using backend.Entities.Common;
using backend.Entities.Referrals;
using backend.Repositories.Common;
using backend.Repositories.Referrals;
using backend.Services.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Services.Referrals
{
    public class ReferralService
    {
        private readonly AppDbContext _db;
        private readonly IJobOpeningRepository _jobRepository;
        private readonly IReferralRepository _referralRepository;
        private readonly IReferralStatusLogRepository _logRepository;
        private readonly IGlobalConfigRepository _configRepository;
        private readonly IEmailLogRepository _emailLogRepository;
        private readonly CloudinaryService _cloudinary;
        private readonly EmailService _emailService;
        private readonly EmailSettings _emailSettings;

        public ReferralService(
            AppDbContext db,
            IJobOpeningRepository jobRepository,
            IReferralRepository referralRepository,
            IReferralStatusLogRepository logRepository,
            IGlobalConfigRepository configRepository,
            IEmailLogRepository emailLogRepository,
            CloudinaryService cloudinary,
            EmailService emailService,
            IOptions<EmailSettings> emailOptions)
        {
            _db = db;
            _jobRepository = jobRepository;
            _referralRepository = referralRepository;
            _logRepository = logRepository;
            _configRepository = configRepository;
            _emailLogRepository = emailLogRepository;
            _cloudinary = cloudinary;
            _emailService = emailService;
            _emailSettings = emailOptions.Value;
        }

        public async Task<ReferralResponseDto> CreateAsync(long jobId, ReferralCreateDto dto, long referredById)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job is null || !job.IsActive)
            {
                throw new ArgumentException("Job opening not found or inactive.");
            }

            var referrer = await _db.Users.FirstOrDefaultAsync(u => u.UserId == referredById);
            if (referrer is null)
            {
                throw new ArgumentException("Referrer not found.");
            }

            if (!string.IsNullOrWhiteSpace(dto.FriendEmail))
            {
                ValidateEmail(dto.FriendEmail);
            }

            var uploadResult = await _cloudinary.UploadAsync(dto.CvFile, "referrals/cv");

            var recipientList = await BuildRecipientListAsync(job);
            if (recipientList.Count == 0)
            {
                throw new ArgumentException("No HR recipients configured for this job.");
            }

            var recipientSnapshot = string.Join(";", recipientList);
            var referral = new Referral
            {
                JobId = job.JobId,
                ReferredBy = referredById,
                FriendName = dto.FriendName,
                FriendEmail = dto.FriendEmail,
                CvFilePath = uploadResult.Url,
                Note = dto.Note,
                HrRecipients = recipientSnapshot,
                Status = ReferralStatus.New,
                SubmittedAt = DateTime.UtcNow
            };

            var created = await _referralRepository.AddAsync(referral);

            var subject = $"Referral: {job.JobTitle} (Job ID {job.JobId})";
            var body = BuildReferralBody(job, created, referrer);

            using var stream = dto.CvFile.OpenReadStream();
            var attachment = new EmailAttachment(stream, dto.CvFile.FileName, dto.CvFile.ContentType ?? "application/octet-stream");
            await _emailService.SendAsync(recipientList, subject, body, new[] { attachment });

            await _emailLogRepository.AddRangeAsync(recipientList.Select(email => new EmailLog
            {
                RecipientEmail = email,
                Subject = subject,
                EmailType = "Referral",
                JobId = job.JobId,
                ReferralId = created.ReferralId,
                SentAt = DateTime.UtcNow
            }));

            await _logRepository.AddAsync(new ReferralStatusLog
            {
                ReferralId = created.ReferralId,
                OldStatus = null,
                NewStatus = ReferralStatus.New,
                RecipientsSnapshot = recipientSnapshot,
                Note = "Referral submitted",
                ChangedAt = DateTime.UtcNow,
                ChangedById = referredById
            });

            return MapToResponse(created);
        }

        public async Task<IReadOnlyCollection<ReferralResponseDto>> GetByJobAsync(long jobId)
        {
            var referrals = await _referralRepository.GetByJobAsync(jobId);
            return referrals.Select(MapToResponse).ToList();
        }

        public async Task<IReadOnlyCollection<ReferralStatusLogDto>> GetLogsAsync(long referralId)
        {
            var logs = await _logRepository.GetByReferralAsync(referralId);
            return logs.Select(l => new ReferralStatusLogDto(
                l.ReferralStatusLogId,
                l.ReferralId,
                l.OldStatus,
                l.NewStatus,
                l.RecipientsSnapshot,
                l.Note,
                l.ChangedAt,
                l.ChangedById
            )).ToList();
        }

        public async Task<ReferralResponseDto> UpdateStatusAsync(long referralId, ReferralStatusUpdateDto dto, long updatedById)
        {
            var referral = await _referralRepository.GetByIdAsync(referralId);
            if (referral is null)
            {
                throw new ArgumentException("Referral not found.");
            }

            if (referral.Status == dto.Status)
            {
                throw new ArgumentException("Referral is already in the specified status.");
            }

            var oldStatus = referral.Status;
            referral.Status = dto.Status;
            referral.StatusUpdatedAt = DateTime.UtcNow;
            referral.StatusUpdatedBy = updatedById;

            await _referralRepository.SaveAsync();

            await _logRepository.AddAsync(new ReferralStatusLog
            {
                ReferralId = referral.ReferralId,
                OldStatus = oldStatus,
                NewStatus = dto.Status,
                RecipientsSnapshot = referral.HrRecipients,
                Note = dto.Note,
                ChangedAt = DateTime.UtcNow,
                ChangedById = updatedById
            });

            return MapToResponse(referral);
        }

        public async Task<DefaultHrEmailResponseDto> GetDefaultHrEmailAsync()
        {
            var config = await _configRepository.GetByFieldAsync(ReferralExtensions.DefaultHrEmailConfigKey);
            var email = config?.ConfigValue;
            if (string.IsNullOrWhiteSpace(email))
            {
                email = _emailSettings.HrMailbox;
            }

            return new DefaultHrEmailResponseDto(string.IsNullOrWhiteSpace(email) ? null : email);
        }

        public async Task<DefaultHrEmailResponseDto> UpdateDefaultHrEmailAsync(string email, long updatedById)
        {
            ValidateEmail(email);

            var config = new GlobalConfig
            {
                ConfigField = ReferralExtensions.DefaultHrEmailConfigKey,
                ConfigValue = email.Trim(),
                RelatedTable = "job_openings",
                UpdatedBy = updatedById,
                UpdatedAt = DateTime.UtcNow
            };

            var saved = await _configRepository.UpsertAsync(config);
            return new DefaultHrEmailResponseDto(saved.ConfigValue);
        }

        public async Task<AnjumHrEmailResponseDto> GetAnjumHrEmailAsync()
        {
            var config = await _configRepository.GetByFieldAsync(ReferralExtensions.AnjumEmailConfigKey);
            var email = config?.ConfigValue;
            if (string.IsNullOrWhiteSpace(email))
            {
                email = _emailSettings.AnjumEmail;
            }

            return new AnjumHrEmailResponseDto(string.IsNullOrWhiteSpace(email) ? null : email);
        }

        public async Task<AnjumHrEmailResponseDto> UpdateAnjumHrEmailAsync(string email, long updatedById)
        {
            ValidateEmail(email);

            var config = new GlobalConfig
            {
                ConfigField = ReferralExtensions.AnjumEmailConfigKey,
                ConfigValue = email.Trim(),
                RelatedTable = "job_openings",
                UpdatedBy = updatedById,
                UpdatedAt = DateTime.UtcNow
            };

            var saved = await _configRepository.UpsertAsync(config);
            return new AnjumHrEmailResponseDto(saved.ConfigValue);
        }

        private async Task<List<string>> BuildRecipientListAsync(JobOpening job)
        {
            var recipients = new List<string>();

            var defaultHr = await _configRepository.GetByFieldAsync(ReferralExtensions.DefaultHrEmailConfigKey);
            if (!string.IsNullOrWhiteSpace(defaultHr?.ConfigValue))
            {
                recipients.Add(defaultHr.ConfigValue);
            }
            else if (!string.IsNullOrWhiteSpace(_emailSettings.HrMailbox))
            {
                recipients.Add(_emailSettings.HrMailbox);
            }

            if (!string.IsNullOrWhiteSpace(job.HrOwnerEmail))
            {
                recipients.Add(job.HrOwnerEmail);
            }

            var anjumConfig = await _configRepository.GetByFieldAsync(ReferralExtensions.AnjumEmailConfigKey);
            if (!string.IsNullOrWhiteSpace(anjumConfig?.ConfigValue))
            {
                recipients.Add(anjumConfig.ConfigValue);
            }
            else if (!string.IsNullOrWhiteSpace(_emailSettings.AnjumEmail))
            {
                recipients.Add(_emailSettings.AnjumEmail);
            }

            recipients.AddRange(JobOpeningService.ParseEmails(job.CvReviewerEmails));

            var cleaned = recipients
                .Where(email => !string.IsNullOrWhiteSpace(email))
                .Select(email => email.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var email in cleaned)
            {
                ValidateEmail(email);
            }

            return cleaned;
        }

        private static string BuildReferralBody(JobOpening job, Referral referral, User referrer)
        {
            var friendEmail = string.IsNullOrWhiteSpace(referral.FriendEmail) ? "N/A" : referral.FriendEmail;
            var note = string.IsNullOrWhiteSpace(referral.Note) ? "N/A" : referral.Note;

            return $"Job Title: {job.JobTitle}\n" +
                   $"Job ID: {job.JobId}\n" +
                   $"Referrer: {referrer.FullName} (Employee ID: {referrer.UserId})\n" +
                   $"Friend Name: {referral.FriendName}\n" +
                   $"Friend Email: {friendEmail}\n" +
                   $"Note: {note}\n";
        }

        private static ReferralResponseDto MapToResponse(Referral referral)
        {
            return new ReferralResponseDto(
                referral.ReferralId,
                referral.JobId,
                referral.ReferredBy,
                referral.FriendName ?? string.Empty,
                referral.FriendEmail,
                referral.CvFilePath ?? string.Empty,
                referral.Note,
                referral.Status,
                referral.HrRecipients,
                referral.SubmittedAt,
                referral.StatusUpdatedAt,
                referral.StatusUpdatedBy
            );
        }

        private static void ValidateEmail(string email)
        {
            _ = new MailAddress(email.Trim());
        }
    }
}