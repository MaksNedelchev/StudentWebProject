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
            // Use GroupJoin so the count/average are computed server-side in a single query per EF Core provider
            Courses = await _context.Courses
                .GroupJoin(
                    _context.Enrollments,
                    course => course.Id,
                    enrollment => enrollment.CourseId,
                    (course, enrollments) => new CourseStats
                    {
                        Id = course.Id,
                        Title = course.Title,
                        Hours = course.Hours,
                        StudentCount = enrollments.Count(),
                        AverageGrade = enrollments.Any() ? enrollments.Average(e => (decimal?)e.Grade) : null
                    })
                .ToListAsync();
        }

        public class CourseStats
        {
            public int Id { get; set; }
            public string Title { get; set; } = string.Empty;
            public int Hours { get; set; }
            public int StudentCount { get; set; }
            public decimal? AverageGrade { get; set; }
        }
    }
}