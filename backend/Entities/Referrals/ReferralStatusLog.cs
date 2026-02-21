using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Entities.Common;

namespace backend.Entities.Referrals
{
    [Table("referral_status_logs")]
    public class ReferralStatusLog
    {
        [Key]
        [Column("pk_referral_status_log_id")]
        public long ReferralStatusLogId { get; set; }

        [Required]
        [Column("fk_referral_id")]
        public long ReferralId { get; set; }

        [Column("old_status")]
        public ReferralStatus? OldStatus { get; set; }

        [Required]
        [Column("new_status")]
        public ReferralStatus NewStatus { get; set; }

        [Column("recipients_snapshot")]
        public string? RecipientsSnapshot { get; set; }

        [Column("note")]
        public string? Note { get; set; }

        [Column("changed_at")]
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        [Column("fk_changed_by")]
        public long? ChangedById { get; set; }

        [ForeignKey("ReferralId")]
        public virtual Referral? Referral { get; set; }

        [ForeignKey("ChangedById")]
        public virtual User? ChangedBy { get; set; }
    }
}