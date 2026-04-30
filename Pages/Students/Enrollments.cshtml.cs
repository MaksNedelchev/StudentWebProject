using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentManagerWebApp.Models;

namespace StudentManagerWebApp.Pages.Students
{
    public class EnrollmentsModel : PageModel
    {
        private readonly StudentDbContext _context;

        public EnrollmentsModel(StudentDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Student Student { get; set; } = new();

        public List<Enrollment> Enrollments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            Student = student;
            Enrollments = await _context.Enrollments.Where(p => p.StudentId == student.Id).Include(e => e.Course).ToListAsync();
            return Page();
        }
    }
}
