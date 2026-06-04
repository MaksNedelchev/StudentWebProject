using System.ComponentModel.DataAnnotations;

namespace StudentManagerWebApp.Models
{
    public class AppUser
    {
        public int Id { get; set; }

        [Required]
        [StringLength(120)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;

        public Student? StudentProfile { get; set; }
        public Teacher? TeacherProfile { get; set; }
    }
}
