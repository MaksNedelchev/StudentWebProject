using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentManagerWebApp.Models;

namespace StudentManagerWebApp.Pages.Students
{
    public class IndexModel : PageModel
    {
        private readonly StudentDbContext _context;
        public IndexModel(StudentDbContext context)
        {
            _context = context;
        }

        public List<Student> Students { get; set; } = new();

        public async Task OnGetAsync()
        {
            Students = await _context.Students.ToListAsync();
        }
    }
}
