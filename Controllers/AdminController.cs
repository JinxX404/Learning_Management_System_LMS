using Learning_Management_System.Models;
using Learning_Management_System.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Learning_Management_System.Controllers
{
    public class AdminController : Controller
    {
        private readonly LmsContext _context;

        public AdminController(LmsContext context)
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

        private async Task<bool> IsAdmin()
        {
            if (!IsLoggedIn()) return false;
            var user = await _context.Users.FindAsync(GetCurrentUserId());
            return user?.Role == "Admin";
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var totalUsers = await _context.Users.CountAsync();
            var totalStudents = await _context.Users
                .Where(u => u.Role == "Student")
                .CountAsync();
            var totalCourses = await _context.Courses.CountAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalCourses = totalCourses;
            ViewData["Title"] = "Admin Dashboard";

            return View();
        }

        public async Task<IActionResult> Users()
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var users = await _context.Users
                .Include(u => u.Institution)
                .OrderBy(u => u.LastName)
                .ToListAsync();

            ViewBag.Users = users;
            ViewData["Title"] = "User Management";

            return View();
        }

        public async Task<IActionResult> Students()
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var students = await _context.Users
                .Where(u => u.Role == "Student")
                .Include(u => u.Institution)
                .OrderBy(u => u.LastName)
                .ToListAsync();

            ViewBag.Students = students;
            ViewData["Title"] = "Student Management";

            return View("StudentManagement");
        }

        public async Task<IActionResult> Instructors()
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var instructors = await _context.Users
                .Where(u => u.Role == "Instructor")
                .Include(u => u.Institution)
                .OrderBy(u => u.LastName)
                .ToListAsync();

            ViewBag.Instructors = instructors;
            ViewData["Title"] = "Instructor Management";

            return View("InstructorManagement");
        }

        public async Task<IActionResult> CreateStudent()
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var institutions = await _context.Institutions.ToListAsync();
            ViewBag.Institutions = institutions;
            ViewData["Title"] = "Add New Student";

            return View("AddStudent");
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent(
            string fullName,
            string email,
            string password,
            string status,
            int institutionId)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            // Basic validation
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName))
            {
                ViewBag.Error = "Full Name, Email and Password are required.";
                ViewBag.Institutions = await _context.Institutions.ToListAsync();
                return View("AddStudent");
            }

            // Split name
            var names = fullName.Trim().Split(' ');
            var firstName = names[0];
            var lastName = names.Length > 1 ? names[names.Length - 1] : "";

            // Check email
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Email already exists.";
                ViewBag.Institutions = await _context.Institutions.ToListAsync();
                return View("AddStudent");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var isActive = status?.ToLower() == "active";

            var user = new User
            {
                Email = email,
                PasswordHash = hashedPassword,
                FirstName = firstName,
                LastName = lastName,
                Role = "Student",
                InstitutionId = institutionId > 0 ? institutionId : 1,
                IsActive = isActive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var profile = new StudentProfile
            {
                UserId = user.UserId,
                StudentIdNumber = $"STU{user.UserId:0000}",
                AdmissionDate = DateOnly.FromDateTime(DateTime.Now)
            };
            _context.StudentProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction("Students");
        }

        public async Task<IActionResult> CreateInstructor()
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var institutions = await _context.Institutions.ToListAsync();
            ViewBag.Institutions = institutions;
            ViewData["Title"] = "Add New Instructor";

            return View("AddInstructor");
        }

        [HttpPost]
        public async Task<IActionResult> CreateInstructor(
            string fullName,
            string email,
            string password,
            string status,
            string department,
            string biography,
            int institutionId)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

             // Basic validation
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName))
            {
                ViewBag.Error = "Full Name, Email and Password are required.";
                ViewBag.Institutions = await _context.Institutions.ToListAsync();
                return View("AddInstructor");
            }

            // Split name
            var names = fullName.Trim().Split(' ');
            var firstName = names[0];
            var lastName = names.Length > 1 ? names[names.Length - 1] : "";

            // Check email
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Email already exists.";
                ViewBag.Institutions = await _context.Institutions.ToListAsync();
                return View("AddInstructor");
            }

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            var isActive = status?.ToLower() == "active";

            var user = new User
            {
                Email = email,
                PasswordHash = hashedPassword,
                FirstName = firstName,
                LastName = lastName,
                Role = "Instructor",
                InstitutionId = institutionId > 0 ? institutionId : 1,
                IsActive = isActive,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var profile = new InstructorProfile
            {
                UserId = user.UserId,
                Department = department,
                Bio = biography
            };
            _context.InstructorProfiles.Add(profile);
            await _context.SaveChangesAsync();

            return RedirectToAction("Instructors");
        }

        public async Task<IActionResult> EditUser(int id)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            var institutions = await _context.Institutions.ToListAsync();
            ViewBag.Institutions = institutions;
            ViewBag.User = user;

            if (user.Role == "Student")
            {
                ViewData["Title"] = "Edit Student";
                return View("EditStudent");
            }
            else if (user.Role == "Instructor")
            {
                ViewData["Title"] = "Edit Instructor";
                return View("EditInstructor");
            }

            ViewData["Title"] = "Edit User";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(
            int userId,
            string email,
            string firstName,
            string lastName,
            string role,
            string password,
            bool isActive)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            // Update properties
            user.Email = email;
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Role = role;
            user.IsActive = isActive;
            user.UpdatedAt = DateTime.Now;

            // If password provided, update it
            if (!string.IsNullOrEmpty(password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Users");
        }

        public async Task<IActionResult> AcademicTerms()
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var terms = await _context.AcademicTerms
                .Include(t => t.Institution)
                .OrderByDescending(t => t.StartDate)
                .ToListAsync();

            ViewBag.Terms = terms;
            var institutions = await _context.Institutions.ToListAsync();
            ViewBag.Institutions = institutions;
            ViewData["Title"] = "Academic Terms";

            return View("TermManagement");
        }

        public async Task<IActionResult> CreateAcademicTerm()
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var institutions = await _context.Institutions.ToListAsync();
            ViewBag.Institutions = institutions;
            ViewData["Title"] = "Add New Term";

            return View("AddTerm");
        }

        [HttpPost]
        public async Task<IActionResult> CreateAcademicTerm(
            string termName,
            DateTime startDate,
            DateTime endDate,
            int institutionId)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            // Validate dates
            if (endDate <= startDate)
            {
                ViewBag.Error = "End date must be after start date.";
                var terms = await _context.AcademicTerms
                    .Include(t => t.Institution)
                    .OrderByDescending(t => t.StartDate)
                    .ToListAsync();
                ViewBag.Terms = terms;
                var institutions = await _context.Institutions.ToListAsync();
                ViewBag.Institutions = institutions;
                return View("AddTerm");
            }

            // Create term
            var term = new AcademicTerm
            {
                TermName = termName,
                StartDate = DateOnly.FromDateTime(startDate),
                EndDate = DateOnly.FromDateTime(endDate),
                InstitutionId = institutionId,
                IsActive = true
            };

            _context.AcademicTerms.Add(term);
            await _context.SaveChangesAsync();

            return RedirectToAction("AcademicTerms");
        }

        public async Task<IActionResult> EditAcademicTerm(int id)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var term = await _context.AcademicTerms.FindAsync(id);
            if (term == null)
                return NotFound();

            var institutions = await _context.Institutions.ToListAsync();
            ViewBag.Institutions = institutions;
            ViewBag.Term = term;
            ViewData["Title"] = "Edit Term";

            return View("EditTerm");
        }

        [HttpPost]
        public async Task<IActionResult> EditAcademicTerm(
            int academicTermId,
            string termName,
            DateTime startDate,
            DateTime endDate,
            int institutionId,
            bool isActive)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var term = await _context.AcademicTerms.FindAsync(academicTermId);
            if (term == null)
                return NotFound();

            // Validate dates
            if (endDate <= startDate)
            {
                ViewBag.Error = "End date must be after start date.";
                var institutions = await _context.Institutions.ToListAsync();
                ViewBag.Institutions = institutions;
                ViewBag.Term = term;
                return View("EditTerm");
            }

            term.TermName = termName;
            term.StartDate = DateOnly.FromDateTime(startDate);
            term.EndDate = DateOnly.FromDateTime(endDate);
            term.InstitutionId = institutionId;
            term.IsActive = isActive;

            _context.AcademicTerms.Update(term);
            await _context.SaveChangesAsync();

            return RedirectToAction("AcademicTerms");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAcademicTerm(int id)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var term = await _context.AcademicTerms.FindAsync(id);
            if (term == null)
                return NotFound();

            // Check if term has courses
            var hasCourses = await _context.Courses.AnyAsync(c => c.AcademicTermId == id);
            if (hasCourses)
            {
                // Cannot delete, maybe archive? For now just show error
                // Since this is a POST redirect, we can't easily show error in view without TempData or similar
                // For now, let's just not delete and redirect.
                return RedirectToAction("AcademicTerms");
            }

            _context.AcademicTerms.Remove(term);
            await _context.SaveChangesAsync();

            return RedirectToAction("AcademicTerms");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Don't allow deleting yourself
            var currentUserId = GetCurrentUserId();
            if (user.UserId == currentUserId)
            {
                ViewBag.Error = "You cannot delete your own account.";
                var users = await _context.Users
                    .Include(u => u.Institution)
                    .OrderBy(u => u.LastName)
                    .ToListAsync();
                ViewBag.Users = users;
                return View("Users");
            }

            // Soft delete - set IsActive to false instead of deleting
            user.IsActive = false;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Reports()
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            // Basic statistics for reports
            var totalUsers = await _context.Users.CountAsync();
            var totalStudents = await _context.Users.Where(u => u.Role == "Student").CountAsync();
            var totalInstructors = await _context.Users.Where(u => u.Role == "Instructor").CountAsync();
            var totalCourses = await _context.Courses.CountAsync();
            var totalEnrollments = await _context.CourseEnrollments.CountAsync();
            var activeTerms = await _context.AcademicTerms.Where(t => t.IsActive).CountAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalInstructors = totalInstructors;
            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalEnrollments = totalEnrollments;
            ViewBag.ActiveTerms = activeTerms;
            ViewData["Title"] = "Reports";

            return View();
        }

        // Course Management
        public async Task<IActionResult> Courses()
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.AcademicTerm)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewData["Title"] = "Course Management";

            return View("CourseManagement");
        }

        public async Task<IActionResult> CreateCourse()
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var instructors = await _context.Users
                .Where(u => u.Role == "Instructor")
                .ToListAsync();

            var terms = await _context.AcademicTerms
                .Where(t => t.IsActive == true)
                .ToListAsync();

            ViewBag.Instructors = instructors;
            ViewBag.Terms = terms;
            ViewData["Title"] = "Create Course";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(
            string courseCode,
            string title,
            string description,
            int instructorId,
            int academicTermId,
            int creditHours,
            int maxEnrollment)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            // Validate
            if (string.IsNullOrEmpty(courseCode) || string.IsNullOrEmpty(title))
            {
                ViewBag.Error = "Course code and title are required.";
                // Reload dropdowns
                ViewBag.Instructors = await _context.Users.Where(u => u.Role == "Instructor").ToListAsync();
                ViewBag.Terms = await _context.AcademicTerms.Where(t => t.IsActive == true).ToListAsync();
                return View();
            }

            var course = new Course
            {
                CourseCode = courseCode,
                Title = title,
                Description = description,
                InstructorId = instructorId,
                AcademicTermId = academicTermId,
                CreditHours = creditHours,
                MaxEnrollment = maxEnrollment,
                CurrentEnrollment = 0,
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("Courses");
        }

        public async Task<IActionResult> EditCourse(int id)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            var instructors = await _context.Users
                .Where(u => u.Role == "Instructor")
                .ToListAsync();

            var terms = await _context.AcademicTerms
                .Where(t => t.IsActive == true)
                .ToListAsync();

            ViewBag.Course = course;
            ViewBag.Instructors = instructors;
            ViewBag.Terms = terms;
            ViewData["Title"] = "Edit Course";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditCourse(
            int courseId,
            string courseCode,
            string title,
            string description,
            int instructorId,
            int academicTermId,
            int creditHours,
            int maxEnrollment,
            string status)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
                return NotFound();

            course.CourseCode = courseCode;
            course.Title = title;
            course.Description = description;
            course.InstructorId = instructorId;
            course.AcademicTermId = academicTermId;
            course.CreditHours = creditHours;
            course.MaxEnrollment = maxEnrollment;
            course.Status = status;

            _context.Courses.Update(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("Courses");
        }

        // Enrollment Management
        public async Task<IActionResult> Enrollments(int? courseId)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var courses = await _context.Courses.OrderBy(c => c.Title).ToListAsync();
            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;

            if (courseId.HasValue)
            {
                var enrollments = await _context.CourseEnrollments
                    .Where(e => e.CourseId == courseId)
                    .Include(e => e.User)
                    .Include(e => e.Course)
                    .ToListAsync();
                ViewBag.Enrollments = enrollments;

                // Get students not enrolled in this course for the dropdown
                var enrolledUserIds = enrollments.Select(e => e.UserId).ToList();
                var availableStudents = await _context.Users
                    .Where(u => u.Role == "Student" && !enrolledUserIds.Contains(u.UserId))
                    .OrderBy(u => u.LastName)
                    .ToListAsync();
                ViewBag.AvailableStudents = availableStudents;
            }

            ViewData["Title"] = "Manage Enrollments";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EnrollStudent(int courseId, int studentId)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return NotFound();

            // Check if already enrolled
            var exists = await _context.CourseEnrollments
                .AnyAsync(e => e.CourseId == courseId && e.UserId == studentId);
            
            if (!exists)
            {
                var enrollment = new CourseEnrollment
                {
                    CourseId = courseId,
                    UserId = studentId,
                    EnrolledAt = DateTime.Now,
                    Status = "Enrolled"
                };

                _context.CourseEnrollments.Add(enrollment);
                
                // Update course count
                course.CurrentEnrollment++;
                
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Enrollments", new { courseId = courseId });
        }

        [HttpPost]
        public async Task<IActionResult> DropStudent(int enrollmentId)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var enrollment = await _context.CourseEnrollments
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);

            if (enrollment != null)
            {
                var courseId = enrollment.CourseId;
                var course = enrollment.Course;

                _context.CourseEnrollments.Remove(enrollment);
                
                // Update course count
                if (course.CurrentEnrollment > 0)
                    course.CurrentEnrollment--;

                await _context.SaveChangesAsync();
                
                return RedirectToAction("Enrollments", new { courseId = courseId });
            }

            return RedirectToAction("Enrollments");
        }
}
}

