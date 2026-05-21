using System.ComponentModel.DataAnnotations;

namespace ServeyApplication.Models
{
    public class EmailConfirmationToken
    {
        [Key]
        public int TokenId { get; set; }

        public int UserId { get; set; }
        public string TokenString { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }

        public Models.User User { get; set; }

    }
}
