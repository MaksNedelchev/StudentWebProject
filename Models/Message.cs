using System.ComponentModel.DataAnnotations;

namespace StudentManagerWebApp.Models
{
    public class Message
    {
        public int Id { get; set; }

        public int SenderUserId { get; set; }
        public int RecipientUserId { get; set; }

        [Required]
        [StringLength(120)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Body { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }

        public AppUser? Sender { get; set; }
        public AppUser? Recipient { get; set; }
    }
}
