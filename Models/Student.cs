using System.ComponentModel.DataAnnotations;

namespace StudentManagerWebApp.Models
{
    public class Student
    {
        public int Id { get; set; }

        public int? AppUserId { get; set; }

        [Required(ErrorMessage = "Enter student's first name")]
        [StringLength(50)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter student's last name")]
        [StringLength(50)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enter date")]
        [Display(Name = "Enrollment Date")]
        public DateOnly EnrollmentDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public AppUser? AppUser { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new();
        public List<AssignmentSubmission> AssignmentSubmissions { get; set; } = new();
        public List<Absence> Absences { get; set; } = new();
    }
}
