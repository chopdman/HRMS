using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Entities.Achievements
{
    [Table("comment_likes")]
    public class CommentLike
    {
        [Key]
        [Column("pk_like_id")]
        public int LikeId { get; set; }

        [Required]
        [Column("fk_comment_id")]
        public int CommentId { get; set; }

        [Required]
        [Column("fk_user_id")]
        public int UserId { get; set; }

        [Column("liked_at")]
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("CommentId")]
        public virtual PostComment? Comment { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }

}
