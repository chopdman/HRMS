using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend.Entities.Common;

namespace backend.Entities.Achievements
{
    [Table("achievement_posts")]
    public class AchievementPost
    {
        [Key]
        [Column("pk_post_id")]
        public long PostId { get; set; }

        [Required]
        [Column("fk_author_id")]
        public long AuthorId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("title")]
        public string? Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("tags")]
        public string? Tags { get; set; }

        [Column("post_visibility")]
        public PostVisibility Visibility { get; set; } = PostVisibility.AllEmployees;

        [Column("post_type")]
        public PostType PostType { get; set; } = PostType.Achievement;

        [Column("is_system_generated")]
        public bool IsSystemGenerated { get; set; } = false;

        [MaxLength(200)]
        [Column("system_key")]
        public string? SystemKey { get; set; }


        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("AuthorId")]
        public virtual User? Author { get; set; }

        public ICollection<PostComment> Comments { get; set; } = new List<PostComment>();

        public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();

    }

    public enum PostType
    {
        Achievement,
        Birthday,
        WorkAnniversary
    }

    public enum PostVisibility
    {
        AllEmployees
    }

}