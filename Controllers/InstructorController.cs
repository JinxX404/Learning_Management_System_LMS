using Learning_Management_System.Models;
using Learning_Management_System.Helpers;
using Learning_Management_System.Models.ViewModels;
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

        #region Helper Methods

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

        #endregion
        //======================= Dashboard & Profile ===================================== ✔️ //
        #region 1. Dashboard & Profile
        //  =================== Helpers =======================
        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");
        private async Task<bool> IsInstructorLoggedIn()
        {
            var id = GetUserId();
            return id.HasValue && await _context.Users
                .AnyAsync(u => u.UserId == id && u.Role == "Instructor");
        }
        private async Task<User?> GetLoggedUser()
        {
            var id = GetUserId();
            return id.HasValue ? await _context.Users.FindAsync(id.Value) : null;
        }
        private IActionResult ErrorSettings(string msg, User user)
        {
            ViewBag.ErrorMessage = msg;
            ViewBag.User = user;
            return View("Settings");
        }
        private IActionResult SuccessSettings(string msg, User user)
        {
            ViewBag.SuccessMessage = msg;
            ViewBag.User = user;
            return View("Settings");
        }
        // ======================= Dashboard ======================
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            if (!await IsInstructorLoggedIn())
                return RedirectToAction("Login", "Auth");

            int userId = GetUserId()!.Value;

            ViewBag.CoursesCount = await _context.Courses
                .CountAsync(c => c.InstructorId == userId);

            ViewBag.QuizzesCount = await _context.Quizzes
                .CountAsync(q => !q.IsDeleted && q.Course.InstructorId == userId);

            ViewData["Title"] = "Instructor Dashboard";

            return View();
        }
        //======================== Profile ======================
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var id = GetUserId();
            if (id is null) return RedirectToAction("Login", "Auth");

            var user = await _context.Users
                .Include(u => u.InstructorProfile)
                .Include(u => u.Institution)
                .FirstOrDefaultAsync(u => u.UserId == id.Value);

            if (user is null) return RedirectToAction("Login", "Auth");

            ViewBag.User = user;

            ViewBag.Courses = await _context.Courses
                .Where(c => c.InstructorId == id.Value)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View();
        }
        // ======================== Settings ==============================
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var user = await GetLoggedUser();
            if (user is null) return RedirectToAction("Login", "Auth");

            ViewBag.User = user;
            return View();
        }
        //  ==================== Change Password ==============================
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var user = await GetLoggedUser();
            if (user is null) return RedirectToAction("Login", "Auth");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                return ErrorSettings("Incorrect current password.", user);

            if (newPassword != confirmPassword)
                return ErrorSettings("New password and confirmation do not match.", user);

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();

            return SuccessSettings("Password changed successfully.", user);
        }
        //========================================================================//
        #endregion
        //======================== Course Management ====================================== ✔️ //
        #region 2. Course Management
        //  ===============================Helpers===============================
        private int? UserId => HttpContext.Session.GetInt32("UserId");

        private async Task<bool> IsAuthorized()
        {
            return UserId.HasValue && await _context.Users
                .AnyAsync(u => u.UserId == UserId && u.Role == "Instructor");
        }

        private async Task<IActionResult?> CheckAuth()
        {
            if (!await IsAuthorized())
                return RedirectToAction("Login", "Auth");

            return null;
        }

        private async Task<List<AcademicTerm>> LoadActiveTerms()
        {
            return await _context.AcademicTerms.Where(t => t.IsActive).ToListAsync();
        }
        //=============================== My Courses ===============================
        [HttpGet]
        public async Task<IActionResult> MyCourses()
        {
            var auth = await CheckAuth();
            if (auth != null) return auth;

            int instructorId = UserId!.Value;

            ViewBag.Courses = await _context.Courses
                .Where(c => c.InstructorId == instructorId)
                .Include(c => c.AcademicTerm)
                .Include(c => c.CourseEnrollments)
                .ToListAsync();

            ViewBag.AcademicTerms = await LoadActiveTerms();
            ViewData["Title"] = "My Courses";

            return View();
        }
        //=============================== Create Course (GET) ===============================
        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            var auth = await CheckAuth();
            if (auth != null) return auth;

            ViewBag.AcademicTerms = await LoadActiveTerms();
            ViewData["Title"] = "Create Course";

            return View();
        }
        //=============================== Create Course (POST) ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(string courseCode, string title, string description, int academicTermId, int creditHours)
        {
            var auth = await CheckAuth();
            if (auth != null) return auth;

            if (string.IsNullOrWhiteSpace(courseCode) || string.IsNullOrWhiteSpace(title))
            {
                ViewBag.Error = "Course code and title are required.";
                ViewBag.AcademicTerms = await LoadActiveTerms();
                return View("MyCourses");
            }

            int instructorId = UserId!.Value;

            academicTermId = academicTermId > 0
                ? academicTermId
                : (await _context.AcademicTerms.FirstOrDefaultAsync(t => t.IsActive))?.AcademicTermId ?? 0;

            creditHours = creditHours > 0 ? creditHours : 3;

            try
            {
                var course = new Course
                {
                    CourseCode = courseCode,
                    Title = title,
                    Description = description,
                    InstructorId = instructorId,
                    AcademicTermId = academicTermId,
                    CreditHours = creditHours,
                    MaxEnrollment = 50,
                    CurrentEnrollment = 0,
                    Status = "Active",
                    CreatedAt = DateTime.Now
                };

                _context.Courses.Add(course);

                if (await _context.SaveChangesAsync() <= 0)
                {
                    ViewBag.Error = "Failed to save course.";
                    ViewBag.AcademicTerms = await LoadActiveTerms();
                    return View("MyCourses");
                }

                TempData["Success"] = "Course created successfully!";
                return RedirectToAction("MyCourses");
            }
            catch (Exception e)
            {
                ViewBag.Error = $"Error saving course: {e.Message}";
                ViewBag.AcademicTerms = await LoadActiveTerms();
                return View("MyCourses");
            }
        }
        //=============================== Course Details ===============================
        [HttpGet]
        public async Task<IActionResult> CourseDetails(int id)
        {
            var auth = await CheckAuth();
            if (auth != null) return auth;

            int instructorId = UserId!.Value;

            var course = await _context.Courses
                .Where(c => c.CourseId == id && c.InstructorId == instructorId)
                .Include(c => c.Lectures).ThenInclude(l => l.LearningAssets)
                .Include(c => c.Quizzes)
                .FirstOrDefaultAsync();

            if (course == null) return NotFound();

            ViewBag.Course = course;
            ViewData["Title"] = course.Title;

            return View();
        }
        //=============================== Course Roster ===============================
        [HttpGet]
        public async Task<IActionResult> CourseRoster(int id)
        {
            var auth = await CheckAuth();
            if (auth != null) return auth;

            int instructorId = UserId!.Value;

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == id && c.InstructorId == instructorId);

            if (course == null) return NotFound();

            ViewBag.Course = course;

            ViewBag.Enrollments = await _context.CourseEnrollments
                .Where(e => e.CourseId == id)
                .Include(e => e.User)
                .ToListAsync();

            ViewData["Title"] = "Course Roster";

            return View();
        }

        #endregion
        //======================== Lecture Management ====================================== ✔️ //
        #region 3. Lecture Management
        //  =============================== Add Lecture ================================
        [HttpPost]
        public async Task<IActionResult> AddLecture(int courseId, string title, string description)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.InstructorId == userId);

            if (course == null)
                return NotFound();

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
        //  =============================== Edit Lecture (GET) ================================
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

        //  =============================== Edit Lecture (POST) ================================
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

        //  =============================== Delete Lecture ================================
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

            _context.LearningAssets.RemoveRange(lecture.LearningAssets);
            _context.Lectures.Remove(lecture);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Lecture deleted successfully.";
            return RedirectToAction("CourseDetails", new { id = lecture.CourseId });
        }
        #endregion
        //======================== Learning Asset Management =============================== ✔️ //
        #region 4. Learning Asset Management
        //  =============================== Add Learning Asset (GET) ================================
        [HttpGet]
        public async Task<IActionResult> AddLearningAsset(int? lectureId, int? courseId)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var model = new LearningAsset();
            ViewBag.LectureId = lectureId;
            ViewBag.LectureTitle = string.Empty;
            ViewBag.CourseId = courseId ?? 0;

            if (lectureId.HasValue)
            {
                var lecture = await _context.Lectures
                    .Include(l => l.Course)
                    .FirstOrDefaultAsync(l => l.LectureId == lectureId.Value);

                if (lecture == null || lecture.Course.InstructorId != GetCurrentUserId())
                    return NotFound();

                ViewBag.LectureTitle = lecture.Title;
                ViewBag.LectureId = lecture.LectureId;
                ViewBag.CourseId = lecture.CourseId;
                model.LectureId = lecture.LectureId;
            }

            return View(model);
        }
        //  =============================== Add Learning Asset (POST) ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLearningAsset(int? courseId, int? lectureId, string title, string assetType, string fileUrl)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            Lecture? lecture = null;

            if (lectureId.HasValue)
            {
                lecture = await _context.Lectures
                    .Include(l => l.Course)
                    .FirstOrDefaultAsync(l => l.LectureId == lectureId.Value);

                if (lecture == null || lecture.Course.InstructorId != GetCurrentUserId())
                    return NotFound();
            }
            else if (courseId.HasValue)
            {
                lecture = new Lecture
                {
                    CourseId = courseId.Value,
                    Title = title ?? "New Lecture"
                };
                _context.Lectures.Add(lecture);
                await _context.SaveChangesAsync();
            }
            else
            {
                return BadRequest("LectureId or CourseId must be provided.");
            }

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
        //  =============================== Edit Learning Asset (GET) ================================
        [HttpGet]
        public async Task<IActionResult> EditLearningAsset(int id)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var asset = await _context.LearningAssets
                .Include(a => a.Lecture)
                .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(a => a.AssetId == id);

            if (asset == null || asset.Lecture.Course.InstructorId != GetCurrentUserId())
                return NotFound();

            ViewBag.LectureTitle = asset.Lecture.Title;
            ViewBag.LectureId = asset.LectureId;
            ViewBag.CourseId = asset.Lecture.CourseId;

            return View(asset);
        }
        //  =============================== Edit Learning Asset (POST) ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLearningAsset(int assetId, string title, string assetType, string fileUrl)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var asset = await _context.LearningAssets
                .Include(a => a.Lecture)
                .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(a => a.AssetId == assetId);

            if (asset == null || asset.Lecture.Course.InstructorId != GetCurrentUserId())
                return NotFound();

            asset.Title = title;
            asset.AssetType = assetType;
            asset.FileUrl = fileUrl;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Learning asset updated successfully.";
            return RedirectToAction("CourseDetails", new { id = asset.Lecture.CourseId });
        }
        //  =============================== Delete Learning Asset ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLearningAsset(int assetId)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var asset = await _context.LearningAssets
                .Include(a => a.Lecture)
                .ThenInclude(l => l.Course)
                .FirstOrDefaultAsync(a => a.AssetId == assetId);

            if (asset == null || asset.Lecture.Course.InstructorId != GetCurrentUserId())
                return NotFound();

            var courseId = asset.Lecture.CourseId;

            _context.LearningAssets.Remove(asset);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Learning asset deleted successfully.";
            return RedirectToAction("CourseDetails", new { id = courseId });
        }
        #endregion
        //======================== Quiz Management ========================================= ✔️ //
        #region 5. Quiz Management

        // =============================== My Quizzes (GET) ================================
        [HttpGet]
        public async Task<IActionResult> MyQuizzes()
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

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

        // =============================== Create Quiz (GET) ================================
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

        // =============================== Create Quiz (POST) ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateQuiz(CreateQuizViewModel model)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                var userId = GetCurrentUserId() ?? 0;
                ViewBag.Courses = await _context.Courses
                    .Where(c => c.InstructorId == userId)
                    .ToListAsync();
                return View(model);
            }

            try
            {
                var quiz = new Quiz
                {
                    CourseId = model.CourseId,
                    Title = model.Title,
                    Description = model.Description,
                    DueDate = model.DueDate,
                    TimeLimitMinutes = model.TimeLimitMinutes
                };

                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();

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
                    await _context.SaveChangesAsync();

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
                TempData["Success"] = $"Quiz '{quiz.Title}' created successfully!";
                return RedirectToAction("MyQuizzes");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error creating quiz: " + ex.Message);
                var userId = GetCurrentUserId() ?? 0;
                ViewBag.Courses = await _context.Courses
                    .Where(c => c.InstructorId == userId)
                    .ToListAsync();
                return View(model);
            }
        }

        // =============================== Quiz Details (GET) ================================
        [HttpGet]
        public async Task<IActionResult> QuizDetails(int id)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuestionOptions)
                .Include(q => q.QuizAttempts)
                    .ThenInclude(qa => qa.User)
                .FirstOrDefaultAsync(q => q.QuizId == id && q.Course.InstructorId == userId && !q.IsDeleted);

            if (quiz == null)
                return NotFound();

            var attempts = quiz.QuizAttempts.ToList();
            ViewBag.TotalAttempts = attempts.Count;
            ViewBag.AverageScore = attempts.Any() ? attempts.Average(a => a.Score ?? 0) : 0;
            ViewBag.CompletionRate = attempts.Any() ?
                attempts.Count(a => a.SubmittedAt.HasValue) * 100.0 / attempts.Count : 0;

            ViewData["Title"] = $"Quiz Details - {quiz.Title}";
            return View(quiz);
        }

        // =============================== Edit Quiz (GET) ================================
        [HttpGet]
        public async Task<IActionResult> EditQuiz(int id)
        {
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            var quiz = await _context.Quizzes
                .Include(q => q.Course)
                .Include(q => q.QuizQuestions)
                    .ThenInclude(qq => qq.QuestionOptions)
                .FirstOrDefaultAsync(q => q.QuizId == id && q.Course.InstructorId == userId && !q.IsDeleted);

            if (quiz == null)
                return NotFound();

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

            var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .ToListAsync();
            ViewBag.Courses = courses;
            ViewBag.IsEditing = true;

            ViewData["Title"] = $"Edit Quiz - {quiz.Title}";
            return View("CreateQuiz", model);
        }

        // =============================== Edit Quiz (POST) ================================
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
                var quiz = await _context.Quizzes
                    .Include(q => q.Course)
                    .Include(q => q.QuizQuestions)
                        .ThenInclude(qq => qq.QuestionOptions)
                    .FirstOrDefaultAsync(q => q.QuizId == model.QuizId && q.Course.InstructorId == userId);

                if (quiz == null)
                    return NotFound();

                quiz.Title = model.Title;
                quiz.Description = model.Description;
                quiz.DueDate = model.DueDate;
                quiz.TimeLimitMinutes = model.TimeLimitMinutes;
                quiz.CourseId = model.CourseId;

                var existingQuestionIds = quiz.QuizQuestions.Select(q => q.QuestionId).ToList();
                var modelQuestionIds = model.Questions.Where(q => q.QuestionId > 0).Select(q => q.QuestionId).ToList();
                var questionsToRemove = quiz.QuizQuestions.Where(q => !modelQuestionIds.Contains(q.QuestionId)).ToList();

                foreach (var questionToRemove in questionsToRemove)
                {
                    _context.QuestionOptions.RemoveRange(questionToRemove.QuestionOptions);
                    _context.QuizQuestions.Remove(questionToRemove);
                }

                foreach (var qVm in model.Questions)
                {
                    QuizQuestion question;

                    if (qVm.QuestionId > 0)
                    {
                        question = quiz.QuizQuestions.FirstOrDefault(q => q.QuestionId == qVm.QuestionId);
                        if (question != null)
                        {
                            question.QuestionText = qVm.QuestionText;
                            question.QuestionType = qVm.QuestionType;
                            question.Points = qVm.Points;

                            _context.QuestionOptions.RemoveRange(question.QuestionOptions);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        question = new QuizQuestion
                        {
                            QuizId = quiz.QuizId,
                            QuestionText = qVm.QuestionText,
                            QuestionType = qVm.QuestionType,
                            Points = qVm.Points
                        };
                        _context.QuizQuestions.Add(question);
                        await _context.SaveChangesAsync();
                    }

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

        // =============================== Delete Quiz (POST) ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            try
            {
                if (!IsLoggedIn() || !await IsInstructor())
                    return Json(new { success = false, message = "Not authorized" });

                var userId = GetCurrentUserId() ?? 0;

                var quiz = await _context.Quizzes
                    .Include(q => q.Course)
                    .Include(q => q.QuizAttempts)
                    .FirstOrDefaultAsync(q => q.QuizId == id && q.Course.InstructorId == userId);

                if (quiz == null)
                    return Json(new { success = false, message = "Quiz not found" });

                quiz.IsDeleted = true;
                await _context.SaveChangesAsync();

                var attemptCount = quiz.QuizAttempts.Count;
                var message = attemptCount > 0
                    ? $"Quiz deleted successfully. {attemptCount} student attempt(s) preserved."
                    : "Quiz deleted successfully.";

                return Json(new { success = true, message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        #endregion
        //========================= Grading ================================================ ✔️ //
        #region 6. Grading
        // =============================== Grading (GET) ================================
        [HttpGet]
        public async Task<IActionResult> Grading(int? courseId)
        {
            // Check if user is logged in and is an instructor
            if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

            // =============================== Get Instructor's Courses ================================
            var instructorCourses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .OrderBy(c => c.Title)
                .Select(c => new { c.CourseId, c.Title })
                .ToListAsync();

            var courseIds = instructorCourses.Select(c => c.CourseId).ToList();

            // =============================== Build Grades Query ================================
            var gradesQuery = _context.Grades
                .Where(g => courseIds.Contains(g.GradeBook.CourseId));

            // =============================== Apply Course Filter ================================
            if (courseId.HasValue && courseId.Value > 0)
            {
                gradesQuery = gradesQuery.Where(g => g.GradeBook.CourseId == courseId.Value);
            }

            // =============================== Execute Query and Fetch Data ================================
            var grades = await gradesQuery
                .Include(g => g.User)                      // Student data
                .Include(g => g.GradeBook)                 // Gradable item data
                    .ThenInclude(gb => gb.Course)         // Course data
                .OrderByDescending(g => g.GradedAt)       // Order by graded date
                .Select(g => new
                {
                    GradeId = g.GradeId,
                    UserId = g.UserId,
                    FirstName = g.User.FirstName,
                    LastName = g.User.LastName,
                    CourseId = g.GradeBook.CourseId,
                    CourseTitle = g.GradeBook.Course.Title,
                    GradableItemType = g.GradableItemType,
                    ItemTitle = g.GradeBook.Name,
                    Points = g.Points,
                    MaxPoints = g.MaxPoints,
                    Percentage = g.Percentage,
                    GradedAt = g.GradedAt,
                    Comments = g.Comments
                })
                .ToListAsync();

            // =============================== Pass Data to View ================================
            ViewBag.Grades = grades;                       // All grades
            ViewBag.InstructorCourses = instructorCourses; // Instructor's courses
            ViewBag.SelectedCourseId = courseId;          // Selected course for filter
            ViewData["Title"] = "My Grades";              // Page title

            // =============================== Return View ================================
            return View();
        }
        #endregion
        //========================= Announcements ========================================== ✔️//
        #region 7. Announcements
        // =============================== Announcements (GET) ================================
        [HttpGet]
        public async Task<IActionResult> Announcements()
        {
             if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

             var courses = await _context.Courses
                .Where(c => c.InstructorId == userId)
                .Include(c => c.CourseEnrollments)
                .ToListAsync();

            var courseIds = courses.Select(c => c.CourseId).ToList();

             var enrolledStudentIds = await _context.CourseEnrollments
                .Where(e => courseIds.Contains(e.CourseId))
                .Select(e => e.UserId)
                .Distinct()
                .ToListAsync();

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
        // =============================== Create Announcement (POST) ================================
        [HttpPost]
        public async Task<IActionResult> CreateAnnouncement(int courseId, string message, bool isImportant = false)
        {
             if (!IsLoggedIn() || !await IsInstructor())
                return RedirectToAction("Login", "Auth");

            var userId = GetCurrentUserId() ?? 0;

             var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.InstructorId == userId);

            if (course == null)
                return NotFound();

            var enrolledUserIds = await _context.CourseEnrollments
                .Where(e => e.CourseId == courseId)
                .Select(e => e.UserId)
                .ToListAsync();

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
        // =============================== Send Announcement (AJAX POST) ================================
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

                var course = await _context.Courses
                    .FirstOrDefaultAsync(c => c.CourseId == courseId && c.InstructorId == userId);

                if (course == null)
                    return Json(new { success = false, message = "Course not found" });

                var enrolledUserIds = await _context.CourseEnrollments
                    .Where(e => e.CourseId == courseId)
                    .Select(e => e.UserId)
                    .ToListAsync();

                int notificationCount = 0;
                var notificationsList = new List<Notification>();

                if (enrolledUserIds.Any())
                {
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
                    var instructorNotification = new Notification
                    {
                        UserId = userId,
                        Message = $"[{course.Title}] {message}",
                        IsRead = true,
                        CreatedAt = DateTime.Now
                    };

                    _context.Notifications.Add(instructorNotification);
                    notificationCount = 0;
                }

                var savedCount = await _context.SaveChangesAsync();

                if (savedCount == 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Failed to save announcement to database"
                    });
                }
                string responseMessage = enrolledUserIds.Any()
                    ? $"Announcement sent successfully to {notificationCount} student(s) and saved to database"
                    : $"Announcement saved successfully (No students enrolled yet)";

                return Json(new
                {
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
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}",
                    details = ex.InnerException?.Message
                });
            }
        }
        #endregion
    }
}

