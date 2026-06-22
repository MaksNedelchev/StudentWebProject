using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentManagerWebApp.Models;

namespace StudentManagerWebApp.Pages.StudentPortal
{
    public class MessagesModel : PageModel
    {
        private readonly StudentDbContext _context;

        public MessagesModel(StudentDbContext context) => _context = context;

        public Student Student { get; set; } = new();
        public int CurrentUserId { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new();
        public List<Message> Messages { get; set; } = new();

        [BindProperty]
        [Display(Name = "Course")]
        public int CourseId { get; set; }

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
            var student = await GetCurrentStudentAsync();
            if (student == null) return NotFound();

            await LoadAsync(student);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var student = await GetCurrentStudentAsync();
            if (student?.AppUser == null) return NotFound();

            if (!ModelState.IsValid)
            {
                await LoadAsync(student);
                return Page();
            }

            var course = await _context.Courses.Include(c => c.Teacher).FirstOrDefaultAsync(c => c.Id == CourseId);
            if (course?.Teacher == null) return NotFound();

            var enrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == student.Id && e.CourseId == CourseId);
            if (!enrolled) return Forbid();

            _context.Messages.Add(new Message
            {
                SenderUserId = student.AppUser.Id,
                RecipientUserId = course.Teacher.AppUserId,
                Subject = Subject,
                Body = Body
            });

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        private async Task<Student?> GetCurrentStudentAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            return await _context.Students.Include(s => s.AppUser).FirstOrDefaultAsync(s => s.AppUserId == userId);
        }

        private async Task LoadAsync(Student student)
        {
            Student = student;
            CurrentUserId = student.AppUser!.Id;

            Enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Include(e => e.Course)
                .ThenInclude(c => c!.Teacher)
                .OrderBy(e => e.Course!.Title)
                .ToListAsync();

            CourseId = CourseId == 0 ? Enrollments.FirstOrDefault(e => e.Course?.Teacher != null)?.CourseId ?? 0 : CourseId;

            Messages = await _context.Messages
                .Where(m => m.RecipientUserId == student.AppUser.Id || m.SenderUserId == student.AppUser.Id)
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();
        }
    }
}
