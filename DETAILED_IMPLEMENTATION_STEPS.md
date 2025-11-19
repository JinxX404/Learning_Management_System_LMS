# Detailed Implementation Steps - LMS Backend

This document provides step-by-step instructions for implementing each feature. Follow these steps in order.

---

## DAY 1: Foundation Setup

### Step 1: Install BCrypt Package
**What to do:**
1. Open terminal/PowerShell in project root directory
2. Run: `dotnet add package BCrypt.Net-Next`
3. Verify: Check that package appears in `.csproj` file

**Why:** We need BCrypt to securely hash passwords before storing them in database.

---

### Step 2: Configure Session in Program.cs
**What to do:**
1. Open `Program.cs`
2. After line `builder.Services.AddControllersWithViews();`, add:
   ```csharp
   builder.Services.AddSession(options => {
       options.IdleTimeout = TimeSpan.FromMinutes(30);
       options.Cookie.HttpOnly = true;
   });
   ```
3. After `app.UseRouting();` and before `app.UseAuthorization();`, add:
   ```csharp
   app.UseSession();
   ```
4. Add using at top: `using Microsoft.AspNetCore.Session;`

**Why:** Sessions allow us to store user login state between requests.

**Flow:** When user logs in, we store their ID in session. On each request, we check session to see if they're logged in.

---

### Step 3: Register DbContext in Program.cs
**What to do:**
1. In `Program.cs`, after other `builder.Services` calls, add:
   ```csharp
   builder.Services.AddDbContext<LmsContext>(options =>
       options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
   ```
2. Add usings:
   ```csharp
   using Learning_Management_System.Models;
   using Microsoft.EntityFrameworkCore;
   ```

**Why:** This tells ASP.NET to inject LmsContext into controllers when needed.

**Flow:** When a controller needs database access, ASP.NET automatically creates LmsContext and passes it to the constructor.

---

### Step 4: Update LmsContext.cs
**What to do:**
1. Open `Models/LmsContext.cs`
2. Find the `OnConfiguring` method (around line 112)
3. Comment it out or remove it (it has hardcoded connection string)
4. Make sure constructor accepts `DbContextOptions<LmsContext>` (should already be there)

**Why:** We want to use connection string from appsettings.json, not hardcoded.

---

### Step 5: Create SessionHelper
**What to do:**
1. Create folder `Helpers/` in project root
2. Create file `Helpers/SessionHelper.cs`
3. Add this code:
   ```csharp
   using Microsoft.AspNetCore.Http;

   namespace Learning_Management_System.Helpers
   {
       public static class SessionHelper
       {
           public static void SetUserId(ISession session, int userId)
           {
               session.SetInt32("UserId", userId);
           }

           public static int? GetUserId(ISession session)
           {
               return session.GetInt32("UserId");
           }

           public static void ClearSession(ISession session)
           {
               session.Clear();
           }
       }
   }
   ```

**Why:** Helper methods make it easier to work with sessions throughout the app.

**Flow:**
- `SetUserId`: Stores user ID when they log in
- `GetUserId`: Retrieves user ID to check if logged in
- `ClearSession`: Removes all session data on logout

---

### Step 6: Create SeedData
**What to do:**
1. Create folder `Data/` in project root
2. Create file `Data/SeedData.cs`
3. Add this code:
   ```csharp
   using Learning_Management_System.Models;
   using Microsoft.EntityFrameworkCore;

   namespace Learning_Management_System.Data
   {
       public static class SeedData
       {
           public static async Task SeedAdminUser(LmsContext context)
           {
               // Check if admin already exists
               var adminExists = await context.Users
                   .AnyAsync(u => u.Email == "admin@lms.com");
               if (adminExists) return;

               // Get or create institution
               var institution = await context.Institutions.FirstOrDefaultAsync();
               if (institution == null)
               {
                   institution = new Institution
                   {
                       Name = "Default Institution",
                       IsActive = true,
                       CreatedAt = DateTime.Now
                   };
                   context.Institutions.Add(institution);
                   await context.SaveChangesAsync();
               }

               // Create admin user
               var hashedPassword = BCrypt.Net.BCrypt.HashPassword("Admin123!");
               var admin = new User
               {
                   Email = "admin@lms.com",
                   PasswordHash = hashedPassword,
                   FirstName = "Admin",
                   LastName = "User",
                   Role = "Admin",
                   InstitutionId = institution.InstitutionId,
                   IsActive = true,
                   CreatedAt = DateTime.Now
               };

               context.Users.Add(admin);
               await context.SaveChangesAsync();
           }
       }
   }
   ```
