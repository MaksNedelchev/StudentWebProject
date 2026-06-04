using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentManagerWebApp.Models;

namespace StudentManagerWebApp.Pages.Courses
{
    public class IndexModel : PageModel
    {
        private readonly StudentDbContext _context;
        public IndexModel(StudentDbContext context) => _context = context;

        public List<CourseStats> Courses { get; set; } = new();

        public async Task OnGetAsync()
        {
            Courses = await _context.Courses
                .Select(course => new CourseStats
                {
                    Id = course.Id,
                    Title = course.Title,
                    Hours = course.Hours,
                    TeacherName = course.Teacher == null ? "Unassigned" : course.Teacher.FirstName + " " + course.Teacher.LastName,
                    StudentCount = course.Enrollments.Count,
                    AverageGrade = course.Enrollments.Any(e => e.Grade.HasValue)
                        ? course.Enrollments.Where(e => e.Grade.HasValue).Average(e => e.Grade)
                        : null
                })
                .ToListAsync();
        }

        public class CourseStats
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public int Hours { get; set; }
            public string TeacherName { get; set; } = string.Empty;
            public int StudentCount { get; set; }
            public decimal? AverageGrade { get; set; }
        }
    }
}
