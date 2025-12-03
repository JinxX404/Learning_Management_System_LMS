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

        public class ActivityItem
        {
            public string? Type { get; set; }
            public string? Message { get; set; }
            public DateTime Date { get; set; }
            public string? Icon { get; set; }
            public string? ColorClass { get; set; }
        }

        [Route("Admin/Dashboard")]
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var totalUsers = await _context.Users.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();
            var activeCourses = await _context.Courses.Where(c => c.Status == "Active").CountAsync();
            var courseEnrollments = await _context.CourseEnrollments.CountAsync();
            var aiGenerations = await _context.AigeneratedContents.CountAsync();

            var lastMonth = DateTime.Now.AddDays(-30);
            var newUsersLastMonth = await _context.Users.CountAsync(u => u.CreatedAt >= lastMonth);
            var newEnrollmentsLastMonth = await _context.CourseEnrollments.CountAsync(e => e.EnrolledAt >= lastMonth);

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalCourses = totalCourses;
            ViewBag.ActiveCourses = activeCourses;
            ViewBag.CourseEnrollments = courseEnrollments;
            ViewBag.AiGenerations = aiGenerations;
            
            ViewBag.NewUsersLastMonth = newUsersLastMonth;
            ViewBag.NewEnrollmentsLastMonth = newEnrollmentsLastMonth;


            var recentUsers = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5)
                .Select(u => new ActivityItem
                {
                    Type = "User",
                    Message = $"New user registered: {u.FirstName} {u.LastName}",
                    Date = u.CreatedAt,
                    Icon = "group_add",
                    ColorClass = "info"
                })
                .ToListAsync();

            var recentCourses = await _context.Courses
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new ActivityItem
                {
                    Type = "Course",
                    Message = $"New course published: {c.Title}",
                    Date = c.CreatedAt,
                    Icon = "add_circle",
                    ColorClass = "success"
                })
                .ToListAsync();

            var recentActivity = recentUsers.Concat(recentCourses)
                .OrderByDescending(a => a.Date)
                .Take(5)
                .ToList();

            ViewBag.RecentActivity = recentActivity;

            // 3. AI Usage (Group by ModelName)
            // Using VwAiinteractionsByUser or AigeneratedContent
            // AigeneratedContent has AimodelId, need to join with AIModels
            var aiUsage = await _context.AigeneratedContents
                .Include(c => c.Aimodel)
                .GroupBy(c => c.Aimodel.ModelName)
                .Select(g => new { Model = g.Key, Count = g.Count() })
                .ToListAsync();
            
            ViewBag.AiUsage = aiUsage;

            ViewData["Title"] = "Admin Dashboard";

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Users(string searchString)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var usersQuery = _context.Users
                .Include(u => u.Institution)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                usersQuery = usersQuery.Where(u => 
                    u.FirstName.Contains(searchString) || 
                    u.LastName.Contains(searchString) || 
                    u.Email.Contains(searchString));
            }

            var users = await usersQuery
                .OrderBy(u => u.LastName)
                .ToListAsync();

            ViewBag.Users = users;
            ViewBag.SearchString = searchString;
            ViewData["Title"] = "User Management";

            return View();
        }

        [HttpGet]
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

        [HttpGet]
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

        [HttpGet]
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

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName))
            {
                ViewBag.Error = "Full Name, Email and Password are required.";
                ViewBag.Institutions = await _context.Institutions.ToListAsync();
                return View("AddStudent");
            }

            var names = fullName.Trim().Split(' ');
            var firstName = names[0];
            var lastName = names.Length > 1 ? names[names.Length - 1] : "";

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

        [HttpGet]
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

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(fullName))
            {
                ViewBag.Error = "Full Name, Email and Password are required.";
                ViewBag.Institutions = await _context.Institutions.ToListAsync();
                return View("AddInstructor");
            }

            var names = fullName.Trim().Split(' ');
            var firstName = names[0];
            var lastName = names.Length > 1 ? names[names.Length - 1] : "";

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

        [HttpGet]
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
        [ValidateAntiForgeryToken]
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

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {

                var institutions = await _context.Institutions.ToListAsync();
                ViewBag.Institutions = institutions;
                ViewBag.User = user;
                ViewBag.Error = "Email, First Name, and Last Name are required.";
                
                if (role == "Student")
                {
                    ViewData["Title"] = "Edit Student";
                    return View("EditStudent");
                }
                else if (role == "Instructor")
                {
                    ViewData["Title"] = "Edit Instructor";
                    return View("EditInstructor");
                }
                
                ViewData["Title"] = "Edit User";
                return View();
            }

            user.Email = email.Trim();
            user.FirstName = firstName.Trim();
            user.LastName = lastName.Trim();
            user.Role = role;
            user.IsActive = isActive;
            user.UpdatedAt = DateTime.Now;

            if (!string.IsNullOrEmpty(password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            if (role == "Student")
                return RedirectToAction("Students");
            else if (role == "Instructor")
                return RedirectToAction("Instructors");
            else
                return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Student")
                return NotFound();

            // Check if student has active enrollments
            var hasEnrollments = await _context.CourseEnrollments
                .AnyAsync(e => e.UserId == id && e.Status == "Enrolled");

            if (hasEnrollments)
            {
                // Student still has active enrollments → do NOT deactivate
                TempData["Error"] = "This student cannot be deleted because they still have active enrollments.";
                return RedirectToAction("Students"); 
            }
            
            // Can safely soft delete
            user.IsActive = false;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            

            return RedirectToAction("Students");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteInstructor(int id)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(id);
            if (user == null || user.Role != "Instructor")
                return NotFound();

            // Check if instructor has active courses
            var hasCourses = await _context.Courses
                .AnyAsync(c => c.InstructorId == id && c.Status == "Active");

            // if (hasCourses)
            // {
            //     // Cannot delete instructor with active courses
            //     user.IsActive = false;
            //     user.UpdatedAt = DateTime.Now;
            //     await _context.SaveChangesAsync();
            // }

            if (hasCourses)
            {
                // Instructor still has active courses → do NOT deactivate
                TempData["Error"] = "This instructor cannot be deleted because they still have active courses.";
                return RedirectToAction("Instructors"); 
            }
            
            // Can safely soft delete
            user.IsActive = false;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return RedirectToAction("Instructors");
        }

        [HttpGet]
        public async Task<IActionResult> StudentDetails(int id)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var student = await _context.Users
                .Include(u => u.StudentProfile)
                .Include(u => u.Institution)
                .FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Student");

            if (student == null)
                return NotFound();

            // Get enrollments
            var enrollments = await _context.CourseEnrollments
                .Where(e => e.UserId == id)
                .Include(e => e.Course)
                .ThenInclude(c => c.AcademicTerm)
                .Include(e => e.Course)
                .ThenInclude(c => c.Instructor)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            ViewBag.Student = student;
            ViewBag.Enrollments = enrollments;
            ViewData["Title"] = $"{student.FirstName} {student.LastName} - Details";

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> InstructorDetails(int id)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var instructor = await _context.Users
                .Include(u => u.InstructorProfile)
                .Include(u => u.Institution)
                .FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Instructor");

            if (instructor == null)
                return NotFound();

            // Get courses taught
            var courses = await _context.Courses
                .Where(c => c.InstructorId == id)
                .Include(c => c.AcademicTerm)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Instructor = instructor;
            ViewBag.Courses = courses;
            ViewData["Title"] = $"{instructor.FirstName} {instructor.LastName} - Details";

            return View();
        }

        [HttpGet]
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

        [HttpGet]
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

        [HttpGet]
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
        [ValidateAntiForgeryToken]
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

            // Validate term name
            if (string.IsNullOrWhiteSpace(termName))
            {
                ViewBag.Error = "Term Name is required.";
                var institutions = await _context.Institutions.ToListAsync();
                ViewBag.Institutions = institutions;
                ViewBag.Term = term;
                return View("EditTerm");
            }

            // Validate dates
            if (endDate <= startDate)
            {
                ViewBag.Error = "End date must be after start date.";
                var institutions = await _context.Institutions.ToListAsync();
                ViewBag.Institutions = institutions;
                ViewBag.Term = term;
                return View("EditTerm");
            }

            term.TermName = termName.Trim();
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
                // Term still has courses → do NOT delete
                TempData["Error"] = "This term cannot be deleted because it still has courses.";
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
                // Cannot delete yourself, just redirect back
                return RedirectToAction("Dashboard");
            }

            // Store role before soft delete
            var userRole = user.Role;

            // Soft delete - set IsActive to false instead of deleting
            user.IsActive = false;
            user.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            // Redirect based on role
            if (userRole == "Student")
                return RedirectToAction("Students");
            else if (userRole == "Instructor")
                return RedirectToAction("Instructors");
            else
                return RedirectToAction("Dashboard");
        }

        [HttpGet]
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

            // 1. Top Performing Students
            var topStudents = await _context.VwTopPerformingStudents
                .OrderByDescending(s => s.Gpa)
                .Take(10)
                .ToListAsync();
            ViewBag.TopStudents = topStudents;

            // 2. Course Pass/Fail Rates
            var courseRates = await _context.VwCoursePassFailRates
                .Join(_context.Courses,
                    rate => rate.CourseId,
                    course => course.CourseId,
                    (rate, course) => new {
                        CourseTitle = course.Title,
                        CourseCode = course.CourseCode,
                        PassRate = rate.PassRate,
                        PassCount = rate.PassCount,
                        FailCount = rate.FailCount,
                        Total = rate.TotalCompletedStudents
                    })
                .OrderByDescending(x => x.PassRate)
                .ToListAsync();
            ViewBag.CourseRates = courseRates;

            // 3. Institution Summary
            var institutionSummary = await _context.VwInstitutionSummaries
                .OrderByDescending(i => i.UserCount)
                .ToListAsync();
            ViewBag.InstitutionSummary = institutionSummary;

            ViewData["Title"] = "Reports";
            ViewData["AdminActive"] = "reports";

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            ViewData["AdminActive"] = "settings";
            
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Login", "Auth");

            ViewBag.User = user;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return RedirectToAction("Login", "Auth");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                ViewBag.ErrorMessage = "Incorrect current password.";
                ViewBag.User = user;
                ViewData["AdminActive"] = "settings";
                return View("Settings");
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "New password and confirmation do not match.";
                ViewBag.User = user;
                ViewData["AdminActive"] = "settings";
                return View("Settings");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            ViewBag.SuccessMessage = "Password changed successfully.";
            ViewBag.User = user;
            ViewData["AdminActive"] = "settings";
            return View("Settings");
        }

        // Course Management
        [HttpGet]
        public async Task<IActionResult> Courses()
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.AcademicTerm)
                .Where(c => c.Status != "Inactive")
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewData["Title"] = "Course Management";
            ViewData["AdminActive"] = "courses";

            return View("CourseManagement");
        }



        [HttpGet]
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

        [HttpGet]
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

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound();

            // Check if course has enrollments
            var hasEnrollments = await _context.CourseEnrollments
                .AnyAsync(e => e.CourseId == id);

            if (hasEnrollments)
            {
                // Cannot delete course with enrollments, set to inactive
                course.Status = "Archived";
                _context.Courses.Update(course);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Can safely delete
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Courses");
        }

        [HttpGet]
        public async Task<IActionResult> CourseDetails(int id)
        {
            if (!IsLoggedIn() || !await IsAdmin())
                return RedirectToAction("Login", "Auth");

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.AcademicTerm)
                .ThenInclude(t => t.Institution)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            // Get enrollments
            var enrollments = await _context.CourseEnrollments
                .Where(e => e.CourseId == id)
                .Include(e => e.User)
                .OrderBy(e => e.User.LastName)
                .ToListAsync();

            ViewBag.Course = course;
            ViewBag.Enrollments = enrollments;
            ViewData["Title"] = $"{course.Title} - Details";

            return View();
        }

        // Enrollment Management

        [HttpGet]
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