4. In `Program.cs`, after `var app = builder.Build();`, add:
   ```csharp
   using (var scope = app.Services.CreateScope())
   {
       var context = scope.ServiceProvider.GetRequiredService<LmsContext>();
       await SeedData.SeedAdminUser(context);
   }
   ```

**Why:** Creates a default admin account so you can log in and test the system.

**Flow:** When app starts, it checks if admin exists. If not, creates one with email "admin@lms.com" and password "Admin123!".

---

## DAY 2-3: Authentication (Team Member 1)

### Step 1: Update AuthController Constructor
**What to do:**
1. Open `Controllers/AuthController.cs`
2. Add field and constructor:
   ```csharp
   private readonly LmsContext _context;

   public AuthController(LmsContext context)
   {
       _context = context;
   }
   ```
3. Add using: `using Learning_Management_System.Models;`

**Why:** Controller needs access to database to check user credentials.

---

### Step 2: Implement Login POST Method
**Flow:**
1. User enters email and password in login form
2. Form submits to `Login` POST method
3. Find user in database by email
4. Verify password matches
5. If valid: Store user ID in session, redirect to their dashboard
6. If invalid: Show error, stay on login page

**What to do:**
1. Update the existing `[HttpPost] Login` method:
   ```csharp
   [HttpPost]
   public async Task<IActionResult> Login(string email, string password)
   {
       // Find user by email
       var user = await _context.Users
           .FirstOrDefaultAsync(u => u.Email == email);

       // Check if user exists and password is correct
       if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
       {
           ViewBag.ErrorMessage = "Invalid email or password.";
           ViewData["Title"] = "Login";
           return View();
       }

       // Check if user is active
       if (!user.IsActive)
       {
           ViewBag.ErrorMessage = "Your account is inactive.";
           ViewData["Title"] = "Login";
           return View();
       }

       // Store user ID in session
       SessionHelper.SetUserId(HttpContext.Session, user.UserId);

       // Update last login time
       user.LastLoginAt = DateTime.Now;
       await _context.SaveChangesAsync();

       // Redirect based on role
       if (user.Role == "Student")
           return RedirectToAction("Dashboard", "Student");
       else if (user.Role == "Instructor")
           return RedirectToAction("Dashboard", "Instructor");
       else if (user.Role == "Admin")
           return RedirectToAction("Dashboard", "Admin");

       return RedirectToAction("Login");
   }
   ```
2. Add usings:
   ```csharp
   using Learning_Management_System.Helpers;
   using Microsoft.EntityFrameworkCore;
   using BCrypt.Net;
   ```

**Why:** This is the core authentication logic - verifies credentials and logs user in.

**Testing:** Try logging in with admin@lms.com / Admin123! - should redirect to admin dashboard.

---

### Step 3: Implement Logout Method
**Flow:**
1. User clicks logout button
2. Clear all session data
3. Redirect to login page

**What to do:**
1. Update `Logout()` method:
   ```csharp
   public IActionResult Logout()
   {
       SessionHelper.ClearSession(HttpContext.Session);
       return RedirectToAction("Login");
   }
   ```
2. Add using: `using Learning_Management_System.Helpers;`

**Why:** Logout should remove all session data so user is no longer considered logged in.

---

## DAY 2-3: Student Features (Team Member 2)

