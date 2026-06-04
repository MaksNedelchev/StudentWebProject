using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentManagerWebApp.Models;
using StudentManagerWebApp.Services;

namespace StudentManagerWebApp.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly StudentDbContext _context;
        private readonly PasswordService _passwordService;

        public LoginModel(StudentDbContext context, PasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }

        [BindProperty]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        public string Password { get; set; } = string.Empty;

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            var user = await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == Email && u.IsActive);
            if (user == null || !_passwordService.VerifyPassword(Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return Page();
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.FullName),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role.ToString())
            };

            var identity = new ClaimsIdentity(claims, "StudentManagerCookie");
            await HttpContext.SignInAsync("StudentManagerCookie", new ClaimsPrincipal(identity));

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToPage("/Dashboard");
        }
    }
}
