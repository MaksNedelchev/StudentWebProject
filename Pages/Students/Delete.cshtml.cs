using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentManagerWebApp.Models;

namespace StudentManagerWebApp.Pages.Students
{
    public class DeleteModel : PageModel
    {
        private readonly StudentDbContext _context;
        public DeleteModel(StudentDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Student Student { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();
            Student = student;
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var student = await _context.Students.FindAsync(Student.Id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("Index");
        }
    }
}
