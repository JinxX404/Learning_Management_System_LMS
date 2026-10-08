using Microsoft.AspNetCore.Mvc;
using Learning_Management_System.Models;
using Learning_Management_System.Helpers;
using Learning_Management_System.ViewModels.Student.Request;
using Learning_Management_System.ViewModels.Student.Response;
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

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var studentId = GetCurrentUserId();
            if (studentId == null) return RedirectToAction("Login", "Auth");

            var student = await _context.Users
                .Include(s => s.CourseEnrollments)
                    .ThenInclude(e => e.Course)
                        .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(s => s.UserId == studentId);

            if (student == null) return RedirectToAction("Login", "Auth");

            var notifications = await _context.Notifications
                .Where(n => n.UserId == studentId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToListAsync();

            var assignmentCount = await _context.QuizAttempts
                .CountAsync(qa => qa.UserId == studentId && qa.SubmittedAt != null);

            var model = new DashboardResponseViewModel
            {
                Enrollments = student.CourseEnrollments.ToList(),
                EnrollmentCount = student.CourseEnrollments.Count,
                RecentNotifications = notifications,
                AssignmentCount = assignmentCount
            };

            ViewData["StudentActive"] = "dashboard";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var studentId = GetCurrentUserId();
            if (studentId == null) return RedirectToAction("Login", "Auth");

            var enrollments = await _context.CourseEnrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .Where(e => e.UserId == studentId)
                .ToListAsync();

            var model = new MyCoursesResponseViewModel { Enrollments = enrollments };
            ViewData["StudentActive"] = "courses";
            return View(model);
        }




        [HttpGet]
        public async Task<IActionResult> CourseContent(int id, int? lectureId = null)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var course = await _context.Courses
                .Include(c => c.Lectures)
                    .ThenInclude(l => l.LearningAssets)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            var lecture = course.Lectures.FirstOrDefault(l => l.LectureId == lectureId) ?? course.Lectures.FirstOrDefault();

            var assets = lecture?.LearningAssets ?? Enumerable.Empty<LearningAsset>();

            var totalLectures = course.Lectures.Count;
            var completedLectures = await _context.CourseEnrollments
                .CountAsync(lp => lp.UserId == userId && lp.Status == "Completed");
            var progress = totalLectures > 0 ? (decimal)completedLectures / totalLectures * 100 : 0;

            var model = new CourseContentResponseViewModel
            {
                Course = course,
                Lecture = lecture,
                Assets = assets,
                Progress = progress
            };

            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Assignments(int? courseId = null)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var enrollments = await _context.CourseEnrollments
                .Where(e => e.UserId == userId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .ToListAsync();

            var model = new AssignmentsResponseViewModel
            {
                EnrolledCourses = enrollments,
                SelectedCourseId = courseId,
                Assignments = new List<dynamic>()
            };

            ViewData["Title"] = "Course Assignments";

            return View(model);
        }     
        [HttpGet]
        public async Task<IActionResult> Grades()
        {
            
                if (!IsLoggedIn())
                    return RedirectToAction("Login", "Auth");

                var userId = GetCurrentUserId() ?? 0;

                var grades = await _context.AllStudentGrades
                    .Where(v => v.UserId == userId)
                    .OrderByDescending(v => v.GradedAt)
                    .ToListAsync();

                var model = new GradesResponseViewModel { Grades = grades };
                ViewData["Title"] = "My Grades";

                return View(model);
            
        }

        [HttpGet]
        public async Task<IActionResult> Announcements()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var notifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var model = new AnnouncementsResponseViewModel
            {
                Notifications = notifications,
                UnreadCount = notifications.Count(n => !n.IsRead)
            };
            ViewData["Title"] = "Announcements";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkLessonComplete(MarkLessonCompleteRequestViewModel model)
        {
            try
            {
                if (!IsLoggedIn() || GetCurrentUserId() != model.UserId)
                    return Json(new LessonCompleteResponseViewModel { Success = false, Message = "Not authorized" });

                var existingProgress = await _context.CourseEnrollments
                    .FirstOrDefaultAsync(lp => lp.UserId == model.UserId && lp.CourseId == model.CourseId);

                if (existingProgress != null)
                {
                    existingProgress.Status = "Completed";
                }
                else
                {
                    var newProgress = new CourseEnrollment
                    {
                        UserId = model.UserId,
                        CourseId = model.CourseId,
                        Status = "Completed"
                    };
                    _context.CourseEnrollments.Add(newProgress);
                }

                await _context.SaveChangesAsync();

                return Json(new LessonCompleteResponseViewModel { Success = true, Message = "Lesson marked as complete" });
            }
            catch (Exception ex)
            {
                // log the error for debugging
                Console.WriteLine($"Error in MarkLessonComplete: {ex.Message}");
                return Json(new LessonCompleteResponseViewModel { Success = false, Message = "An error occurred while marking lesson as complete." });
            }
        }
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

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

            var model = new ProfileResponseViewModel
            {
                User = user,
                Enrollments = enrollments
            };
            ViewData["Title"] = "Profile";

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> AccountSettings()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return RedirectToAction("Login", "Auth");

            var model = new AccountSettingsResponseViewModel { User = user };
            ViewData["Title"] = "Account Settings";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccountSettings(AccountSettingsRequestViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Auth");

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UpdatedAt = DateTime.Now;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var response = new AccountSettingsResponseViewModel
            {
                User = user,
                SuccessMessage = "Profile updated successfully."
            };
            ViewData["Title"] = "Account Settings";

            return View(response);
        }

        
        [HttpGet]
        public async Task<IActionResult> CourseDetails(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var isEnrolled = await _context.CourseEnrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == id);

            if (!isEnrolled)
                return RedirectToAction("MyCourses");

            var course = await _context.Courses
                .Include(c => c.Instructor)
                    .ThenInclude(i => i.InstructorProfile)
                .Include(c => c.AcademicTerm)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            var model = new CourseDetailsResponseViewModel { Course = course, IsEnrolled = isEnrolled };
            ViewData["Title"] = course.Title;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CourseQuizzes(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var isEnrolled = await _context.CourseEnrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == id);

            if (!isEnrolled)
                return RedirectToAction("MyCourses");

            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

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
                    LatestAttempt = _context.QuizAttempts
                        .Where(a => a.UserId == userId && a.QuizId == q.QuizId)
                        .OrderByDescending(a => a.StartedAt)
                        .Select(a => new { a.AttemptId, a.SubmittedAt })
                        .FirstOrDefault()
                })
                .ToListAsync();

            var quizViewModels = quizzes.Select(q => new QuizListItemViewModel
            {
                QuizId = q.QuizId,
                Title = q.Title,
                Description = q.Description,
                DueDate = q.DueDate,
                TimeLimitMinutes = q.TimeLimitMinutes,
                QuestionCount = q.QuestionCount,
                IsDeleted = q.IsDeleted,
                Status = q.LatestAttempt == null ? "Available" :
                         q.LatestAttempt.SubmittedAt == null ? "InProgress" : "Completed",
                LastAttemptId = q.LatestAttempt?.AttemptId
            });

            var visibleQuizzes = quizViewModels
                .Where(q => !q.IsDeleted || q.Status != "Available")
                .ToList();

            var model = new CourseQuizzesResponseViewModel
            {
                Course = course,
                Quizzes = visibleQuizzes
            };
            ViewData["Title"] = $"{course.Title} - Quizzes";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveQuizProgress(SaveQuizProgressRequestViewModel model)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var attempt = await _context.QuizAttempts
                .FirstOrDefaultAsync(a => a.AttemptId == model.AttemptId && a.UserId == userId);

            if (attempt == null || attempt.SubmittedAt != null)
                return BadRequest("Invalid attempt or already submitted.");

            // Find existing response or create new
            var response = await _context.QuizResponses
                .FirstOrDefaultAsync(r => r.AttemptId == model.AttemptId && r.QuestionId == model.QuestionId);

            if (response == null)
            {
                response = new QuizResponse
                {
                    AttemptId = model.AttemptId,
                    QuestionId = model.QuestionId
                };
                _context.QuizResponses.Add(response);
            }

            // Determine if it's an OptionID (int) or Text
            if (int.TryParse(model.Answer, out int optionId))
            {
                response.SelectedOptionId = optionId;
                response.ResponseText = null;
            }
            else
            {
                response.SelectedOptionId = null;
                response.ResponseText = model.Answer;
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> QuizPage(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuestionOptions)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null)
                return NotFound();

            var isEnrolled = await _context.CourseEnrollments
                .AnyAsync(e => e.UserId == userId && e.CourseId == quiz.CourseId);

            if (!isEnrolled)
                return RedirectToAction("MyCourses");

            ViewData["Title"] = quiz.Title;

            if (quiz.DueDate.HasValue && quiz.DueDate.Value < DateTime.Now)
            {
                TempData["ErrorMessage"] = "This quiz is past its due date and can no longer be taken.";
                return RedirectToAction("CourseQuizzes", new { id = quiz.CourseId });
            }

            var activeAttempt = await _context.QuizAttempts
                .Include(qa => qa.QuizResponses)
                .Where(a => a.UserId == userId && a.QuizId == id && a.SubmittedAt == null)
                .OrderByDescending(a => a.StartedAt)
                .FirstOrDefaultAsync();

            if (activeAttempt != null)
            {
                var existingResponses = new Dictionary<int, string>();
                foreach(var r in activeAttempt.QuizResponses)
                {
                    if (r.SelectedOptionId.HasValue)
                        existingResponses[r.QuestionId] = r.SelectedOptionId.Value.ToString();
                    else if (!string.IsNullOrEmpty(r.ResponseText))
                        existingResponses[r.QuestionId] = r.ResponseText;
                }

                var model = new QuizPageResponseViewModel
                {
                    Quiz = quiz,
                    AttemptId = activeAttempt.AttemptId,
                    StartedAt = activeAttempt.StartedAt,
                    ExistingResponses = existingResponses
                };
                return View(model);
            }
            else
            {
                var completedAttempt = await _context.QuizAttempts
                    .Where(a => a.UserId == userId && a.QuizId == id && a.SubmittedAt != null)
                    .OrderByDescending(a => a.StartedAt)
                    .FirstOrDefaultAsync();

                if (completedAttempt != null)
                {
                    TempData["InfoMessage"] = "You have already submitted this quiz.";
                    return RedirectToAction("QuizResult", new { id = completedAttempt.AttemptId });
                }

                var newAttempt = new QuizAttempt
                {
                    UserId = userId,
                    QuizId = id,
                    StartedAt = DateTime.Now
                };
                _context.QuizAttempts.Add(newAttempt);
                await _context.SaveChangesAsync();

                var model = new QuizPageResponseViewModel
                {
                    Quiz = quiz,
                    AttemptId = newAttempt.AttemptId,
                    StartedAt = newAttempt.StartedAt,
                    ExistingResponses = new Dictionary<int, string>()
                };
                return View(model);
            }
        }
        [HttpGet]
        public async Task<IActionResult> QuizResult(int id)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var quizAttempt = await _context.QuizAttempts
                .Include(qa => qa.Quiz)
                    .ThenInclude(q => q.QuizQuestions)
                        .ThenInclude(qq => qq.QuestionOptions)
                .Include(qa => qa.QuizResponses)
                    .ThenInclude(qr => qr.SelectedOption)
                .FirstOrDefaultAsync(qa => qa.AttemptId == id && qa.UserId == userId);

            if (quizAttempt == null)
                return NotFound();

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
                        var correctOption = question.QuestionOptions.FirstOrDefault(o => o.IsCorrect);
                        if (correctOption != null && string.Equals(response.ResponseText?.Trim(), correctOption.OptionText?.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            isCorrect = true;
                        }
                    }
                    else
                    {
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
            
            quizAttempt.Score = totalScore;
            await _context.SaveChangesAsync();

            var model = new QuizResultResponseViewModel
            {
                QuizAttempt = quizAttempt,
                TotalScore = totalScore,
                MaxScore = maxScore,
                Percentage = maxScore > 0 ? Math.Round(totalScore / maxScore * 100, 2) : 0
            };
            ViewData["Title"] = "Quiz Results";

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitQuiz(SubmitQuizRequestViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // Get the existing attempt
            var attempt = await _context.QuizAttempts
                .Include(a => a.QuizResponses)
                .FirstOrDefaultAsync(a => a.AttemptId == model.AttemptId);

            if (attempt == null || attempt.UserId != userId || attempt.QuizId != model.QuizId)
                return NotFound();

            if (attempt.SubmittedAt != null)
                return BadRequest("Quiz already submitted.");

            // Get quiz questions to calculate score
            var quiz = await _context.Quizzes
                .Include(q => q.QuizQuestions)
                .FirstOrDefaultAsync(q => q.QuizId == model.QuizId);

            if (quiz == null) return NotFound();

            // Clear previous responses (if any) to avoid duplicates on re-submission/update
            _context.QuizResponses.RemoveRange(attempt.QuizResponses);

            // Save new responses
            foreach (var answer in model.Answers)
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
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC sp_SubmitQuizAttempt @p0",
                attempt.AttemptId
            );


            // Redirect to results
            return RedirectToAction("QuizResult", new { id = attempt.AttemptId });
        }
    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestViewModel model)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return RedirectToAction("Login", "Auth");

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
            {
                var errorModel = new AccountSettingsResponseViewModel
                {
                    User = user,
                    ErrorMessage = "Current password is incorrect."
                };
                return View("AccountSettings", errorModel);
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                var errorModel = new AccountSettingsResponseViewModel
                {
                    User = user,
                    ErrorMessage = "New password and confirmation do not match."
                };
                return View("AccountSettings", errorModel);
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            user.UpdatedAt = DateTime.Now;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            var successModel = new AccountSettingsResponseViewModel
            {
                User = user,
                SuccessMessage = "Profile updated successfully."
            };
            return View("AccountSettings", successModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboardUpdates()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            // 1. Unread Notifications
            var unreadCount = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            // 2. Upcoming Deadlines (Quizzes due in next 48 hours)
            var upcomingDeadlines = await _context.Quizzes
                .Where(q => q.DueDate != null && q.DueDate > DateTime.Now && q.DueDate <= DateTime.Now.AddHours(48))
                .Where(q => _context.CourseEnrollments.Any(e => e.UserId == userId && e.CourseId == q.CourseId)) // Enrolled only
                .Where(q => !_context.QuizAttempts.Any(qa => qa.QuizId == q.QuizId && qa.UserId == userId && qa.SubmittedAt != null)) // Exclude completed
                .Select(q => new
                {
                    title = q.Title,
                    courseName = q.Course.Title,
                    dueInMinutes = (int)EF.Functions.DateDiffMinute(DateTime.Now, q.DueDate),
                    dueDate = q.DueDate
                })
                .OrderBy(q => q.dueDate)
                .ToListAsync();

            return Json(new DashboardUpdatesResponseViewModel
            {
                UnreadCount = unreadCount,
                UpcomingDeadlines = upcomingDeadlines.Select(q => new UpcomingDeadlineItem
                {
                    Title = q.title,
                    CourseName = q.courseName,
                    DueInMinutes = q.dueInMinutes,
                    DueDate = q.dueDate
                }).ToList()
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetGradeTrends()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var grades = await _context.Grades
                .Where(g => g.UserId == userId && g.MaxPoints > 0)
                .Include(g => g.GradeBook)
                .OrderBy(g => g.GradedAt)
                .ToListAsync();

            var result = grades.Select(g => new
            {
                label = g.GradeBook?.Name ?? "Assessment",
                score = Math.Round((g.Points / g.MaxPoints) * 100, 1)
            });

            return Json(new GradeTrendResponseViewModel
            {
                Labels = result.Select(r => r.label).ToList(),
                Data = result.Select(r => r.score).ToList()
            });
        }
    }
}
