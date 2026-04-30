using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentManagerWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace StudentManagerWebApp.Pages.Students
{
    public class CreateModel : PageModel
    {
        private readonly StudentDbContext _context;
        public CreateModel(StudentDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Student Student { get; set; } = new();

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            _context.Students.Add(Student);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