### Step 1: Setup StudentController
**What to do:**
1. Open `Controllers/StudentController.cs`
2. Add field and constructor:
   ```csharp
   private readonly LmsContext _context;

   public StudentController(LmsContext context)
   {
       _context = context;
   }
   ```
3. Add helper methods:
   ```csharp
   private int? GetCurrentUserId()
   {
       return SessionHelper.GetUserId(HttpContext.Session);
   }

   private bool IsLoggedIn()
   {
       return GetCurrentUserId() != null;
   }
   ```
4. Add usings:
   ```csharp
   using Learning_Management_System.Models;
   using Learning_Management_System.Helpers;
   using Microsoft.EntityFrameworkCore;
   ```

**Why:** All student actions need to check if user is logged in and get their ID.

---

### Step 2: Implement Dashboard
**Flow:**
1. Check if user is logged in (redirect to login if not)
2. Get current user ID from session
3. Count how many courses student is enrolled in
4. Count upcoming assignments (can be 0 for now)
5. Pass counts to view via ViewBag

**What to do:**
1. Update `Dashboard()` method:
   ```csharp
   public async Task<IActionResult> Dashboard()
   {
       if (!IsLoggedIn())
           return RedirectToAction("Login", "Auth");

       var userId = GetCurrentUserId().Value;

       // Count enrolled courses
       var enrollmentCount = await _context.CourseEnrollments
           .Where(e => e.UserId == userId)
           .CountAsync();

       // Count upcoming assignments (simplified - can be 0 for now)
       var assignmentCount = 0;

       ViewBag.EnrollmentCount = enrollmentCount;
       ViewBag.AssignmentCount = assignmentCount;
       ViewData["Title"] = "Student Dashboard";

       return View();
   }
   ```

**Why:** Dashboard shows student overview - how many courses they're taking.

**Testing:** Login as student, should see dashboard with enrollment count.

---

### Step 3: Implement MyCourses
**Flow:**
1. Check login
2. Get all enrollments for current user
3. Include course details (instructor, term) using `.Include()`
4. Pass list to view

**What to do:**
1. Update `MyCourses()` method:
   ```csharp
   public async Task<IActionResult> MyCourses()
   {
       if (!IsLoggedIn())
           return RedirectToAction("Login", "Auth");

       var userId = GetCurrentUserId().Value;

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
   ```

**Why:** Shows all courses the student is enrolled in with details.

**Testing:** Should see list of enrolled courses with course name, instructor, term.

---

### Step 4: Implement CourseDetails
**Flow:**
1. Check login
2. Get course ID from URL parameter
3. Verify student is enrolled in this course
4. Get course with all lectures
5. Pass to view

**What to do:**
1. Update `CourseDetails(int id)` method:
   ```csharp
   public async Task<IActionResult> CourseDetails(int id)
   {
       if (!IsLoggedIn())
           return RedirectToAction("Login", "Auth");

       var userId = GetCurrentUserId().Value;

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
   ```

**Why:** Shows detailed course information including all lectures.

**Testing:** Click on a course from MyCourses, should see course details and lectures list.

---

### Step 5: Implement Grades
**Flow:**
1. Check login
2. Get all grades for current user
3. Include course and gradebook info
4. Pass to view

**What to do:**
1. Update `Grades()` method:
   ```csharp
   public async Task<IActionResult> Grades()
   {
       if (!IsLoggedIn())
           return RedirectToAction("Login", "Auth");

       var userId = GetCurrentUserId().Value;

       // Get all grades for this student
       var grades = await _context.Grades
           .Where(g => g.UserId == userId)
           .Include(g => g.GradeBook)
               .ThenInclude(gb => gb.Course)
           .OrderByDescending(g => g.GradedAt)
           .ToListAsync();

       ViewBag.Grades = grades;
       ViewData["Title"] = "My Grades";

       return View();
   }
   ```

**Why:** Shows all grades student has received.

**Testing:** Should see list of grades with course name, points, percentage.

---

## DAY 2-3: Instructor Features (Team Member 3)

