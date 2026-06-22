using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentManagerWebApp.Models;

namespace StudentManagerWebApp.Pages.TeacherPortal
{
    public class MessagesModel : PageModel
    {
        private readonly StudentDbContext _context;

        public MessagesModel(StudentDbContext context) => _context = context;

        public Teacher Teacher { get; set; } = new();
        public int CurrentUserId { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new();
        public List<Message> Messages { get; set; } = new();

        [BindProperty]
        [Display(Name = "Student")]
        public int EnrollmentId { get; set; }

        [BindProperty]
        [Required]
        [StringLength(120)]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        [StringLength(2000)]
        public string Body { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound();

            await LoadAsync(teacher);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadAsync(teacher);
                return Page();
            }

            var enrollment = await _context.Enrollments
                .Include(e => e.Course)
                .Include(e => e.Student)
                .ThenInclude(s => s!.AppUser)
                .FirstOrDefaultAsync(e => e.Id == EnrollmentId);

            if (enrollment?.Course?.TeacherId != teacher.Id) return Forbid();
            if (enrollment.Student?.AppUser == null) return NotFound();

            _context.Messages.Add(new Message
            {
                SenderUserId = teacher.AppUserId,
                RecipientUserId = enrollment.Student.AppUser.Id,
                Subject = Subject,
                Body = Body
            });

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        private async Task<Teacher?> GetCurrentTeacherAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            return await _context.Teachers.Include(t => t.AppUser).FirstOrDefaultAsync(t => t.AppUserId == userId);
        }

        private async Task LoadAsync(Teacher teacher)
        {
            Teacher = teacher;
            CurrentUserId = teacher.AppUserId;

            Enrollments = await _context.Enrollments
                .Where(e => e.Course!.TeacherId == teacher.Id)
                .Include(e => e.Course)
                .Include(e => e.Student)
                .ThenInclude(s => s!.AppUser)
                .OrderBy(e => e.Course!.Title)
                .ThenBy(e => e.Student!.LastName)
                .ThenBy(e => e.Student!.FirstName)
                .ToListAsync();

            EnrollmentId = EnrollmentId == 0 ? Enrollments.FirstOrDefault()?.Id ?? 0 : EnrollmentId;

            Messages = await _context.Messages
                .Where(m => m.RecipientUserId == teacher.AppUserId || m.SenderUserId == teacher.AppUserId)
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }
    }
}
