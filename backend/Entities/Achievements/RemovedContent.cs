using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Entities.Achievements
{
    [Table("removed_contents")]
    public class RemovedContent
    {
        [Key]
        [Column("pk_log_id")]
        public int LogId { get; set; }

        [Required]
        [Column("content_type")]
        public ContentType ContentType { get; set; }

        [Required]
        [Column("fk_content_id")]
        public int ContentId { get; set; }

        [Required]
        [Column("fk_deleted_by")]
        public int DeletedBy { get; set; }

        [Required]
        [Column("fk_author_id")]
        public int AuthorId { get; set; }

        [Column("reason")]
        public string? Reason { get; set; }

        [Column("deleted_at")]
        public DateTime DeletedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("DeletedBy")]
        public virtual User? Moderator { get; set; }

        [ForeignKey("AuthorId")]
        public virtual User? OriginalAuthor { get; set; }
    }


    public enum ContentType
    {
        Post,
        Comment
    }

}
