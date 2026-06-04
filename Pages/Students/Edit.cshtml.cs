using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentManagerWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace StudentManagerWebApp.Pages.Students
{
    public class EditModel : PageModel
    {
        private readonly StudentDbContext _context;
        public EditModel(StudentDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Student Student { get; set; } = new();

        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var student = await _context.Students.Include(s => s.AppUser).FirstOrDefaultAsync(s => s.Id == id);
            if (student == null) return NotFound();
            Student = student;
            Email = student.AppUser?.Email ?? string.Empty;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var student = await _context.Students.Include(s => s.AppUser).FirstOrDefaultAsync(s => s.Id == Student.Id);
            if (student == null) return NotFound();

            student.FirstName = Student.FirstName;
            student.LastName = Student.LastName;
            student.EnrollmentDate = Student.EnrollmentDate;
            if (student.AppUser != null)
            {
                student.AppUser.Email = Email;
                student.AppUser.FullName = $"{Student.FirstName} {Student.LastName}";
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
