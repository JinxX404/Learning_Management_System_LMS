using Learning_Management_System.Models;
using Learning_Management_System.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Learning_Management_System.Controllers
{
    using Learning_Management_System.Models.ViewModels;

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

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Count courses taught
            var coursesCount = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .CountAsync();

            // Count quizzes (non-deleted)
            var quizzesCount = await _context.Quizzes
                .Include(q => q.Course)
                .Where(q => q.Course.InstructorId == userId && !q.IsDeleted)
                .CountAsync();

            // Count pending grades (simplified)
            var pendingGradesCount = 0;

            ViewBag.CoursesCount = coursesCount;
            ViewBag.QuizzesCount = quizzesCount;
            ViewBag.PendingGradesCount = pendingGradesCount;
            ViewData["Title"] = "Instructor Dashboard";

            return View();
        }

        [HttpGet]
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

            // Also provide academic terms for the create form dropdown
            var terms = await _context.AcademicTerms
                .Where(t => t.IsActive == true)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.AcademicTerms = terms;
            ViewData["Title"] = "My Courses";

            return View();
        }

        [HttpGet]
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(
            string courseCode,
            string title,
            string description,
            int academicTermId,
            int creditHours)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            // Basic validation
            if (string.IsNullOrWhiteSpace(courseCode) || string.IsNullOrWhiteSpace(title))
            {
                ViewBag.Error = "Course code and title are required.";
                var terms = await _context.AcademicTerms
                    .Where(t => t.IsActive == true)
                    .ToListAsync();
                ViewBag.AcademicTerms = terms;
                return View("MyCourses");
            }

            var userId = GetCurrentUserId() ?? 0;

            // If academicTermId wasn't provided, try to pick a default
            if (academicTermId <= 0)
            {
                var defaultTerm = await _context.AcademicTerms.FirstOrDefaultAsync(t => t.IsActive == true);
                academicTermId = defaultTerm?.AcademicTermId ?? 0;
            }

            // Ensure creditHours has a reasonable default
            if (creditHours <= 0) creditHours = 3;

            try
            {
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
                var saved = await _context.SaveChangesAsync();

                if (saved <= 0)
                {
                    ViewBag.Error = "Failed to save the course to the database.";
                    var terms = await _context.AcademicTerms
                        .Where(t => t.IsActive == true)
                        .ToListAsync();
                    ViewBag.AcademicTerms = terms;
                    return View("MyCourses");
                }

                TempData["Success"] = "Course created successfully.";
                return RedirectToAction("MyCourses");
            }
            catch (Exception ex)
            {
                // surface the error for debugging
                ViewBag.Error = "Error saving course: " + ex.Message;
                var terms = await _context.AcademicTerms
                    .Where(t => t.IsActive == true)
                    .ToListAsync();
                ViewBag.AcademicTerms = terms;
                return View("MyCourses");
            }
        }

        [HttpGet]
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
                    .ThenInclude(l => l.LearningAssets)
                .Include(c => c.Quizzes)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            ViewBag.Course = course;
            ViewData["Title"] = course.Title;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var user = await _context.Users
                .Include(u => u.InstructorProfile)
                .Include(u => u.Institution)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null) return RedirectToAction("Login", "Auth");

            var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.Courses = courses;
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

        [HttpGet]
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

        [HttpGet]
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

        // صفحة عرض الإعلانات للمدرس - GET
        [HttpGet]
        public async Task<IActionResult> Announcements()
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // جلب كل الكورسات الخاصة بالمدرس مع التسجيلات
            var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .Include(c => c.CourseEnrollments)
                .ToListAsync();

            // جلب معرفات الطلاب المسجلين في كورسات المدرس
            var courseIds = courses.Select(c => c.CourseId).ToList();
            
            var enrolledStudentIds = await _context.CourseEnrollments
                .Where(e => courseIds.Contains(e.CourseId))
                .Select(e => e.UserId)
                .Distinct()
                .ToListAsync();

            // جلب آخر الإعلانات المرسلة للطلاب المسجلين في كورسات المدرس
            var recentNotifications = await _context.Notifications
                .Where(n => enrolledStudentIds.Contains(n.UserId))
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewBag.RecentNotifications = recentNotifications;
            ViewData["Title"] = "Manage Announcements";

            return View();
        }
        
        //=======================================================================
        //===================إرسال إعلان جديد - POST=============================
        //=======================================================================
        [HttpPost]
        public async Task<IActionResult> SendAnnouncement(int courseId, string message)
        {
            try
            {
                if (!IsLoggedIn() || !await IsInstructor())
                    return Json(new { success = false, message = "Not authorized" });

                if (string.IsNullOrWhiteSpace(message))
                    return Json(new { success = false, message = "Message cannot be empty" });

                var userId = GetCurrentUserId() ?? 0;

                // التحقق من ملكية الكورس
                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseId == courseId && c.InstructorId == userId);

                if (course == null)
                    return Json(new { success = false, message = "Course not found" });

                // جلب معرفات كل الطلاب المسجلين
                var enrolledUserIds = await _context.CourseEnrollments
                    .Where(e => e.CourseId == courseId)
                    .Select(e => e.UserId)
                    .ToListAsync();

                int notificationCount = 0;
                var notificationsList = new List<Notification>();

                // إذا كان هناك طلاب مسجلين، أرسل لهم الإعلانات
                if (enrolledUserIds.Any())
                {
                    // إنشاء إشعار لكل طالب مسجل
                    foreach (var studentId in enrolledUserIds)
                    {
                        var notification = new Notification
                        {
                            UserId = studentId,
                            Message = $"[{course.Title}] {message}",
                            IsRead = false,
                            CreatedAt = DateTime.Now
                        };
                        notificationsList.Add(notification);
                    }

                    _context.Notifications.AddRange(notificationsList);
                    notificationCount = notificationsList.Count;
                }
                else
                {
                    // حتى لو مافيش طلاب، نسجل الإعلان للمدرس نفسه كـ "سجل"
                    var instructorNotification = new Notification
                    {
                        UserId = userId, // نسجله باسم المدرس
                        Message = $"[{course.Title}] {message}",
                        IsRead = true, // نخليه مقروء لأنه للمدرس
                        CreatedAt = DateTime.Now
                    };

                    _context.Notifications.Add(instructorNotification);
                    notificationCount = 0;
                }

                // حفظ التغييرات في قاعدة البيانات
                var savedCount = await _context.SaveChangesAsync();

                // التحقق من أن البيانات تم حفظها
                if (savedCount == 0)
                {
                    return Json(new { 
                        success = false, 
                        message = "Failed to save announcement to database" 
                    });
                }

                string responseMessage = enrolledUserIds.Any() 
                    ? $"Announcement sent successfully to {notificationCount} student(s) and saved to database"
                    : $"Announcement saved successfully (No students enrolled yet)";

                return Json(new { 
                    success = true, 
                    message = responseMessage,
                    count = notificationCount,
                    hasStudents = enrolledUserIds.Any(),
                    savedToDb = true,
                    savedRecords = savedCount
                });
            }
            catch (Exception ex)
            {
                // Log the error
                return Json(new { 
                    success = false, 
                    message = $"Error: {ex.Message}",
                    details = ex.InnerException?.Message
                });
            }
        }

        //=======================================================================
        // صفحة عرض الدورات المتاحة للمستخدم
        [HttpGet]
        public async Task<IActionResult> MyEnrolledCourses()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            int userId = GetCurrentUserId() ?? 0;

            // جلب الكورسات التي تم تسجيل المستخدم فيها
            var enrollments = await _context.CourseEnrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                .ToListAsync();

            return View(enrollments);
        }

        // GET: عرض نموذج تسجيل في كورس
        [HttpGet]
        public async Task<IActionResult> Enroll(int courseId)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var course = await _context.Courses
                .Include(c => c.AcademicTerm)
                .FirstOrDefaultAsync(c => c.CourseId == courseId);

            if (course == null)
                return NotFound();

            ViewBag.Course = course;
            return View();
        }

        // POST: تسجيل في كورس
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId, string status = "Enrolled")
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            int userId = GetCurrentUserId() ?? 0;

            // التأكد أن المستخدم لم يسجل من قبل
            bool alreadyEnrolled = await _context.CourseEnrollments
                .AnyAsync(e => e.CourseId == courseId && e.UserId == userId);

            if (alreadyEnrolled)
            {
                TempData["Error"] = "You are already enrolled in this course.";
                return RedirectToAction("MyCourses");
            }

            var enrollment = new CourseEnrollment
            {
                UserId = userId,
                CourseId = courseId,
                EnrolledAt = DateTime.Now,
                Status = status
            };

            _context.CourseEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Successfully enrolled in the course.";
            return RedirectToAction("MyCourses");
        }

        // GET: Instructor/CreateAssignment
        [HttpGet]
        public async Task<IActionResult> CreateAssignment(int? courseId)
        {
            // Only allow logged-in instructors
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            // Get instructor's courses for dropdown
            var userId = GetCurrentUserId() ?? 0;
            var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .ToListAsync();
            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;
            return View();
        }

        // POST: Instructor/CreateAssignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment(int courseId, string title, string description, DateTime dueDate, int maxPoints, string assignmentType, bool allowLateSubmission, int latePenalty, string allowedFileTypes, int maxFileSize, string externalLinks)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            // TODO: Save assignment to DB (add your logic here)
            // For now, just redirect to CourseDetails
            return RedirectToAction("CourseDetails", new { id = courseId });
        }

        // GET: Instructor/CreateQuiz
        // FUTURE: To integrate AI-powered quiz generation:
        // 1. Create a new action, e.g., GenerateQuizFromAI(int courseId, string topic)
        // 2. Call your AI service to generate a CreateQuizViewModel
        // 3. Return View("CreateQuiz", generatedViewModel) to pre-fill this form.
        [HttpGet]
        public async Task<IActionResult> CreateQuiz(int? courseId)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;
            var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .ToListAsync();
            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;
            return View();
        }

        // POST: Instructor/CreateQuiz
        // POST: Instructor/CreateQuiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuiz(CreateQuizViewModel model)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                var userId = GetCurrentUserId() ?? 0;
                ViewBag.Courses = await _context.Courses.Where(c => c.InstructorId == userId).ToListAsync();
                return View(model);
            }

            try 
            {
                // Map ViewModel to Entity
                var quiz = new Quiz
                {
                    CourseId = model.CourseId,
                    Title = model.Title,
                    Description = model.Description,
                    DueDate = model.DueDate,
                    TimeLimitMinutes = model.TimeLimitMinutes
                };

                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync(); // Save to get QuizId

                foreach (var qVm in model.Questions)
                {
                    var question = new QuizQuestion
                    {
                        QuizId = quiz.QuizId,
                        QuestionText = qVm.QuestionText,
                        QuestionType = qVm.QuestionType,
                        Points = qVm.Points
                    };

                    _context.QuizQuestions.Add(question);
                    await _context.SaveChangesAsync(); // Save to get QuestionId

                    var options = new List<QuestionOption>();

                    if (qVm.QuestionType == "MultipleChoice")
                    {
                        foreach (var optVm in qVm.Options)
                        {
                            if (!string.IsNullOrWhiteSpace(optVm.OptionText))
                            {
                                options.Add(new QuestionOption
                                {
                                    QuestionId = question.QuestionId,
                                    OptionText = optVm.OptionText,
                                    IsCorrect = optVm.IsCorrect
                                });
                            }
                        }
                    }
                    else if (qVm.QuestionType == "TrueFalse")
                    {
                        bool isTrueCorrect = qVm.CorrectAnswerText?.ToLower() == "true";
                        options.Add(new QuestionOption { QuestionId = question.QuestionId, OptionText = "True", IsCorrect = isTrueCorrect });
                        options.Add(new QuestionOption { QuestionId = question.QuestionId, OptionText = "False", IsCorrect = !isTrueCorrect });
                    }
                    else if (qVm.QuestionType == "ShortAnswer")
                    {
                        // Save the correct answer as an option for reference/grading
                        if (!string.IsNullOrWhiteSpace(qVm.CorrectAnswerText))
                        {
                            options.Add(new QuestionOption
                            {
                                QuestionId = question.QuestionId,
                                OptionText = qVm.CorrectAnswerText,
                                IsCorrect = true
                            });
                        }
                    }

                    if (options.Any())
                    {
                        _context.QuestionOptions.AddRange(options);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Quiz '{quiz.Title}' created successfully!";
                return RedirectToAction("MyQuizzes");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating quiz: " + ex.Message);
                var userId = GetCurrentUserId() ?? 0;
                ViewBag.Courses = await _context.Courses.Where(c => c.InstructorId == userId).ToListAsync();
                return View(model);
            }
        }

        // GET: Instructor/UploadMaterials
        [HttpGet]
        public async Task<IActionResult> UploadMaterials(int? courseId)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;
            var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .ToListAsync();
            ViewBag.Courses = courses;
            ViewBag.SelectedCourseId = courseId;
            return View();
        }

        // POST: Instructor/UploadMaterials
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadMaterials(/* add upload parameters here */)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            // TODO: Save uploaded materials to DB (add your logic here)
            // For now, just redirect to Dashboard
            return RedirectToAction("Dashboard");
        }
        // GET: Instructor/AddLearningAsset
        [HttpGet]
        public async Task<IActionResult> AddLearningAsset(int? lectureId, int? courseId)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var model = new LearningAsset();

            ViewBag.LectureId = lectureId;
            ViewBag.LectureTitle = string.Empty;

            int returnCourseId = courseId ?? 0;

            if (lectureId.HasValue && lectureId.Value > 0)
            {
                var lecture = await _context.Lectures
                    .Include(l => l.Course)
                    .FirstOrDefaultAsync(l => l.LectureId == lectureId.Value);

                if (lecture == null || lecture.Course.InstructorId != userId)
                    return NotFound();

                ViewBag.LectureTitle = lecture.Title;
                ViewBag.LectureId = lecture.LectureId;
                returnCourseId = lecture.CourseId;
                model.LectureId = lecture.LectureId;
            }

            ViewBag.CourseId = returnCourseId;

            return View(model);
        }

        // POST: AddLearningAsset (updated to require antiforgery)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLearningAsset(int? courseId, int? lectureId, string title, string assetType, string fileUrl, string description)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            Lecture? lecture = null;

            // If lectureId provided, load it and verify ownership
            if (lectureId.HasValue && lectureId.Value > 0)
            {
                lecture = await _context.Lectures
                    .Include(l => l.Course)
                    .FirstOrDefaultAsync(l => l.LectureId == lectureId.Value);

                if (lecture == null || lecture.Course.InstructorId != userId)
                    return NotFound();
            }
            else if (courseId.HasValue && courseId.Value > 0)
            {
                // Verify course ownership
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId.Value && c.InstructorId == userId);
                if (course == null)
                    return NotFound();

                // Try to find an existing lecture with same title for this course
                if (!string.IsNullOrWhiteSpace(title))
                {
                    lecture = await _context.Lectures.FirstOrDefaultAsync(l => l.CourseId == courseId.Value && l.Title == title);
                }

                // If no lecture found, create one
                if (lecture == null)
                {
                    lecture = new Lecture
                    {
                        CourseId = courseId.Value,
                        Title = title ?? "New Lecture",
                        Description = description
                    };

                    _context.Lectures.Add(lecture);
                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                return BadRequest("LectureId or CourseId must be provided.");
            }

            // Create the learning asset
            var asset = new LearningAsset
            {
                LectureId = lecture.LectureId,
                Title = title ?? "Untitled",
                AssetType = string.IsNullOrWhiteSpace(assetType) ? "link" : assetType,
                FileUrl = fileUrl ?? string.Empty
            };

            _context.LearningAssets.Add(asset);
            await _context.SaveChangesAsync();

            return RedirectToAction("CourseDetails", new { id = lecture.CourseId });
        }

        // GET: Instructor/EditLecture/5
        [HttpGet]
        public async Task<IActionResult> EditLecture(int id)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var lecture = await _context.Lectures
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LectureId == id);

            if (lecture == null || lecture.Course.InstructorId != userId)
                return NotFound();

            return View(lecture);
        }

        // POST: Instructor/EditLecture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLecture(int lectureId, string title, string description)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var lecture = await _context.Lectures
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.LectureId == lectureId);

            if (lecture == null || lecture.Course.InstructorId != userId)
                return NotFound();

            lecture.Title = title;
            lecture.Description = description;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Lecture updated successfully.";
            return RedirectToAction("CourseDetails", new { id = lecture.CourseId });
        }

        // POST: Instructor/DeleteLecture
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLecture(int lectureId)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var lecture = await _context.Lectures
                .Include(l => l.Course)
                .Include(l => l.LearningAssets)
                .FirstOrDefaultAsync(l => l.LectureId == lectureId);

            if (lecture == null || lecture.Course.InstructorId != userId)
                return NotFound();

            // Hard delete for now as requested
            _context.LearningAssets.RemoveRange(lecture.LearningAssets);
            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Lecture deleted successfully.";
            return RedirectToAction("CourseDetails", new { id = lecture.CourseId });
        }

        // GET: Instructor/EditLearningAsset/5
        [HttpGet]
        public async Task<IActionResult> EditLearningAsset(int id)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var asset = await _context.LearningAssets
                .Include(a => a.Lecture)
                .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(a => a.AssetId == id);

            if (asset == null || asset.Lecture.Course.InstructorId != userId)
                return NotFound();

            ViewBag.LectureTitle = asset.Lecture.Title;
            ViewBag.LectureId = asset.LectureId;
            ViewBag.CourseId = asset.Lecture.CourseId;

            return View(asset);
        }

        // POST: Instructor/EditLearningAsset
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLearningAsset(int assetId, string title, string assetType, string fileUrl)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var asset = await _context.LearningAssets
                .Include(a => a.Lecture)
                .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(a => a.AssetId == assetId);

            if (asset == null || asset.Lecture.Course.InstructorId != userId)
                return NotFound();

            asset.Title = title;
            asset.AssetType = assetType;
            asset.FileUrl = fileUrl;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Learning asset updated successfully.";
            return RedirectToAction("CourseDetails", new { id = asset.Lecture.CourseId });
        }

        // POST: Instructor/DeleteLearningAsset
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLearningAsset(int assetId)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var asset = await _context.LearningAssets
                .Include(a => a.Lecture)
                .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(a => a.AssetId == assetId);

            if (asset == null || asset.Lecture.Course.InstructorId != userId)
                return NotFound();

            var courseId = asset.Lecture.CourseId;

            _context.LearningAssets.Remove(asset);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Learning asset deleted successfully.";
            return RedirectToAction("CourseDetails", new { id = courseId });
        }

        
        // GET: Instructor/MyQuizzes - List all quizzes
        [HttpGet]
        public async Task<IActionResult> MyQuizzes()
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get all quizzes for instructor's courses (excluding deleted)
            var quizzes = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.QuizQuestions)
                .Include(q => q.QuizAttempts)
                .Where(q => q.Course.InstructorId == userId && !q.IsDeleted)
                .OrderByDescending(q => q.QuizId)
                .ToListAsync();

            ViewData["Title"] = "My Quizzes";
            return View(quizzes);
        }

        // GET: Instructor/QuizDetails/5 - View quiz details with statistics
        [HttpGet]
        public async Task<IActionResult> QuizDetails(int id)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get quiz with all related data
            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuestionOptions)
                .Include(q => q.QuizAttempts)
                    .ThenInclude(qa => qa.User)
                .FirstOrDefaultAsync(q => q.QuizId == id && q.Course.InstructorId == userId && !q.IsDeleted);

            if (quiz == null)
                return NotFound();

            // Calculate statistics
            var attempts = quiz.QuizAttempts.ToList();
            ViewBag.TotalAttempts = attempts.Count;
            ViewBag.AverageScore = attempts.Any() ? attempts.Average(a => a.Score ?? 0) : 0;
            ViewBag.CompletionRate = attempts.Any() ? 
                attempts.Count(a => a.SubmittedAt.HasValue) * 100.0 / attempts.Count : 0;

            ViewData["Title"] = $"Quiz Details - {quiz.Title}";
            return View(quiz);
        }

        // GET: Instructor/EditQuiz/5 - Show edit form
        [HttpGet]
        public async Task<IActionResult> EditQuiz(int id)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get quiz with all questions and options
            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuestionOptions)
                .FirstOrDefaultAsync(q => q.QuizId == id && q.Course.InstructorId == userId && !q.IsDeleted);

            if (quiz == null)
                return NotFound();

            // Map to CreateQuizViewModel for editing
            var model = new CreateQuizViewModel
            {
                QuizId = quiz.QuizId,
                CourseId = quiz.CourseId,
                Title = quiz.Title,
                Description = quiz.Description ?? "",
                DueDate = quiz.DueDate,
                TimeLimitMinutes = quiz.TimeLimitMinutes ?? 0,
                Questions = quiz.QuizQuestions.Select(q => new QuestionViewModel
                {
                    QuestionId = q.QuestionId,
                    QuestionText = q.QuestionText,
                    QuestionType = q.QuestionType,
                    Points = q.Points,
                    CorrectAnswerText = q.QuestionOptions.FirstOrDefault(o => o.IsCorrect)?.OptionText ?? "",
                    Options = q.QuestionOptions.Select(o => new OptionViewModel
                    {
                        OptionId = o.OptionId,
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect
                    }).ToList()
                }).ToList()
            };

            // Get instructor's courses for dropdown
            var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .ToListAsync();
            ViewBag.Courses = courses;
            ViewBag.IsEditing = true;

            ViewData["Title"] = $"Edit Quiz - {quiz.Title}";
            return View("CreateQuiz", model);
        }

        // POST: Instructor/EditQuiz - Update quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuiz(CreateQuizViewModel model)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            if (!ModelState.IsValid)
            {
                ViewBag.Courses = await _context.Courses.Where(c => c.InstructorId == userId).ToListAsync();
                ViewBag.IsEditing = true;
                return View("CreateQuiz", model);
            }

            try
            {
                // Get existing quiz and verify ownership
                var quiz = await _context.Quizzes
                    .Include(q => q.Course)
                    .Include(q => q.QuizQuestions)
                        .ThenInclude(qq => qq.QuestionOptions)
                    .FirstOrDefaultAsync(q => q.QuizId == model.QuizId && q.Course.InstructorId == userId);

                if (quiz == null)
                    return NotFound();

                // Update quiz metadata
                quiz.Title = model.Title;
                quiz.Description = model.Description;
                quiz.DueDate = model.DueDate;
                quiz.TimeLimitMinutes = model.TimeLimitMinutes;
                quiz.CourseId = model.CourseId;

                // Remove questions that are no longer in the model
                var existingQuestionIds = quiz.QuizQuestions.Select(q => q.QuestionId).ToList();
                var modelQuestionIds = model.Questions.Where(q => q.QuestionId > 0).Select(q => q.QuestionId).ToList();
                var questionsToRemove = quiz.QuizQuestions.Where(q => !modelQuestionIds.Contains(q.QuestionId)).ToList();
                
                foreach (var questionToRemove in questionsToRemove)
                {
                    _context.QuestionOptions.RemoveRange(questionToRemove.QuestionOptions);
                    _context.QuizQuestions.Remove(questionToRemove);
                }

                // Update existing questions and add new ones
                foreach (var qVm in model.Questions)
                {
                    QuizQuestion question;
                    
                    if (qVm.QuestionId > 0)
                    {
                        // Update existing question
                        question = quiz.QuizQuestions.FirstOrDefault(q => q.QuestionId == qVm.QuestionId);
                        if (question != null)
                        {
                            question.QuestionText = qVm.QuestionText;
                            question.QuestionType = qVm.QuestionType;
                            question.Points = qVm.Points;

                            // Remove old options
                            _context.QuestionOptions.RemoveRange(question.QuestionOptions);
                        }
                        else
                        {
                            continue; // Question not found, skip
                        }
                    }
                    else
                    {
                        // Add new question
                        question = new QuizQuestion
                        {
                            QuizId = quiz.QuizId,
                            QuestionText = qVm.QuestionText,
                            QuestionType = qVm.QuestionType,
                            Points = qVm.Points
                        };
                        _context.QuizQuestions.Add(question);
                        await _context.SaveChangesAsync(); // Save to get QuestionId
                    }

                    // Add options for this question
                    var options = new List<QuestionOption>();

                    if (qVm.QuestionType == "MultipleChoice")
                    {
                        foreach (var optVm in qVm.Options)
                        {
                            if (!string.IsNullOrWhiteSpace(optVm.OptionText))
                            {
                                options.Add(new QuestionOption
                                {
                                    QuestionId = question.QuestionId,
                                    OptionText = optVm.OptionText,
                                    IsCorrect = optVm.IsCorrect
                                });
                            }
                        }
                    }
                    else if (qVm.QuestionType == "TrueFalse")
                    {
                        bool isTrueCorrect = qVm.CorrectAnswerText?.ToLower() == "true";
                        options.Add(new QuestionOption { QuestionId = question.QuestionId, OptionText = "True", IsCorrect = isTrueCorrect });
                        options.Add(new QuestionOption { QuestionId = question.QuestionId, OptionText = "False", IsCorrect = !isTrueCorrect });
                    }
                    else if (qVm.QuestionType == "ShortAnswer")
                    {
                        if (!string.IsNullOrWhiteSpace(qVm.CorrectAnswerText))
                        {
                            options.Add(new QuestionOption
                            {
                                QuestionId = question.QuestionId,
                                OptionText = qVm.CorrectAnswerText,
                                IsCorrect = true
                            });
                        }
                    }

                    if (options.Any())
                    {
                        _context.QuestionOptions.AddRange(options);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Quiz updated successfully.";
                return RedirectToAction("MyQuizzes");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error updating quiz: " + ex.Message);
                ViewBag.Courses = await _context.Courses.Where(c => c.InstructorId == userId).ToListAsync();
                ViewBag.IsEditing = true;
                return View("CreateQuiz", model);
            }
        }

        // POST: Instructor/DeleteQuiz/5 - Soft delete quiz
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            try
            {
                if (!IsLoggedIn() || !await IsInstructor())
                    return Json(new { success = false, message = "Not authorized" });

                var userId = GetCurrentUserId() ?? 0;

                // Get quiz and verify ownership
                var quiz = await _context.Quizzes
                    .Include(q => q.Course)
                    .Include(q => q.QuizAttempts)
                    .FirstOrDefaultAsync(q => q.QuizId == id && q.Course.InstructorId == userId);

                if (quiz == null)
                    return Json(new { success = false, message = "Quiz not found" });

                // Soft delete
                quiz.IsDeleted = true;
                await _context.SaveChangesAsync();

                var attemptCount = quiz.QuizAttempts.Count;
                var message = attemptCount > 0 
                    ? $"Quiz deleted successfully. {attemptCount} student attempt(s) preserved."
                    : "Quiz deleted successfully.";

                return Json(new { success = true, message = message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
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
                return View("Settings");
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "New password and confirmation do not match.";
                ViewBag.User = user;
                return View("Settings");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            ViewBag.SuccessMessage = "Password changed successfully.";
            ViewBag.User = user;
            return View("Settings");
        }
    }
}

