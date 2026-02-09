using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Entities.Games
{
    [Table("game_booking_participants")]
    public class GameBookingParticipant
    {
        [Key]
        [Column("pk_participant_id")]
        public int ParticipantId { get; set; }

        [Required]
        [Column("fk_booking_id")]
        public int BookingId { get; set; }

        [Required]
        [Column("fk_user_id")]
        public int UserId { get; set; }

        [Column("joined_at")]
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("BookingId")]
        public virtual GameBooking? Booking { get; set; }

        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
    }

}