### Step 1: Create InstructorController
**What to do:**
1. Create new file `Controllers/InstructorController.cs`
2. Add basic structure:
   ```csharp
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
       }
   }
   ```

**Why:** New controller for all instructor-specific actions.

---

### Step 2: Implement Dashboard
**Flow:**
1. Check login and instructor role
2. Count courses where instructor is current user
3. Count pending grades (optional - can be 0)
4. Pass counts to view

**What to do:**
1. Add `Dashboard()` method:
   ```csharp
   public async Task<IActionResult> Dashboard()
   {
       if (!IsLoggedIn() || !await IsInstructor())
           return RedirectToAction("Login", "Auth");

       var userId = GetCurrentUserId().Value;

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
   ```

**Why:** Shows instructor overview of their courses.

---

### Step 3: Implement MyCourses
**Flow:**
1. Check login and instructor role
2. Get all courses where InstructorId matches current user
3. Include related data (term, enrollment count)
4. Pass to view

**What to do:**
1. Add `MyCourses()` method:
   ```csharp
   public async Task<IActionResult> MyCourses()
   {
       if (!IsLoggedIn() || !await IsInstructor())
           return RedirectToAction("Login", "Auth");

       var userId = GetCurrentUserId().Value;

       var courses = await _context.Courses
           .Where(c => c.InstructorId == userId)
           .Include(c => c.AcademicTerm)
           .Include(c => c.CourseEnrollments)
           .ToListAsync();

       ViewBag.Courses = courses;
       ViewData["Title"] = "My Courses";

       return View();
   }
   ```

**Why:** Lists all courses the instructor teaches.

---

### Step 4: Implement CreateCourse (GET)
**Flow:**
1. Check login and instructor role
2. Get list of active academic terms (for dropdown)
3. Show form to create course

**What to do:**
1. Add `CreateCourse()` GET method:
   ```csharp
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
   ```

**Why:** Shows form to create new course.

---

### Step 5: Implement CreateCourse (POST)
**Flow:**
1. Check login and instructor role
2. Validate form data (course code, title required)
3. Create new Course object
4. Set InstructorId to current user
5. Save to database
6. Redirect to MyCourses

**What to do:**
1. Add `[HttpPost] CreateCourse` method:
   ```csharp
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

       var userId = GetCurrentUserId().Value;

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
   ```

**Why:** Saves new course to database.

**Testing:** Fill form, submit, should see new course in MyCourses list.

---

### Step 6: Implement CourseDetails
**Flow:**
1. Check login and instructor role
2. Verify instructor owns this course
3. Get course with all lectures
4. Pass to view

**What to do:**
1. Add `CourseDetails(int id)` method:
   ```csharp
   public async Task<IActionResult> CourseDetails(int id)
   {
       if (!IsLoggedIn() || !await IsInstructor())
           return RedirectToAction("Login", "Auth");

       var userId = GetCurrentUserId().Value;

       // Verify ownership
       var course = await _context.Courses
           .FirstOrDefaultAsync(c => c.CourseId == id && c.InstructorId == userId);

       if (course == null)
           return NotFound();

       // Get course with lectures
       course = await _context.Courses
           .Include(c => c.Lectures)
           .FirstOrDefaultAsync(c => c.CourseId == id);

       ViewBag.Course = course;
       ViewData["Title"] = course.Title;

       return View();
   }
   ```

**Why:** Shows course details and allows managing lectures.

---

### Step 7: Implement AddLecture (POST)
**Flow:**
1. Check login and instructor role
2. Verify instructor owns the course
3. Create new Lecture object
4. Save to database
5. Redirect back to CourseDetails

**What to do:**
1. Add `[HttpPost] AddLecture` method:
   ```csharp
   [HttpPost]
   public async Task<IActionResult> AddLecture(int courseId, string title, string description)
   {
       if (!IsLoggedIn() || !await IsInstructor())
           return RedirectToAction("Login", "Auth");

       var userId = GetCurrentUserId().Value;

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
   ```

