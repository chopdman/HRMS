using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Entities;


[Table("notifications")]
public class Notification
{
    [Key]
    [Column("pk_notification_id")]
    public int NotificationId { get; set; }

    [Required]
    [Column("fk_user_id")]
    public int UserId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}