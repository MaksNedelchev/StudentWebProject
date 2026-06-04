using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using StudentManagerWebApp.Models;
using StudentManagerWebApp.Services;
using System.ComponentModel.DataAnnotations;

namespace StudentManagerWebApp.Pages.Teachers
{
    public class CreateModel : PageModel
    {
        private readonly StudentDbContext _context;
        private readonly PasswordService _passwordService;

        public CreateModel(StudentDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [BindProperty]
        public Teacher Teacher { get; set; } = new();

        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            Teacher.AppUser = new AppUser
            {
                Email = Email,
                FullName = $"{Teacher.FirstName} {Teacher.LastName}",
                Role = UserRole.Teacher,
                PasswordHash = _passwordService.HashPassword(Password)
            };

            _context.Teachers.Add(Teacher);
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
