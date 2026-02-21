using System.Net;
using System.Net.Mail;
using backend.Config;
using Microsoft.Extensions.Options;

namespace backend.Services.Common;

public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public Task SendAsync(string toEmail, string subject, string body)
    {
        return SendAsync(new[] { toEmail }, subject, body, null);
    }

    public async Task SendAsync(IEnumerable<string> toEmails, string subject, string body, IReadOnlyCollection<EmailAttachment>? attachments)
    {
        var recipientList = toEmails
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Select(email => email.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (recipientList.Count == 0)
        {
            throw new ArgumentException("At least one recipient is required.");
        }

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        foreach (var email in recipientList)
        {
            message.To.Add(email);
        }

        if (attachments is not null)
        {
            foreach (var attachment in attachments)
            {
                if (attachment.Content.CanSeek)
                {
                    attachment.Content.Position = 0;
                }
                message.Attachments.Add(new Attachment(attachment.Content, attachment.FileName, attachment.ContentType));
            }
        }

        await client.SendMailAsync(message);
    }
}

public sealed class EmailAttachment
{
    public EmailAttachment(Stream content, string fileName, string contentType)
    {
        Content = content;
        FileName = fileName;
        ContentType = contentType;
    }

    public Stream Content { get; }
    public string FileName { get; }
    public string ContentType { get; }
}