**Why:** Allows instructor to add lectures to their courses.

**Testing:** Go to course details, add lecture, should see it in the list.

---

### Step 8: Implement GradeSubmissions
**Flow:**
1. Check login and instructor role
2. Get all grades for instructor's courses that need grading
3. Show list with option to grade
4. When grading: Update Grades table with score and feedback

**What to do:**
1. Add `GradeSubmissions()` method:
   ```csharp
   public async Task<IActionResult> GradeSubmissions()
   {
       if (!IsLoggedIn() || !await IsInstructor())
           return RedirectToAction("Login", "Auth");

       var userId = GetCurrentUserId().Value;

       // Get instructor's course IDs
       var courseIds = await _context.Courses
           .Where(c => c.InstructorId == userId)
           .Select(c => c.CourseId)
           .ToListAsync();

       // Get ungraded items (grades with null points or 0)
       var ungraded = await _context.Grades
           .Where(g => courseIds.Contains(g.GradeBook.CourseId) && 
                      (g.Points == null || g.Points == 0))
           .Include(g => g.User)
           .Include(g => g.GradeBook)
               .ThenInclude(gb => gb.Course)
           .ToListAsync();

       ViewBag.Ungraded = ungraded;
       ViewData["Title"] = "Grade Submissions";

       return View();
   }
   ```

2. Add `[HttpPost] GradeSubmission` method:
   ```csharp
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
       grade.Feedback = feedback;
       grade.GradedAt = DateTime.Now;

       await _context.SaveChangesAsync();

       return RedirectToAction("GradeSubmissions");
   }
   ```

**Why:** Allows instructor to grade student work.

---

## DAY 2-3: Admin Features (Team Member 4)

### Step 1: Create AdminController
**What to do:**
1. Create new file `Controllers/AdminController.cs`
2. Add basic structure (similar to InstructorController but check for "Admin" role)

**Why:** New controller for all admin-specific actions.

---

### Step 2: Implement Dashboard
**Flow:**
1. Check login and admin role
2. Count total users, courses, students
3. Pass counts to view

**What to do:**
1. Add `Dashboard()` method:
   ```csharp
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
   ```

**Why:** Shows system overview statistics.

---

### Step 3: Implement Users (List)
**Flow:**
1. Check login and admin role
2. Get all users from database
3. Include related data
4. Pass list to view

**What to do:**
1. Add `Users()` method:
   ```csharp
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
   ```

**Why:** Lists all users in the system.

---

### Step 4: Implement CreateUser (GET)
**Flow:**
1. Check login and admin role
2. Get list of institutions (for dropdown)
3. Show form to create user

**What to do:**
1. Add `CreateUser()` GET method:
   ```csharp
   public async Task<IActionResult> CreateUser()
   {
       if (!IsLoggedIn() || !await IsAdmin())
           return RedirectToAction("Login", "Auth");

       var institutions = await _context.Institutions.ToListAsync();
       ViewBag.Institutions = institutions;
       ViewData["Title"] = "Create User";

       return View();
   }
   ```

---

### Step 5: Implement CreateUser (POST)
**Flow:**
1. Check login and admin role
2. Validate form data
3. Check if email already exists
4. Hash password with BCrypt
5. Create new User object
6. If role is Student, create StudentProfile
7. If role is Instructor, create InstructorProfile
8. Save to database
9. Redirect to Users list

