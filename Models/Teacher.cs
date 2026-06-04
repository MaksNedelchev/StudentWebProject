using System.ComponentModel.DataAnnotations;

namespace StudentManagerWebApp.Models
{
    public class Teacher
    {
        public int Id { get; set; }

        public int AppUserId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        public string Department { get; set; } = string.Empty;

        public AppUser? AppUser { get; set; }
        public List<Course> Courses { get; set; } = new();
    }
}
