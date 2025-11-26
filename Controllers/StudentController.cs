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




        public async Task<IActionResult> CourseContent(int id, int? lectureId = null)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get course with lectures
            var course = await _context.Courses
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.LearningAssets)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            // Get current lecture
            var lecture = course.Lectures.FirstOrDefault(l => l.LectureId == lectureId) ?? course.Lectures.FirstOrDefault();

            // Get assets for current lecture
            var assets = lecture?.LearningAssets ?? Enumerable.Empty<LearningAsset>();

            // Calculate progress
            var totalLectures = course.Lectures.Count;
            var completedLectures = await _context.CourseEnrollments
                .CountAsync(lp => lp.UserId == userId && lp.Status == "Completed");
            var progress = totalLectures > 0 ? (decimal)completedLectures / totalLectures * 100 : 0;

            ViewBag.Course = course;
            ViewBag.Lecture = lecture;
            ViewBag.Assets = assets;
            ViewBag.Progress = progress;

            return View();
        }
        public async Task<IActionResult> Assignments(int? courseId = null)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get enrolled courses
            var enrollments = await _context.CourseEnrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .ToListAsync();

            ViewBag.EnrolledCourses = enrollments;
            ViewBag.SelectedCourseId = courseId;

            // Future: Fetch actual assignments here
            // For now, we return empty to separate from Quizzes
            ViewBag.Assignments = new List<dynamic>(); 

            ViewData["Title"] = "Course Assignments";

            return View();
        }     
        public async Task<IActionResult> Grades(int? courseId = null)
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

            // Calculate GPA
            decimal gpa = CalculateGpa(grades);
            ViewBag.Gpa = gpa;

            // Calculate total credits
            var totalCredits = grades
                .Where(g => g.GradeBook != null && g.GradeBook.Course != null)
                .GroupBy(g => g.GradeBook.CourseId)
                .Sum(group => group.First().GradeBook.Course.CreditHours);

            ViewBag.TotalCredits = totalCredits;

            // Academic standing
            string academicStanding = "N/A";
            if (gpa >= 3.5m) academicStanding = "Dean's List";
            else if (gpa >= 3.0m) academicStanding = "Good";
            else if (gpa >= 2.0m) academicStanding = "Satisfactory";
            else academicStanding = "Needs Attention";

            ViewBag.AcademicStanding = academicStanding;

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
        public async Task<IActionResult> MarkLessonComplete(int CourseId, int userId)
        {
            try
            {
                if (!IsLoggedIn() || GetCurrentUserId() != userId)
                    return Json(new { success = false, message = "Not authorized" });

                var existingProgress = await _context.CourseEnrollments
                    .FirstOrDefaultAsync(lp => lp.UserId == userId && lp.CourseId == CourseId);

                if (existingProgress != null)
                {
                    existingProgress.Status = "Completed";
                }
                else
                {
                    var newProgress = new CourseEnrollment
                    {
                        UserId = userId,
                        CourseId = CourseId,
                        Status = "Completed"
                    };
                    _context.CourseEnrollments.Add(newProgress);
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Lesson marked as complete" });
            }
            catch (Exception ex)
            {
                // log the error for debugging
                Console.WriteLine($"Error in MarkLessonComplete: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while marking lesson as complete." });
            }
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
            // احسب من كل الدرجات اللي عندك
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
        
        public async Task<IActionResult> CourseDetails(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Verify student is enrolled
            var isEnrolled = await _context.CourseEnrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == id);

            if (!isEnrolled)
                return RedirectToAction("MyCourses");

            // Get course with details
            var course = await _context.Courses
                .Include(c => c.Instructor)
                    .ThenInclude(i => i.InstructorProfile)
                .Include(c => c.AcademicTerm)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            ViewBag.Course = course;
            ViewBag.IsEnrolled = isEnrolled;
            ViewData["Title"] = course.Title;

            return View();
        }

        public async Task<IActionResult> CourseQuizzes(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Verify student is enrolled
            var isEnrolled = await _context.CourseEnrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == id);

            if (!isEnrolled)
                return RedirectToAction("MyCourses");

            // Get course details for header
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            // Get quizzes for this course
            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == id)
                .Select(q => new
                {
                    QuizId = q.QuizId,
                    Title = q.Title,
                    Description = q.Description,
                    DueDate = q.DueDate,
                    TimeLimitMinutes = q.TimeLimitMinutes,
                    QuestionCount = q.QuizQuestions.Count(),
                    IsDeleted = q.IsDeleted,
                    // Get the LATEST attempt for this user and quiz
                    LatestAttempt = _context.QuizAttempts
                        .Where(a => a.UserId == userId && a.QuizId == q.QuizId)
                        .OrderByDescending(a => a.StartedAt)
                        .Select(a => new { a.AttemptId, a.SubmittedAt })
                        .FirstOrDefault()
                })
                .ToListAsync();

            // Process status in memory (easier than complex LINQ translation)
            var quizViewModels = quizzes.Select(q => new
            {
                q.QuizId,
                q.Title,
                q.Description,
                q.DueDate,
                q.TimeLimitMinutes,
                q.QuestionCount,
                q.IsDeleted,
                // Determine Status based on Latest Attempt
                Status = q.LatestAttempt == null ? "Available" :
                         q.LatestAttempt.SubmittedAt == null ? "InProgress" : "Completed",
                LastAttemptId = q.LatestAttempt?.AttemptId
            });

            // Filter: Show quiz if NOT deleted OR student has already submitted/started
            var visibleQuizzes = quizViewModels
                .Where(q => !q.IsDeleted || q.Status != "Available")
                .ToList();

            ViewBag.Course = course;
            ViewBag.Quizzes = visibleQuizzes;
            ViewData["Title"] = $"{course.Title} - Quizzes";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveQuizProgress(int attemptId, int questionId, string answer)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var attempt = await _context.QuizAttempts
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.UserId == userId);

            if (attempt == null || attempt.SubmittedAt != null)
                return BadRequest("Invalid attempt or already submitted.");

            // Find existing response or create new
            var response = await _context.QuizResponses
                .FirstOrDefaultAsync(r => r.AttemptId == attemptId && r.QuestionId == questionId);

            if (response == null)
            {
                response = new QuizResponse
                {
                    AttemptId = attemptId,
                    QuestionId = questionId
                };
                _context.QuizResponses.Add(response);
            }

            // Determine if it's an OptionID (int) or Text
            if (int.TryParse(answer, out int optionId))
            {
                response.SelectedOptionId = optionId;
                response.ResponseText = null;
            }
            else
            {
                response.SelectedOptionId = null;
                response.ResponseText = answer;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        public async Task<IActionResult> QuizPage(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get quiz with questions and options
            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuestionOptions)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null)
                return NotFound();

            // Verify enrollment
            var isEnrolled = await _context.CourseEnrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == quiz.CourseId);

            if (!isEnrolled)
                return RedirectToAction("MyCourses");

            ViewBag.Quiz = quiz;
            ViewData["Title"] = quiz.Title;

            // Check Due Date
            if (quiz.DueDate.HasValue && quiz.DueDate.Value < DateTime.Now)
            {
                TempData["ErrorMessage"] = "This quiz is past its due date and can no longer be taken.";
                return RedirectToAction("CourseQuizzes", new { id = quiz.CourseId });
            }

            // 1. Check for ACTIVE (Incomplete) attempt first
            var activeAttempt = await _context.QuizAttempts
                .Include(qa => qa.QuizResponses)
                .Where(a => a.UserId == userId && a.QuizId == id && a.SubmittedAt == null)
                .OrderByDescending(a => a.StartedAt)
                .FirstOrDefaultAsync();

            if (activeAttempt != null)
            {
                // RESUME active session
                ViewBag.AttemptId = activeAttempt.AttemptId;
                ViewBag.StartedAt = activeAttempt.StartedAt;
                
                // Prepare existing responses
                var existingResponses = new Dictionary<int, string>();
                foreach(var r in activeAttempt.QuizResponses)
                {
                    if (r.SelectedOptionId.HasValue)
                        existingResponses[r.QuestionId] = r.SelectedOptionId.Value.ToString();
                    else if (!string.IsNullOrEmpty(r.ResponseText))
                        existingResponses[r.QuestionId] = r.ResponseText;
                }
                ViewBag.ExistingResponses = existingResponses;
            }
            else
            {
                // 2. Check for COMPLETED attempts
                var completedAttempt = await _context.QuizAttempts
                    .Where(a => a.UserId == userId && a.QuizId == id && a.SubmittedAt != null)
                    .OrderByDescending(a => a.StartedAt)
                    .FirstOrDefaultAsync();

                if (completedAttempt != null)
                {
                    // Already taken -> Show result
                    TempData["InfoMessage"] = "You have already submitted this quiz.";
                    return RedirectToAction("QuizResult", new { id = completedAttempt.AttemptId });
                }

                // 3. Create NEW attempt
                var newAttempt = new QuizAttempt
                {
                    UserId = userId,
                    QuizId = id,
                    StartedAt = DateTime.Now
                };
                _context.QuizAttempts.Add(newAttempt);
                await _context.SaveChangesAsync();
                
                ViewBag.AttemptId = newAttempt.AttemptId;
                ViewBag.StartedAt = newAttempt.StartedAt;
                ViewBag.ExistingResponses = new Dictionary<int, string>();
            }

            return View();
        }
        public async Task<IActionResult> QuizResult(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get quiz attempt with questions and responses
            var quizAttempt = await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                    .ThenInclude(q => q.QuizQuestions)
                        .ThenInclude(qq => qq.QuestionOptions)
                .Include(qa => qa.QuizResponses)
                    .ThenInclude(qr => qr.SelectedOption)
                .FirstOrDefaultAsync(qa => qa.AttemptId == id && qa.UserId == userId);

            if (quizAttempt == null)
                return NotFound();

            // Calculate correct answers
            decimal totalScore = 0;
            decimal maxScore = 0;
            
            foreach (var question in quizAttempt.Quiz.QuizQuestions)
            {
                maxScore += question.Points;
                var response = quizAttempt.QuizResponses.FirstOrDefault(qr => qr.QuestionId == question.QuestionId);
                
                if (response != null)
                {
                    bool isCorrect = false;
                    
                    if (question.QuestionType == "ShortAnswer")
                    {
                        // Compare text with the correct option text
                        var correctOption = question.QuestionOptions.FirstOrDefault(o => o.IsCorrect);
                        if (correctOption != null && string.Equals(response.ResponseText?.Trim(), correctOption.OptionText?.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            isCorrect = true;
                        }
                    }
                    else
                    {
                        // Check selected option
                        if (response.SelectedOption?.IsCorrect == true)
                        {
                            isCorrect = true;
                        }
                    }

                    if (isCorrect)
                    {
                        totalScore += question.Points;
                    }
                }
            }
            
            // Update the attempt score in DB if needed (optional, but good practice)
            quizAttempt.Score = totalScore;
            await _context.SaveChangesAsync();

            ViewBag.QuizAttempt = quizAttempt;
            ViewBag.TotalScore = totalScore;
            ViewBag.MaxScore = maxScore;
            ViewBag.Percentage = maxScore > 0 ? Math.Round(totalScore / maxScore * 100, 2) : 0;
            ViewData["Title"] = "Quiz Results";

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> SubmitQuiz(int quizId, int attemptId, Dictionary<int, string> answers)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get the existing attempt
            var attempt = await _context.QuizAttempts
                .Include(a => a.QuizResponses)
                .FirstOrDefaultAsync(a => a.AttemptId == attemptId);

            if (attempt == null || attempt.UserId != userId || attempt.QuizId != quizId)
                return NotFound();

            if (attempt.SubmittedAt != null)
                return BadRequest("Quiz already submitted.");

            // Get quiz questions to calculate score
            var quiz = await _context.Quizzes
                .Include(q => q.QuizQuestions)
                .FirstOrDefaultAsync(q => q.QuizId == quizId);

            if (quiz == null) return NotFound();

            // Clear previous responses (if any) to avoid duplicates on re-submission/update
            _context.QuizResponses.RemoveRange(attempt.QuizResponses);

            // Save new responses
            foreach (var answer in answers)
            {
                var questionId = answer.Key;
                var answerValue = answer.Value;
                
                var question = quiz.QuizQuestions.FirstOrDefault(q => q.QuestionId == questionId);
                if (question == null) continue;

                var response = new QuizResponse
                {
                    AttemptId = attempt.AttemptId,
                    QuestionId = questionId
                };

                if (question.QuestionType == "ShortAnswer")
                {
                    response.ResponseText = answerValue;
                }
                else
                {
                    if (int.TryParse(answerValue, out int optionId))
                    {
                        response.SelectedOptionId = optionId;
                    }
                }

                _context.QuizResponses.Add(response);
            }

            // Mark as submitted
            attempt.SubmittedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            // Redirect to results
            return RedirectToAction("QuizResult", new { id = attempt.AttemptId });
        }
        [HttpPost]
        // public async Task<IActionResult> SubmitAssignment(int gradeId, string? submissionText, IFormFile? file)
        // {
        //     if (!IsLoggedIn())
        //         return RedirectToAction("Login", "Auth");

        //     var userId = GetCurrentUserId() ?? 0;

        //     // تأكد إن الطالب مسموح له ي-submit
        //     var grade = await _context.Grades
        //         .Include(g => g.GradeBook)
        //             .ThenInclude(gb => gb.Course)
        //         .FirstOrDefaultAsync(g => g.GradeId == gradeId);

        //     if (grade == null || grade.UserId != userId)
        //         return NotFound();

        //     // احفظ الملف لو موجود
        //     string? filePath = null;
        //     if (file != null && file.Length > 0)
        //     {
        //         var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "assignments");
        //         Directory.CreateDirectory(uploadsFolder);
        //         var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        //         var fullPath = Path.Combine(uploadsFolder, fileName);
        //         using (var stream = new FileStream(fullPath, FileMode.Create))
        //         {
        //             await file.CopyToAsync(stream);
        //         }
        //         filePath = $"/uploads/assignments/{fileName}";
        //     }

        //     // أنشئ Submission جديد
        //     var submission = new AssignmentSubmission
        //     {
        //         GradeId = gradeId,
        //         UserId = userId,
        //         SubmissionText = submissionText,
        //         FilePath = filePath
        //     };

        //     _context.AssignmentSubmissions.Add(submission);
        //     await _context.SaveChangesAsync();

        //     return RedirectToAction("AssignmentSubmission", new { id = gradeId });
        // }
        // public async Task<IActionResult> AssignmentSubmission(int id)
        // {
        //     if (!IsLoggedIn())
        //         return RedirectToAction("Login", "Auth");

        //     var userId = GetCurrentUserId() ?? 0;

        //     var assignment = await _context.Grades
        //         .Include(g => g.GradeBook)
        //             .ThenInclude(gb => gb.Course)
        //         .FirstOrDefaultAsync(g => g.GradeId == id && g.UserId == userId);

        //     if (assignment == null)
        //         return NotFound();

        //     var submissionHistory = await _context.AssignmentSubmissions
        //         .Where(s => s.GradeId == id && s.UserId == userId)
        //         .OrderByDescending(s => s.SubmittedAt)
        //         .ToListAsync();

        //     ViewBag.Assignment = assignment;
        //     ViewBag.SubmissionHistory = submissionHistory;
        //     ViewBag.LastSubmissionText = submissionHistory.FirstOrDefault()?.SubmissionText;

        //     return View();
        // }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(
    string firstName,
    string lastName,
    string currentPassword,
    string newPassword,
    string confirmPassword)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Auth");

            // تحقق من الباسورد القديم
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            {
                ViewBag.ErrorMessage = "Current password is incorrect.";
                ViewBag.User = user;
                return View("AccountSettings");
            }

            // تحقق من تطابق الباسورد الجديد
            if (newPassword != confirmPassword)
            {
                ViewBag.ErrorMessage = "New password and confirmation do not match.";
                ViewBag.User = user;
                return View("AccountSettings");
            }

            // حدّث البيانات
            user.FirstName = firstName;
            user.LastName = lastName;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.Now;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            ViewBag.SuccessMessage = "Profile updated successfully.";
            ViewBag.User = user;
            return View("AccountSettings");
        }
    }
}
