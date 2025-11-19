using Learning_Management_System.Models;
using Learning_Management_System.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Learning_Management_System.Controllers
{
    public class InstructorController : Controller
    {
        private readonly LmsContext _context;

        public InstructorController(LmsContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            return SessionHelper.GetUserId(HttpContext.Session);
        }

        private bool IsLoggedIn()
        {
            return GetCurrentUserId() != null;
        }

        private async Task<bool> IsInstructor()
        {
            if (!IsLoggedIn()) return false;
            var user = await _context.Users.FindAsync(GetCurrentUserId());
            return user?.Role == "Instructor";
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Count courses taught
            var coursesCount = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .CountAsync();

            // Count pending grades (simplified)
            var pendingGradesCount = 0;

            ViewBag.CoursesCount = coursesCount;
            ViewBag.PendingGradesCount = pendingGradesCount;
            ViewData["Title"] = "Instructor Dashboard";

            return View();
        }

        public async Task<IActionResult> MyCourses()
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .Include(c => c.AcademicTerm)
                .Include(c => c.CourseEnrollments)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewData["Title"] = "My Courses";

            return View();
        }

        public async Task<IActionResult> CreateCourse()
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            // Get academic terms for dropdown
            var terms = await _context.AcademicTerms
                .Where(t => t.IsActive == true)
                .ToListAsync();

            ViewBag.AcademicTerms = terms;
            ViewData["Title"] = "Create Course";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(
            string courseCode,
            string title,
            string description,
            int academicTermId,
            int creditHours)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            // Validate
            if (string.IsNullOrEmpty(courseCode) || string.IsNullOrEmpty(title))
            {
                ViewBag.Error = "Course code and title are required.";
                var terms = await _context.AcademicTerms
                    .Where(t => t.IsActive == true)
                    .ToListAsync();
                ViewBag.AcademicTerms = terms;
                return View();
            }

            var userId = GetCurrentUserId() ?? 0;

            // Create course
            var course = new Course
            {
                CourseCode = courseCode,
                Title = title,
                Description = description,
                InstructorId = userId,
                AcademicTermId = academicTermId,
                CreditHours = creditHours,
                MaxEnrollment = 50,
                CurrentEnrollment = 0,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyCourses");
        }

        public async Task<IActionResult> CourseDetails(int id)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Verify ownership
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == id && c.InstructorId == userId);

            if (course == null)
                return NotFound();

            // Get course with lectures
            course = await _context.Courses
                .Include(c => c.Lectures)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            ViewBag.Course = course;
            ViewData["Title"] = course.Title;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddLecture(int courseId, string title, string description)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Verify ownership
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.InstructorId == userId);

            if (course == null)
                return NotFound();

            // Create lecture
            var lecture = new Lecture
            {
                CourseId = courseId,
                Title = title,
                Description = description
            };

            _context.Lectures.Add(lecture);
            await _context.SaveChangesAsync();

            return RedirectToAction("CourseDetails", new { id = courseId });
        }

        public async Task<IActionResult> GradeSubmissions()
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get instructor's course IDs
            var courseIds = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .Select(c => c.CourseId)
                .ToListAsync();

            // Get ungraded items (grades with 0 points - simplified check)
            var ungraded = await _context.Grades
                .Where(g => courseIds.Contains(g.GradeBook.CourseId) && g.Points == 0)
                .Include(g => g.User)
                .Include(g => g.GradeBook)
                    .ThenInclude(gb => gb.Course)
                .ToListAsync();

            ViewBag.Ungraded = ungraded;
            ViewData["Title"] = "Grade Submissions";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GradeSubmission(int gradeId, decimal points, string feedback)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var grade = await _context.Grades.FindAsync(gradeId);
            if (grade == null)
                return NotFound();

            // Update grade
            grade.Points = points;
            grade.Comments = feedback;
            grade.GradedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction("GradeSubmissions");
        }

        public async Task<IActionResult> CourseRoster(int id)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Verify ownership
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == id && c.InstructorId == userId);

            if (course == null)
                return NotFound();

            // Get enrolled students
            var enrollments = await _context.CourseEnrollments
                .Where(e => e.CourseId == id)
                .Include(e => e.User)
                .ToListAsync();

            ViewBag.Course = course;
            ViewBag.Enrollments = enrollments;
            ViewData["Title"] = "Course Roster";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAnnouncement(int courseId, string message, bool isImportant = false)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Verify ownership
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.InstructorId == userId);

            if (course == null)
                return NotFound();

            // Get all enrolled students
            var enrolledUserIds = await _context.CourseEnrollments
                .Where(e => e.CourseId == courseId)
                .Select(e => e.UserId)
                .ToListAsync();

            // Create notifications for all enrolled students
            var notifications = enrolledUserIds.Select(uid => new Notification
            {
                UserId = uid,
                Message = $"[{course.Title}] {message}",
                IsRead = false,
                CreatedAt = DateTime.Now
            }).ToList();

            _context.Notifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            return RedirectToAction("CourseDetails", new { id = courseId });
        }
    }
}

