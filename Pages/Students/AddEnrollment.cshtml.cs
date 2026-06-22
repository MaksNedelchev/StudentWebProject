using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StudentManagerWebApp.Models;

namespace StudentManagerWebApp.Pages.Students
{
    public class AddEnrollmentModel : PageModel
    {
        private readonly StudentDbContext _context;
        public AddEnrollmentModel(StudentDbContext context)
        {
            _context = context;
        }
    
        public Student Student { get; set; } = new();

        [BindProperty]
        public int StudentId { get; set; }

        [BindProperty]
        public List<int> SelectedCoursesIds { get; set; } = new();

        public List<SelectListItem> CoursesList { get; set; } = new();
        public List<Course> AllCourses { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            Student = student;
            StudentId = id;

            await LoadCoursesAsync(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var student = await _context.Students.FindAsync(StudentId);
            if (student == null) return NotFound();
            Student = student;
            await LoadCoursesAsync(StudentId);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (SelectedCoursesIds == null || SelectedCoursesIds.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Select at least one course to enroll the student.");
                return Page();
            }

            var availableCourseIds = AllCourses.Select(c => c.Id).ToHashSet();
            if (SelectedCoursesIds.Any(courseId => !availableCourseIds.Contains(courseId)))
            {
                return Forbid();
            }

            var enrollmentsToAdd = SelectedCoursesIds.Select(courseId => new Enrollment
            {
                StudentId = StudentId,
                CourseId = courseId
            }).ToList();

            _context.Enrollments.AddRange(enrollmentsToAdd);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Students/Enrollments", new { id = StudentId });
        }

        private async Task LoadCoursesAsync(int studentId)
        {
            var enrolledCourseIds = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => e.CourseId)
                .ToListAsync();

            AllCourses = await _context.Courses
                .Where(c => !enrolledCourseIds.Contains(c.Id))
                .OrderBy(c => c.Title)
                .ToListAsync();

            CoursesList = AllCourses
                .Select(c => new SelectListItem { Text = c.Title, Value = c.Id.ToString() })
                .ToList();
        }
    }
}
