using System.ComponentModel.DataAnnotations;

namespace StudentManagerWebApp.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Display(Name = "Teacher")]
        public int? TeacherId { get; set; }

        [Required(ErrorMessage = "Enter course title")]
        [StringLength(50)]
        [Display(Name = "Title")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage ="Enter course duration")]
        [Display(Name ="Duration")]
        public int Hours { get; set; }

        public Teacher? Teacher { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new();
        public List<Assignment> Assignments { get; set; } = new();
        public List<Absence> Absences { get; set; } = new();

    }
}
