using System.ComponentModel.DataAnnotations;

namespace StudentManagerWebApp.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        [Required]
        [StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Due date")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);

        public Course? Course { get; set; }
        public List<AssignmentSubmission> Submissions { get; set; } = new();
    }
}
