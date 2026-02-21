using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Entities.Common
{
    [Table("global_config")]
    public class GlobalConfig
    {
        [Key]
        [Column("pk_config_id")]
        public long ConfigId { get; set; }

        [Required]
        [MaxLength(200)]
        [Column("config_field")]
        public string ConfigField { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        [Column("config_value")]
        public string ConfigValue { get; set; } = string.Empty;

        [MaxLength(200)]
        [Column("related_table")]
        public string? RelatedTable { get; set; }

        [Column("updated_by")]
        public long? UpdatedBy { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}