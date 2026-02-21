using System.Net.Http;
using System.Net.Mail;
using backend.DTO.Referrals;
using backend.Entities.Referrals;
using backend.Repositories.Referrals;
using backend.Services.Common;

namespace backend.Services.Referrals
{
    public class JobShareService
    {
        private readonly IJobOpeningRepository _jobRepository;
        private readonly IJobShareRepository _shareRepository;
        private readonly EmailService _emailService;
        private readonly IHttpClientFactory _httpClientFactory;

        public JobShareService(IJobOpeningRepository jobRepository, IJobShareRepository shareRepository, EmailService emailService, IHttpClientFactory httpClientFactory)
        {
            _jobRepository = jobRepository;
            _shareRepository = shareRepository;
            _emailService = emailService;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<JobShareResponseDto> ShareAsync(long jobId, JobShareRequestDto dto, long sharedById)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job is null || !job.IsActive)
            {
                throw new ArgumentException("Job opening not found or inactive.");
            }

            var recipients = ValidateEmails(dto.RecipientEmails);
            if (recipients.Count == 0)
            {
                throw new ArgumentException("At least one valid recipient email is required.");
            }

            var attachmentPath = job.JobDescriptionPath;
            if (string.IsNullOrWhiteSpace(attachmentPath))
            {
                throw new ArgumentException("Job description file path is not configured.");
            }

            var subject = $"Job Opening: {job.JobTitle}";
            var body = BuildShareBody(job);
            var sharedAt = DateTime.UtcNow;

            var attachment = await GetAttachmentAsync(attachmentPath);
            await _emailService.SendAsync(recipients, subject, body, new[] { attachment });

            var shares = recipients.Select(recipient => new JobShare
            {
                JobId = job.JobId,
                SharedBy = sharedById,
                RecipientEmail = recipient,
                SharedAt = sharedAt
            }).ToList();

            await _shareRepository.AddRangeAsync(shares);

            return new JobShareResponseDto(job.JobId, recipients, sharedAt);
        }

        private async Task<EmailAttachment> GetAttachmentAsync(string pathOrUrl)
        {
            if (pathOrUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                var client = _httpClientFactory.CreateClient();
                using var response = await client.GetAsync(pathOrUrl);
                response.EnsureSuccessStatusCode();

                await using var responseStream = await response.Content.ReadAsStreamAsync();
                var memoryStream = new MemoryStream();
                await responseStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                var fileName = GetFileNameFromUrl(pathOrUrl);

                return new EmailAttachment(memoryStream, fileName, contentType);
            }

            if (!File.Exists(pathOrUrl))
            {
                throw new ArgumentException("Job description file not found.");
            }

            var stream = File.OpenRead(pathOrUrl);
            return new EmailAttachment(stream, Path.GetFileName(pathOrUrl), "application/octet-stream");
        }

        private static string GetFileNameFromUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                var name = Path.GetFileName(uri.LocalPath);
                if (!string.IsNullOrWhiteSpace(name))
                {
                    return name;
                }
            }

            return "job-description";
        }

        private static string BuildShareBody(JobOpening job)
        {
            var summary = string.IsNullOrWhiteSpace(job.JobSummary) ? "N/A" : job.JobSummary;
            var location = string.IsNullOrWhiteSpace(job.Location) ? "N/A" : job.Location;
            var department = string.IsNullOrWhiteSpace(job.Department) ? "N/A" : job.Department;

            return $"Job Title: {job.JobTitle}\n" +
                   $"Job ID: {job.JobId}\n" +
                   $"Department: {department}\n" +
                   $"Location: {location}\n" +
                   $"Summary: {summary}\n";
        }

        private static List<string> ValidateEmails(IEnumerable<string> emails)
        {
            var recipients = new List<string>();
            foreach (var email in emails)
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    continue;
                }

                try
                {
                    var normalized = email.Trim();
                    _ = new MailAddress(normalized);
                    recipients.Add(normalized);
                }
                catch
                {
                    throw new ArgumentException("Please insert valid email");
                }
            }

            return recipients
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}