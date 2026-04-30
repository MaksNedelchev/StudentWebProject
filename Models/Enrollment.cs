using System.ComponentModel.DataAnnotations;

namespace StudentManagerWebApp.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public int CourseId { get; set; }

        [Range(2.00, 6.00)]
        public decimal Grade { get; set; }

        public Course? Course { get; set; }
        public Student? Student { get; set; }
    }
}
