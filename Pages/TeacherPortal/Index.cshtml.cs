using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentManagerWebApp.Models;

namespace StudentManagerWebApp.Pages.TeacherPortal
{
    public class IndexModel : PageModel
    {
        private readonly StudentDbContext _context;

        public IndexModel(StudentDbContext context) => _context = context;

        public Teacher Teacher { get; set; } = new();
        public List<Course> Courses { get; set; } = new();
        public int? SelectedCourseId { get; set; }
        public Course? SelectedCourse { get; set; }
        public List<Enrollment> Enrollments { get; set; } = new();
        public List<Assignment> Assignments { get; set; } = new();
        public List<AssignmentSubmission> Submissions { get; set; } = new();
        public List<Message> Messages { get; set; } = new();

        [BindProperty]
        public int CourseId { get; set; }

        [BindProperty]
        public string AssignmentTitle { get; set; } = string.Empty;

        [BindProperty]
        public string AssignmentDescription { get; set; } = string.Empty;

        [BindProperty]
        public DateTime AssignmentDueDate { get; set; } = DateTime.Today.AddDays(7);

        [BindProperty]
        public int StudentId { get; set; }

        [BindProperty]
        public decimal? Grade { get; set; }

        [BindProperty]
        public DateOnly AbsenceDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [BindProperty]
        public string AbsenceNotes { get; set; } = string.Empty;

        [BindProperty]
        public int SubmissionId { get; set; }

        [BindProperty]
        public string Feedback { get; set; } = string.Empty;

        [BindProperty]
        public string Subject { get; set; } = string.Empty;

        [BindProperty]
        public string Body { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? courseId)
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound();

            await LoadAsync(teacher, courseId);
            return Page();
        }

        public async Task<IActionResult> OnPostAssignmentAsync()
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound();
            if (!await OwnsCourseAsync(teacher.Id, CourseId)) return Forbid();

            _context.Assignments.Add(new Assignment
            {
                CourseId = CourseId,
                Title = AssignmentTitle,
                Description = AssignmentDescription,
                DueDate = AssignmentDueDate
            });
            await _context.SaveChangesAsync();
            return RedirectToPage(new { courseId = CourseId });
        }

        public async Task<IActionResult> OnPostGradeAsync()
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound();
            if (!await OwnsCourseAsync(teacher.Id, CourseId)) return Forbid();

            var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.CourseId == CourseId && e.StudentId == StudentId);
            if (enrollment == null) return NotFound();
            enrollment.Grade = Grade;
            await _context.SaveChangesAsync();
            return RedirectToPage(new { courseId = CourseId });
        }

        public async Task<IActionResult> OnPostAbsenceAsync()
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound();
            if (!await OwnsCourseAsync(teacher.Id, CourseId)) return Forbid();

            var enrolled = await _context.Enrollments.AnyAsync(e => e.CourseId == CourseId && e.StudentId == StudentId);
            if (!enrolled) return Forbid();

            _context.Absences.Add(new Absence
            {
                CourseId = CourseId,
                StudentId = StudentId,
                Date = AbsenceDate,
                Notes = AbsenceNotes
            });
            await _context.SaveChangesAsync();
            return RedirectToPage(new { courseId = CourseId });
        }

        public async Task<IActionResult> OnPostSubmissionGradeAsync()
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound();

            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.Id == SubmissionId);
            if (submission?.Assignment == null) return NotFound();
            if (!await OwnsCourseAsync(teacher.Id, submission.Assignment.CourseId)) return Forbid();

            submission.Grade = Grade;
            submission.Feedback = Feedback;
            await _context.SaveChangesAsync();
            return RedirectToPage(new { courseId = submission.Assignment.CourseId });
        }

        public async Task<IActionResult> OnPostMessageAsync()
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound();
            if (!await OwnsCourseAsync(teacher.Id, CourseId)) return Forbid();

            var student = await _context.Students.Include(s => s.AppUser).FirstOrDefaultAsync(s => s.Id == StudentId);
            if (student?.AppUser == null) return NotFound();

            var enrolled = await _context.Enrollments.AnyAsync(e => e.CourseId == CourseId && e.StudentId == StudentId);
            if (!enrolled) return Forbid();

            _context.Messages.Add(new Message
            {
                SenderUserId = teacher.AppUserId,
                RecipientUserId = student.AppUser.Id,
                Subject = Subject,
                Body = Body
            });
            await _context.SaveChangesAsync();
            return RedirectToPage(new { courseId = CourseId });
        }

        private async Task<Teacher?> GetCurrentTeacherAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            return await _context.Teachers.Include(t => t.AppUser).FirstOrDefaultAsync(t => t.AppUserId == userId);
        }

        private async Task<bool> OwnsCourseAsync(int teacherId, int courseId)
        {
            return await _context.Courses.AnyAsync(c => c.Id == courseId && c.TeacherId == teacherId);
        }

        private async Task LoadAsync(Teacher teacher, int? courseId)
        {
            Teacher = teacher;
            Courses = await _context.Courses.Where(c => c.TeacherId == teacher.Id).OrderBy(c => c.Title).ToListAsync();
            SelectedCourseId = courseId ?? Courses.FirstOrDefault()?.Id;
            if (SelectedCourseId == null)
            {
                return;
            }

            SelectedCourse = Courses.FirstOrDefault(c => c.Id == SelectedCourseId.Value);
            Enrollments = await _context.Enrollments
                .Where(e => e.CourseId == SelectedCourseId.Value)
                .Include(e => e.Student)
                .ThenInclude(s => s!.AppUser)
                .ToListAsync();

            Assignments = await _context.Assignments
                .Where(a => a.CourseId == SelectedCourseId.Value)
                .OrderByDescending(a => a.DueDate)
                .ToListAsync();

            var assignmentIds = Assignments.Select(a => a.Id).ToList();
            Submissions = await _context.AssignmentSubmissions
                .Where(s => assignmentIds.Contains(s.AssignmentId))
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();

            Messages = await _context.Messages
                .Where(m => m.RecipientUserId == teacher.AppUserId || m.SenderUserId == teacher.AppUserId)
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .OrderByDescending(m => m.SentAt)
                .Take(30)
                .ToListAsync();
        }
    }
}
