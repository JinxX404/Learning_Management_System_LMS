using Microsoft.AspNetCore.Mvc;
using Learning_Management_System.Models;
using Learning_Management_System.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Learning_Management_System.Controllers
{
    public class StudentController : Controller
    {
        private readonly LmsContext _context;

        public StudentController(LmsContext context)
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

        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var enrollments = await _context.CourseEnrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .ToListAsync();

            var enrollmentCount = enrollments.Count;

            var grades = await _context.Grades
                .Where(g => g.UserId == userId)
                .ToListAsync();

            var recentNotifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(3)
                .ToListAsync();

            ViewBag.EnrollmentCount = enrollmentCount;
            ViewBag.AssignmentCount = grades.Count;
            ViewBag.Gpa = CalculateGpa(grades);
            ViewBag.Enrollments = enrollments;
            ViewBag.RecentNotifications = recentNotifications;
            ViewData["Title"] = "Student Dashboard";

            return View();
        }

        public async Task<IActionResult> MyCourses()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get enrollments with course details
            var enrollments = await _context.CourseEnrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .Include(e => e.Course)
                    .ThenInclude(c => c.AcademicTerm)
                .ToListAsync();

            ViewBag.Enrollments = enrollments;
            ViewData["Title"] = "My Courses";

            return View();
        }



        public async Task<IActionResult> CourseDetails(int id = 1)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Verify student is enrolled
            var isEnrolled = await _context.CourseEnrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == id);

            if (!isEnrolled)
                return RedirectToAction("MyCourses");

            // Get course with all details
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.AcademicTerm)
                .Include(c => c.Lectures)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            ViewBag.Course = course;
            ViewData["Title"] = course.Title;

            return View();
        }

        public async Task<IActionResult> CourseContent(int id = 1)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Verify enrollment
            var isEnrolled = await _context.CourseEnrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == id);

            if (!isEnrolled)
                return RedirectToAction("MyCourses");

            // Get course with lectures and learning assets
            var course = await _context.Courses
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.LearningAssets)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            ViewBag.Course = course;
            ViewData["Title"] = "Course Content";

            return View();
        }

        public async Task<IActionResult> Assignments()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var enrolledCourses = await _context.CourseEnrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .ToListAsync();

            ViewBag.EnrolledCourses = enrolledCourses;
            ViewData["Title"] = "Assignments";

            return View();
        }

        public async Task<IActionResult> AssignmentsSubmissions()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get grades that represent submissions (simplified)
            var submissions = await _context.Grades
                .Where(g => g.UserId == userId && g.GradableItemType == "Assignment")
                .Include(g => g.GradeBook)
                    .ThenInclude(gb => gb.Course)
                .OrderByDescending(g => g.GradedAt)
                .ToListAsync();

            ViewBag.Submissions = submissions;
            ViewData["Title"] = "Assignment Submissions";

            return View();
        }

        public IActionResult QuizPage(int id = 1)
        {
            ViewData["Title"] = "Quiz";
            ViewBag.QuizId = id;
            return View();
        }

        public IActionResult QuizResult(int id = 1)
        {
            ViewData["Title"] = "Quiz Results";
            ViewBag.QuizId = id;
            return View();
        }

        public async Task<IActionResult> Grades()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get all grades for this student
            var grades = await _context.Grades
                .Where(g => g.UserId == userId)
                .Include(g => g.GradeBook)
                    .ThenInclude(gb => gb.Course)
                .OrderByDescending(g => g.GradedAt)
                .ToListAsync();

            ViewBag.Grades = grades;
            ViewBag.Gpa = CalculateGpa(grades);
            ViewBag.TotalCredits = CalculateTotalCredits(grades);
            ViewBag.AcademicStanding = GetAcademicStanding(ViewBag.Gpa);
            ViewData["Title"] = "My Grades";

            return View();
        }

        public async Task<IActionResult> Announcements()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get notifications for this user
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            ViewBag.Notifications = notifications;
            ViewBag.UnreadCount = notifications.Count(n => !n.IsRead);
            ViewData["Title"] = "Announcements";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> MarkNotificationRead(int notificationId)
        {
            if (!IsLoggedIn())
                return Json(new { success = false, message = "Not logged in" });

            var userId = GetCurrentUserId() ?? 0;

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Notification not found" });
        }

        public async Task<IActionResult> Profile()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get user with profile
            var user = await _context.Users
                .Include(u => u.StudentProfile)
                .Include(u => u.Institution)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var enrollments = await _context.CourseEnrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                .ToListAsync();

            var profileGrades = await _context.Grades
                .Where(g => g.UserId == userId)
                .ToListAsync();

            ViewBag.Enrollments = enrollments;
            ViewBag.Gpa = CalculateGpa(profileGrades);
            ViewBag.User = user;
            ViewData["Title"] = "Profile";

            return View();
        }

        public async Task<IActionResult> AccountSettings()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            ViewBag.User = user;
            ViewData["Title"] = "Account Settings";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AccountSettings(string firstName, string lastName, string email)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Auth");

            // Update user info
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Email = email;
            user.UpdatedAt = DateTime.Now;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.SuccessMessage = "Profile updated successfully.";
            ViewBag.User = user;
            ViewData["Title"] = "Account Settings";

            return View();
        }

        private decimal CalculateGpa(IEnumerable<Grade> grades)
        {
            var scoreRatios = grades
                .Where(g => g.MaxPoints > 0)
                .Select(g => g.Points / g.MaxPoints)
                .ToList();

            if (!scoreRatios.Any())
                return 0m;

            var average = scoreRatios.Average(r => (double)r);
            return Math.Round((decimal)(average * 4), 2);
        }

        private int CalculateTotalCredits(IEnumerable<Grade> grades)
        {
            return grades
                .Where(g => g.GradeBook?.Course != null)
                .GroupBy(g => g.GradeBook!.CourseId)
                .Sum(group => group.First().GradeBook!.Course.CreditHours);
        }

        private string GetAcademicStanding(decimal gpa)
        {
            if (gpa >= 3.5m) return "Dean's List";
            if (gpa >= 3.0m) return "Good";
            if (gpa >= 2.0m) return "Satisfactory";
            return "Needs Attention";
        }
    }
}
