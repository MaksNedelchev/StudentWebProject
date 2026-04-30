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

            // Value should be course id so we can bind selected ids
            CoursesList = await _context.Courses
                .Select(c => new SelectListItem { Text = c.Title, Value = c.Id.ToString() })
                .ToListAsync();

            AllCourses = await _context.Courses.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var student = await _context.Students.FindAsync(StudentId);
            if (student == null) return NotFound();
            Student = student;
            CoursesList = await _context.Courses
                .Select(c => new SelectListItem { Text = c.Title, Value = c.Id.ToString() })
                .ToListAsync();
            AllCourses = await _context.Courses.ToListAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (SelectedCoursesIds == null || SelectedCoursesIds.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Select at least one course to enroll the student.");
                return Page();
            }

            var enrollmentsToAdd = new List<Enrollment>();

            foreach (var courseId in SelectedCoursesIds)
            {
                var formKey = $"Grades_{courseId}";
                var gradeValue = Request.Form[formKey].ToString();

                if (string.IsNullOrWhiteSpace(gradeValue)
                    || (!decimal.TryParse(gradeValue, out var grade)
                        && !decimal.TryParse(gradeValue, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out grade)))
                {
                    ModelState.AddModelError(string.Empty, $"Invalid grade for course id {courseId}.");
                    return Page();
                }
                if (grade < 2.00m || grade > 6.00m)
                {
                    ModelState.AddModelError(string.Empty, $"Grade for course id {courseId} must be between 2.00 and 6.00.");
                    return Page();
                }

                enrollmentsToAdd.Add(new Enrollment
                {
                    StudentId = StudentId,
                    CourseId = courseId,
                    Grade = grade
                });
            }

            _context.Enrollments.AddRange(enrollmentsToAdd);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Students/Enrollments", new { id = StudentId });
        }
    }
}
