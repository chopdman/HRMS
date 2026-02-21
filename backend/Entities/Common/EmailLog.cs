using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Entities.Common
{
    [Table("email_logs")]
    public class EmailLog
    {
        [Key]
        [Column("pk_email_log_id")]
        public long EmailLogId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("recipient_email")]
        public string RecipientEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("subject")]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("email_type")]
        public string EmailType { get; set; } = string.Empty;

        [Column("fk_job_id")]
        public long? JobId { get; set; }

        [Column("fk_referral_id")]
        public long? ReferralId { get; set; }

        [Column("sent_at")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}