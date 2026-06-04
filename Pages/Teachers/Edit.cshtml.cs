using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentManagerWebApp.Models;
using System.ComponentModel.DataAnnotations;

namespace StudentManagerWebApp.Pages.Teachers
{
    public class EditModel : PageModel
    {
        private readonly StudentDbContext _context;

        public EditModel(StudentDbContext context) => _context = context;

        [BindProperty]
        public Teacher Teacher { get; set; } = new();

        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var teacher = await _context.Teachers.Include(t => t.AppUser).FirstOrDefaultAsync(t => t.Id == id);
            if (teacher == null) return NotFound();
            Teacher = teacher;
            Email = teacher.AppUser?.Email ?? string.Empty;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var teacher = await _context.Teachers.Include(t => t.AppUser).FirstOrDefaultAsync(t => t.Id == Teacher.Id);
            if (teacher == null) return NotFound();

            teacher.FirstName = Teacher.FirstName;
            teacher.LastName = Teacher.LastName;
            teacher.Department = Teacher.Department;
            if (teacher.AppUser != null)
            {
                teacher.AppUser.Email = Email;
                teacher.AppUser.FullName = $"{Teacher.FirstName} {Teacher.LastName}";
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