**What to do:**
1. Add `[HttpPost] CreateUser` method:
   ```csharp
   [HttpPost]
   public async Task<IActionResult> CreateUser(
       string email, 
       string password, 
       string firstName, 
       string lastName, 
       string role, 
       int institutionId)
   {
       if (!IsLoggedIn() || !await IsAdmin())
           return RedirectToAction("Login", "Auth");

       // Validate
       if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
       {
           ViewBag.Error = "Email and password are required.";
           var institutions = await _context.Institutions.ToListAsync();
           ViewBag.Institutions = institutions;
           return View();
       }

       // Check email exists
       var emailExists = await _context.Users
           .AnyAsync(u => u.Email == email);
       if (emailExists)
       {
           ViewBag.Error = "Email already exists.";
           var institutions = await _context.Institutions.ToListAsync();
           ViewBag.Institutions = institutions;
           return View();
       }

       // Hash password
       var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

       // Create user
       var user = new User
       {
           Email = email,
           PasswordHash = hashedPassword,
           FirstName = firstName,
           LastName = lastName,
           Role = role,
           InstitutionId = institutionId,
           IsActive = true,
           CreatedAt = DateTime.Now
       };

       _context.Users.Add(user);
       await _context.SaveChangesAsync();

       // Create profile based on role
       if (role == "Student")
       {
           var profile = new StudentProfile
           {
               UserId = user.UserId,
               StudentIdNumber = $"STU{user.UserId:0000}"
           };
           _context.StudentProfiles.Add(profile);
       }
       else if (role == "Instructor")
       {
           var profile = new InstructorProfile
           {
               UserId = user.UserId
           };
           _context.InstructorProfiles.Add(profile);
       }

       await _context.SaveChangesAsync();

       return RedirectToAction("Users");
   }
   ```

**Why:** Allows admin to create new users in the system.

**Testing:** Create a student user, then try logging in with that email/password.

---

### Step 6: Implement EditUser
**Similar flow to CreateUser but updates existing user instead of creating new one.**

---

### Step 7: Implement AcademicTerms
**Flow:**
1. Check login and admin role
2. Get all academic terms
3. Show list and form to create new term

**What to do:**
1. Add `AcademicTerms()` GET method to show list
2. Add `[HttpPost] CreateAcademicTerm` to create new term

**Why:** Admin needs to manage academic terms (semesters) before courses can be created.

---

## DAY 4-5: Integration & Testing

### Testing Checklist
1. **Authentication Flow:**
   - [ ] Login with admin@lms.com / Admin123! → Should redirect to admin dashboard
   - [ ] Create a student user via admin panel
   - [ ] Logout
   - [ ] Login with student credentials → Should redirect to student dashboard

2. **Student Flow:**
   - [ ] Student logs in
   - [ ] Views dashboard (should show enrollment count)
   - [ ] Views MyCourses (should show enrolled courses)
   - [ ] Views course details
   - [ ] Views grades

3. **Instructor Flow:**
   - [ ] Admin creates instructor user
   - [ ] Instructor logs in
   - [ ] Instructor creates a course
   - [ ] Instructor adds lecture to course
   - [ ] Instructor views course details

4. **Admin Flow:**
   - [ ] Admin views dashboard (should show stats)
   - [ ] Admin views users list
   - [ ] Admin creates new user
   - [ ] Admin edits user
   - [ ] Admin manages academic terms

5. **Integration Flow:**
   - [ ] Admin creates academic term
   - [ ] Admin creates instructor user
   - [ ] Instructor creates course (using the term)
   - [ ] Admin creates student user
   - [ ] Student enrolls in course (if enrollment feature exists)
   - [ ] Instructor grades student work

---

## Common Issues & Solutions

### Issue: "DbContext is not registered"
**Solution:** Make sure you added `builder.Services.AddDbContext<LmsContext>` in Program.cs

### Issue: "Session is null"
**Solution:** Make sure you added `app.UseSession()` in Program.cs after `UseRouting()`

### Issue: "User not found" when logging in
**Solution:** Run the app once to trigger SeedData, or manually create admin user in database

### Issue: "Include() not working"
**Solution:** Make sure you have `using Microsoft.EntityFrameworkCore;` at the top

### Issue: "BCrypt not found"
**Solution:** Make sure you installed the package: `dotnet add package BCrypt.Net-Next`

---

## Next Steps (Bonus Features)

Once core features work, you can add:
- Student enrollment in courses
- Assignment submission
- Quiz creation and taking
- File uploads
- Announcements
- Password reset
- Reports

