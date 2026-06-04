using System.ComponentModel.DataAnnotations;

namespace StudentManagerWebApp.Models
{
    public class Absence
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public int CourseId { get; set; }

        [Display(Name = "Absent on")]
        public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [StringLength(300)]
        public string Notes { get; set; } = string.Empty;

        public Student? Student { get; set; }
        public Course? Course { get; set; }
    }
}
