using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentManagerWebApp.Models;

namespace StudentManagerWebApp.Pages.Teachers
{
    public class IndexModel : PageModel
    {
        private readonly StudentDbContext _context;

        public IndexModel(StudentDbContext context) => _context = context;

        public List<Teacher> Teachers { get; set; } = new();

        public async Task OnGetAsync()
        {
            Teachers = await _context.Teachers.Include(t => t.AppUser).Include(t => t.Courses).ToListAsync();
        }
    }
}
