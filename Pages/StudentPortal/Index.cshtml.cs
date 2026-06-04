using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentManagerWebApp.Models;

namespace StudentManagerWebApp.Pages.StudentPortal
{
    public class IndexModel : PageModel
    {
        private readonly StudentDbContext _context;

        public IndexModel(StudentDbContext context) => _context = context;

        public Student Student { get; set; } = new();
        public List<Enrollment> Enrollments { get; set; } = new();
        public List<Message> Messages { get; set; } = new();
        public List<Assignment> Assignments { get; set; } = new();
        public List<AssignmentSubmission> Submissions { get; set; } = new();
        public decimal? AverageGrade { get; set; }
        public int AbsenceCount { get; set; }

        [BindProperty]
        public int AssignmentId { get; set; }

        [BindProperty]
        public string SubmissionContent { get; set; } = string.Empty;

        [BindProperty]
        public int CourseId { get; set; }

        [BindProperty]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        public string Body { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return NotFound();

            await LoadAsync(student);
            return Page();
        }

        public async Task<IActionResult> OnPostSubmitAsync()
        {
            var student = await GetCurrentStudentAsync();
            if (student == null) return NotFound();

            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == AssignmentId);
            if (assignment == null) return NotFound();

            var enrolled = await _context.Enrollments.AnyAsync(e => e.StudentId == student.Id && e.CourseId == assignment.CourseId);
            if (!enrolled) return Forbid();

            var existing = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentId == AssignmentId && s.StudentId == student.Id);

            if (existing == null)
            {
                _context.AssignmentSubmissions.Add(new AssignmentSubmission
                {
                    AssignmentId = AssignmentId,
                    StudentId = student.Id,
                    Content = SubmissionContent,
                    SubmittedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.Content = SubmissionContent;
                existing.SubmittedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMessageAsync()
        {
            var student = await GetCurrentStudentAsync();
            if (student?.AppUser == null) return NotFound();

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
            Enrollments = await _context.Enrollments
                .Where(e => e.StudentId == student.Id)
                .Include(e => e.Course)
                .ThenInclude(c => c!.Teacher)
                .ToListAsync();

            var courseIds = Enrollments.Select(e => e.CourseId).ToList();

            Assignments = await _context.Assignments
                .Where(a => courseIds.Contains(a.CourseId))
                .Include(a => a.Course)
                .OrderBy(a => a.DueDate)
                .ToListAsync();

            Submissions = await _context.AssignmentSubmissions
                .Where(s => s.StudentId == student.Id)
                .Include(s => s.Assignment)
                .ToListAsync();

            Messages = await _context.Messages
                .Where(m => m.RecipientUserId == student.AppUser!.Id)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            AverageGrade = Enrollments.Where(e => e.Grade.HasValue).Select(e => e.Grade!.Value).DefaultIfEmpty().Average();
            if (!Enrollments.Any(e => e.Grade.HasValue))
            {
                AverageGrade = null;
            }

            AbsenceCount = await _context.Absences.CountAsync(a => a.StudentId == student.Id);
        }
    }
}
