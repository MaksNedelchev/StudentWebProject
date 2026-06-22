using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using StudentManagerWebApp.Models;

namespace StudentManagerWebApp.Pages.TeacherPortal
{
    public class AttendanceModel : PageModel
    {
        private readonly StudentDbContext _context;

        public AttendanceModel(StudentDbContext context) => _context = context;

        public Teacher Teacher { get; set; } = new();
        public List<Course> Courses { get; set; } = new();
        public List<Enrollment> Enrollments { get; set; } = new();
        public HashSet<int> AbsentStudentIds { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        [Display(Name = "Subject")]
        public int? SelectedCourseId { get; set; }

        [BindProperty(SupportsGet = true)]
        [Display(Name = "Date")]
        public DateOnly SelectedDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [BindProperty]
        public List<int> PostedAbsentStudentIds { get; set; } = new();

        [BindProperty]
        public List<StudentGradeInput> Grades { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? courseId, DateOnly? date)
        {
            SelectedCourseId = courseId;
            SelectedDate = date ?? DateOnly.FromDateTime(DateTime.Today);

            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound();

            await LoadAsync(teacher);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var teacher = await GetCurrentTeacherAsync();
            if (teacher == null) return NotFound();
            if (SelectedCourseId == null) return NotFound();
            if (!await OwnsCourseAsync(teacher.Id, SelectedCourseId.Value)) return Forbid();

            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == SelectedCourseId.Value)
                .ToListAsync();
            var enrolledStudentIds = enrollments.Select(e => e.StudentId).ToHashSet();

            foreach (var grade in Grades.Where(g => enrolledStudentIds.Contains(g.StudentId)))
            {
                var enrollment = enrollments.FirstOrDefault(e => e.StudentId == grade.StudentId);
                if (enrollment != null)
                {
                    enrollment.Grade = grade.Grade;
                }
            }

            var postedAbsentIds = PostedAbsentStudentIds.Where(enrolledStudentIds.Contains).ToHashSet();
            var existingAbsences = await _context.Absences
                .Where(a => a.CourseId == SelectedCourseId.Value && a.Date == SelectedDate)
                .ToListAsync();

            foreach (var absence in existingAbsences.Where(a => !postedAbsentIds.Contains(a.StudentId)))
            {
                _context.Absences.Remove(absence);
            }

            var existingAbsentIds = existingAbsences.Select(a => a.StudentId).ToHashSet();
            foreach (var studentId in postedAbsentIds.Where(id => !existingAbsentIds.Contains(id)))
            {
                _context.Absences.Add(new Absence
                {
                    CourseId = SelectedCourseId.Value,
                    StudentId = studentId,
                    Date = SelectedDate
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { courseId = SelectedCourseId.Value, date = SelectedDate.ToString("yyyy-MM-dd") });
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

        private async Task LoadAsync(Teacher teacher)
        {
            Teacher = teacher;
            Courses = await _context.Courses
                .Where(c => c.TeacherId == teacher.Id)
                .OrderBy(c => c.Title)
                .ToListAsync();

            SelectedCourseId ??= Courses.FirstOrDefault()?.Id;
            if (SelectedCourseId == null)
            {
                return;
            }

            if (!Courses.Any(c => c.Id == SelectedCourseId.Value))
            {
                SelectedCourseId = Courses.FirstOrDefault()?.Id;
            }

            if (SelectedCourseId == null)
            {
                return;
            }

            Enrollments = await _context.Enrollments
                .Where(e => e.CourseId == SelectedCourseId.Value)
                .Include(e => e.Student)
                .OrderBy(e => e.Student!.LastName)
                .ThenBy(e => e.Student!.FirstName)
                .ToListAsync();

            var absentStudentIds = await _context.Absences
                .Where(a => a.CourseId == SelectedCourseId.Value && a.Date == SelectedDate)
                .Select(a => a.StudentId)
                .ToListAsync();
            AbsentStudentIds = absentStudentIds.ToHashSet();
        }

        public class StudentGradeInput
        {
            public int StudentId { get; set; }

            [Range(2.00, 6.00)]
            public decimal? Grade { get; set; }
        }
    }
}
