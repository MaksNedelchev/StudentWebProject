using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagerWebApp.Models;

namespace StudentManagerWebApp.Pages.Courses
{
    public class CreateModel : PageModel
    {
        private readonly StudentDbContext _context;
        public CreateModel(StudentDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Course Course { get; set; } = new();

        public List<SelectListItem> TeacherList { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadTeachersAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadTeachersAsync();
                return Page();
            }

            _context.Courses.Add(Course);
            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        private async Task LoadTeachersAsync()
        {
            TeacherList = await _context.Teachers
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"{t.FirstName} {t.LastName}"
                })
                .ToListAsync();
        }
    }
}
