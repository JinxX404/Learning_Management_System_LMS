using Microsoft.AspNetCore.Mvc;

namespace Learning_Management_System.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Dashboard()
        {
            ViewData["Title"] = "Student Dashboard";
            return View();
        }

        public IActionResult MyCourses()
        {
            ViewData["Title"] = "My Courses";
            return View();
        }

        public IActionResult CourseDetails(int id = 1)
        {
            ViewData["Title"] = "Course Details";
            ViewBag.CourseId = id;
            return View();
        }

        public IActionResult CourseContent(int id = 1)
        {
            ViewData["Title"] = "Course Content";
            ViewBag.CourseId = id;
            return View();
        }

        public IActionResult Assignments()
        {
            ViewData["Title"] = "Assignments";
            return View();
        }

        public IActionResult AssignmentsSubmissions()
        {
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

        public IActionResult Grades()
        {
            ViewData["Title"] = "Grades";
            return View();
        }

        public IActionResult Announcements()
        {
            ViewData["Title"] = "Announcements";
            return View();
        }

        public IActionResult Profile()
        {
            ViewData["Title"] = "Profile";
            return View();
        }

        public IActionResult AccountSettings()
        {
            ViewData["Title"] = "Account Settings";
            return View();
        }
    }
}
