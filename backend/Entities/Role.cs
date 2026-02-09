using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Entities
{
    [Table("roles")]
    public class Role
    {

        [Key]
        public int RoleId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

          public ICollection<User> Users { get; set; } = new List<User>();
    }
}
