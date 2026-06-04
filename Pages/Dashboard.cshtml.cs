using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace StudentManagerWebApp.Pages
{
    public class DashboardModel : PageModel
    {
        public IActionResult OnGet()
        {
            if (User.IsInRole("Student"))
            {
                return RedirectToPage("/StudentPortal/Index");
            }

            if (User.IsInRole("Teacher"))
            {
                return RedirectToPage("/TeacherPortal/Index");
            }

            if (User.IsInRole("Admin"))
            {
                return Page();
            }

            return RedirectToPage("/Account/Login");
        }
    }
}
