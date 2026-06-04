using System.ComponentModel.DataAnnotations;

namespace StudentManagerWebApp.Models
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }
        public int StudentId { get; set; }

        [Required]
        [StringLength(2000)]
        [Display(Name = "Submission")]
        public string Content { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        [Range(2.00, 6.00)]
        public decimal? Grade { get; set; }

        [StringLength(1000)]
        public string Feedback { get; set; } = string.Empty;

        public Assignment? Assignment { get; set; }
        public Student? Student { get; set; }
    }
}
