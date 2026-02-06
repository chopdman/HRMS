using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Entities
{
    [Table("users")]
    public class User
    {
        [Key]
        [Column("pk_user_id")]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("email")]
        public  string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(20)]
        [Column("phone")]
        public string Phone { get; set; } = string.Empty;

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Required]
        [Column("date_of_joining")]
        public DateTime DateOfJoining { get; set; }

        [MaxLength(500)]
        [Column("profile_photo_url")]
        public string? ProfilePhotoUrl { get; set; }

        [MaxLength(100)]
        [Column("department")]
        public string? Department { get; set; }

        [MaxLength(100)]
        [Column("designation")]
        public string? Designation { get; set; }

        [Column("fk_manager_id")]
        public int? ManagerId { get; set; }

        [Required]
        [Column("role")]
        public UserRole Role { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("hash_refresh_token")]
        public string? RefreshTokenHash { get; set; }

        [Column("refresh_token_expiry")]
        public DateTime? RefreshTokenExpiryTime { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ManagerId")]
        public virtual User? Manager { get; set; }


    }

    public enum UserRole
    {
        Employee,
        Manager,
        HR
    }
}

 

