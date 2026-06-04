using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
            var student = await _context.Students.Include(s => s.AppUser).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound();
            Student = student;
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var student = await _context.Students.Include(s => s.AppUser).FirstOrDefaultAsync(s => s.Id == Student.Id);
            if (student != null)
            {
                if (student.AppUser != null)
                {
                    _context.AppUsers.Remove(student.AppUser);
                }
                else
                {
                    _context.Students.Remove(student);
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("Index");
        }
    }
}
