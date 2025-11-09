using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Learning_Management_System.Models;

public partial class LmsContext : DbContext
{
    public LmsContext()
    {
    }

    public LmsContext(DbContextOptions<LmsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AcademicTerm> AcademicTerms { get; set; }

    public virtual DbSet<AigeneratedContent> AigeneratedContents { get; set; }

    public virtual DbSet<Aiinteraction> Aiinteractions { get; set; }

    public virtual DbSet<Aimodel> Aimodels { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseEnrollment> CourseEnrollments { get; set; }

    public virtual DbSet<Grade> Grades { get; set; }

    public virtual DbSet<GradeBook> GradeBooks { get; set; }

    public virtual DbSet<Institution> Institutions { get; set; }

    public virtual DbSet<InstructorProfile> InstructorProfiles { get; set; }

    public virtual DbSet<LearningAsset> LearningAssets { get; set; }

    public virtual DbSet<Lecture> Lectures { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<NotificationTemplate> NotificationTemplates { get; set; }

    public virtual DbSet<NotificationType> NotificationTypes { get; set; }

    public virtual DbSet<QuestionOption> QuestionOptions { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizAttempt> QuizAttempts { get; set; }

    public virtual DbSet<QuizQuestion> QuizQuestions { get; set; }

    public virtual DbSet<QuizResponse> QuizResponses { get; set; }

    public virtual DbSet<StudentProfile> StudentProfiles { get; set; }

    public virtual DbSet<Transcript> Transcripts { get; set; }

    public virtual DbSet<TranscriptEntry> TranscriptEntries { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VwActiveTermsWithCourseCount> VwActiveTermsWithCourseCounts { get; set; }

    public virtual DbSet<VwAigeneratedQuiz> VwAigeneratedQuizzes { get; set; }

    public virtual DbSet<VwAiinteractionsByUser> VwAiinteractionsByUsers { get; set; }

    public virtual DbSet<VwAimodelStatus> VwAimodelStatuses { get; set; }

    public virtual DbSet<VwAllStudentGrade> VwAllStudentGrades { get; set; }

    public virtual DbSet<VwCourseDetail> VwCourseDetails { get; set; }

    public virtual DbSet<VwCoursePassFailRate> VwCoursePassFailRates { get; set; }

    public virtual DbSet<VwCoursesWithEnrollment> VwCoursesWithEnrollments { get; set; }

    public virtual DbSet<VwCoursesWithInstructorAndTerm> VwCoursesWithInstructorAndTerms { get; set; }

    public virtual DbSet<VwGradeBookDetail> VwGradeBookDetails { get; set; }

    public virtual DbSet<VwInstitutionSummary> VwInstitutionSummaries { get; set; }

    public virtual DbSet<VwInstructorsWithCourse> VwInstructorsWithCourses { get; set; }

    public virtual DbSet<VwLessonsWithAssetCount> VwLessonsWithAssetCounts { get; set; }

    public virtual DbSet<VwNotificationTemplatesWithType> VwNotificationTemplatesWithTypes { get; set; }

    public virtual DbSet<VwNotificationsWithUserDetail> VwNotificationsWithUserDetails { get; set; }

    public virtual DbSet<VwQuizResponsesWithCorrectness> VwQuizResponsesWithCorrectnesses { get; set; }

    public virtual DbSet<VwQuizzesWithStat> VwQuizzesWithStats { get; set; }

    public virtual DbSet<VwStudentCourseGrade> VwStudentCourseGrades { get; set; }

    public virtual DbSet<VwStudentEnrollment> VwStudentEnrollments { get; set; }

    public virtual DbSet<VwStudentQuizPerformance> VwStudentQuizPerformances { get; set; }

    public virtual DbSet<VwStudentsWithGpaandCourse> VwStudentsWithGpaandCourses { get; set; }

    public virtual DbSet<VwTopPerformingStudent> VwTopPerformingStudents { get; set; }

    public virtual DbSet<VwUsersWithActivity> VwUsersWithActivities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=.;Database=LMS;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AcademicTerm>(entity =>
        {
            entity.HasKey(e => e.AcademicTermId).HasName("PK__Academic__F667F2FCFC75E580");

            entity.HasIndex(e => e.InstitutionId, "IX_AcademicTerms_InstitutionId");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TermName).HasMaxLength(100);

            entity.HasOne(d => d.Institution).WithMany(p => p.AcademicTerms)
                .HasForeignKey(d => d.InstitutionId)
                .HasConstraintName("FK_AcademicTerms_InstitutionId");
        });

        modelBuilder.Entity<AigeneratedContent>(entity =>
        {
            entity.HasKey(e => e.AicontentId).HasName("PK__AIGenera__D067E6EC6FB337D2");

            entity.ToTable("AIGeneratedContent");

            entity.Property(e => e.AicontentId).HasColumnName("AIContentId");
            entity.Property(e => e.AimodelId).HasColumnName("AIModelId");
            entity.Property(e => e.GeneratedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Aimodel).WithMany(p => p.AigeneratedContents)
                .HasForeignKey(d => d.AimodelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AIGeneratedContent_AIModelId");
        });

        modelBuilder.Entity<Aiinteraction>(entity =>
        {
            entity.HasKey(e => e.InteractionId).HasName("PK__AIIntera__922C049657BDDA1B");

            entity.ToTable("AIInteractions");

            entity.Property(e => e.AimodelId).HasColumnName("AIModelId");
            entity.Property(e => e.Airesponse).HasColumnName("AIResponse");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Aimodel).WithMany(p => p.Aiinteractions)
                .HasForeignKey(d => d.AimodelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AIInteractions_AIModelId");

            entity.HasOne(d => d.User).WithMany(p => p.Aiinteractions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AIInteractions_UserId");
        });

        modelBuilder.Entity<Aimodel>(entity =>
        {
            entity.HasKey(e => e.AimodelId).HasName("PK__AIModels__93889CC67E006785");

            entity.ToTable("AIModels");

            entity.Property(e => e.AimodelId).HasColumnName("AIModelId");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ModelName).HasMaxLength(255);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A7BFC035FA");

            entity.ToTable(tb => tb.HasTrigger("trg_CreateGradeBookForCourse"));

            entity.HasIndex(e => e.AcademicTermId, "IX_Courses_AcademicTermId");

            entity.HasIndex(e => e.InstructorId, "IX_Courses_InstructorId");

            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.CreditHours).HasDefaultValue(3);
            entity.Property(e => e.MaxEnrollment).HasDefaultValue(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.AcademicTerm).WithMany(p => p.Courses)
                .HasForeignKey(d => d.AcademicTermId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Courses_AcademicTermId");

            entity.HasOne(d => d.Instructor).WithMany(p => p.Courses)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Courses_InstructorId");
        });

        modelBuilder.Entity<CourseEnrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__CourseEn__7F68771B02E4FA4A");

            entity.ToTable(tb =>
                {
                    tb.HasTrigger("trg_CompleteEnrollmentOnFinalGrade");
                    tb.HasTrigger("trg_UpdateCourseEnrollmentCount");
                });

            entity.HasIndex(e => e.CourseId, "IX_CourseEnrollments_CourseId");

            entity.HasIndex(e => e.UserId, "IX_CourseEnrollments_UserId");

            entity.HasIndex(e => new { e.UserId, e.CourseId }, "UQ_User_Course_Enrollment").IsUnique();

            entity.Property(e => e.EnrolledAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.FinalGrade).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(d => d.Course).WithMany(p => p.CourseEnrollments)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_CourseEnrollments_CourseId");

            entity.HasOne(d => d.User).WithMany(p => p.CourseEnrollments)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_CourseEnrollments_UserId");
        });

        modelBuilder.Entity<Grade>(entity =>
        {
            entity.HasKey(e => e.GradeId).HasName("PK__Grades__54F87A576559713D");

            entity.ToTable(tb => tb.HasTrigger("trg_UpdateGPAOnGradeChange"));

            entity.HasIndex(e => e.GradeBookId, "IX_Grades_GradeBookId");

            entity.HasIndex(e => e.UserId, "IX_Grades_UserId");

            entity.Property(e => e.GradableItemType).HasMaxLength(50);
            entity.Property(e => e.GradedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.MaxPoints).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Percentage)
                .HasComputedColumnSql("(case when [MaxPoints]>(0) then ([Points]/[MaxPoints])*(100) else (0) end)", true)
                .HasColumnType("decimal(17, 8)");
            entity.Property(e => e.Points).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.GradeBook).WithMany(p => p.Grades)
                .HasForeignKey(d => d.GradeBookId)
                .HasConstraintName("FK_Grades_GradeBookId");

            entity.HasOne(d => d.User).WithMany(p => p.Grades)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Grades_UserId");
        });

        modelBuilder.Entity<GradeBook>(entity =>
        {
            entity.HasKey(e => e.GradeBookId).HasName("PK__GradeBoo__347A6A861E3D65C1");

            entity.HasIndex(e => e.CourseId, "UQ__GradeBoo__C92D71A664C0BF3A").IsUnique();

            entity.HasIndex(e => e.CourseId, "UQ__GradeBoo__C92D71A6E792C3E7").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Name).HasMaxLength(255);

            entity.HasOne(d => d.Course).WithOne(p => p.GradeBook)
                .HasForeignKey<GradeBook>(d => d.CourseId)
                .HasConstraintName("FK_GradeBooks_CourseId");
        });

        modelBuilder.Entity<Institution>(entity =>
        {
            entity.HasKey(e => e.InstitutionId).HasName("PK__Institut__8DF6B6ADF3590BE8");

            entity.ToTable(tb => tb.HasTrigger("trg_DeactivateUsersOnInstitutionDeactivation"));

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<InstructorProfile>(entity =>
        {
            entity.HasKey(e => e.InstructorProfileId).HasName("PK__Instruct__3A8F7B2FF6F4112C");

            entity.HasIndex(e => e.UserId, "UQ__Instruct__1788CC4D10CEAA8F").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ__Instruct__1788CC4D7A9132AE").IsUnique();

            entity.Property(e => e.Department).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.InstructorProfile)
                .HasForeignKey<InstructorProfile>(d => d.UserId)
                .HasConstraintName("FK_InstructorProfiles_UserId");
        });

        modelBuilder.Entity<LearningAsset>(entity =>
        {
            entity.HasKey(e => e.AssetId).HasName("PK__Learning__434923524A660EF9");

            entity.HasIndex(e => e.LectureId, "IX_LearningAssets_LessonId");

            entity.Property(e => e.AssetType).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Lecture).WithMany(p => p.LearningAssets)
                .HasForeignKey(d => d.LectureId)
                .HasConstraintName("FK_LearningAssets_LessonId");
        });

        modelBuilder.Entity<Lecture>(entity =>
        {
            entity.HasKey(e => e.LectureId).HasName("PK__Lectures__B739F6BF8462FEC8");

            entity.HasIndex(e => e.CourseId, "IX_Lessons_CourseId");

            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Course).WithMany(p => p.Lectures)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_Lessons_CourseId");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E12F8E9BD5D");

            entity.HasIndex(e => e.UserId, "IX_Notifications_UserId");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Notifications_UserId");
        });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.HasKey(e => e.TemplateId).HasName("PK__Notifica__F87ADD271D66B724");

            entity.Property(e => e.TemplateName).HasMaxLength(100);

            entity.HasOne(d => d.NotificationType).WithMany(p => p.NotificationTemplates)
                .HasForeignKey(d => d.NotificationTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NotificationTemplates_TypeId");
        });

        modelBuilder.Entity<NotificationType>(entity =>
        {
            entity.HasKey(e => e.NotificationTypeId).HasName("PK__Notifica__299002C1ABEB6B61");

            entity.HasIndex(e => e.TypeName, "UQ__Notifica__D4E7DFA856526EA8").IsUnique();

            entity.HasIndex(e => e.TypeName, "UQ__Notifica__D4E7DFA8CED0DD6E").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TypeName).HasMaxLength(100);
        });

        modelBuilder.Entity<QuestionOption>(entity =>
        {
            entity.HasKey(e => e.OptionId).HasName("PK__Question__92C7A1FF5F7F2248");

            entity.HasIndex(e => e.QuestionId, "IX_QuestionOptions_QuestionId");

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionOptions)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_QuestionOptions_QuestionId");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PK__Quizzes__8B42AE8E071DEC99");

            entity.HasIndex(e => e.CourseId, "IX_Quizzes_CourseId");

            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Course).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK_Quizzes_CourseId");
        });

        modelBuilder.Entity<QuizAttempt>(entity =>
        {
            entity.HasKey(e => e.AttemptId).HasName("PK__QuizAtte__891A68E6DEC62FBC");

            entity.HasIndex(e => e.QuizId, "IX_QuizAttempts_QuizId");

            entity.HasIndex(e => e.UserId, "IX_QuizAttempts_UserId");

            entity.Property(e => e.Score).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.StartedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizAttempts)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuizAttempts_QuizId");

            entity.HasOne(d => d.User).WithMany(p => p.QuizAttempts)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_QuizAttempts_UserId");
        });

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__QuizQues__0DC06FAC98720DEB");

            entity.HasIndex(e => e.QuizId, "IX_QuizQuestions_QuizId");

            entity.Property(e => e.Points)
                .HasDefaultValue(10.0m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.QuestionType).HasMaxLength(50);

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizQuestions)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("FK_QuizQuestions_QuizId");
        });

        modelBuilder.Entity<QuizResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("PK__QuizResp__1AAA646CC7E3FE3A");

            entity.HasIndex(e => e.AttemptId, "IX_QuizResponses_AttemptId");

            entity.HasOne(d => d.Attempt).WithMany(p => p.QuizResponses)
                .HasForeignKey(d => d.AttemptId)
                .HasConstraintName("FK_QuizResponses_AttemptId");

            entity.HasOne(d => d.Question).WithMany(p => p.QuizResponses)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuizResponses_QuestionId");

            entity.HasOne(d => d.SelectedOption).WithMany(p => p.QuizResponses)
                .HasForeignKey(d => d.SelectedOptionId)
                .HasConstraintName("FK_QuizResponses_SelectedOptionId");
        });

        modelBuilder.Entity<StudentProfile>(entity =>
        {
            entity.HasKey(e => e.StudentProfileId).HasName("PK__StudentP__222BD0B08764AB7A");

            entity.HasIndex(e => e.UserId, "UQ__StudentP__1788CC4DCAB85C03").IsUnique();

            entity.HasIndex(e => e.UserId, "UQ__StudentP__1788CC4DCC5ECEC6").IsUnique();

            entity.HasIndex(e => e.StudentIdNumber, "UQ__StudentP__EDCE3FE405519064").IsUnique();

            entity.HasIndex(e => e.StudentIdNumber, "UQ__StudentP__EDCE3FE4C3D1F867").IsUnique();

            entity.Property(e => e.Gpa)
                .HasColumnType("decimal(4, 3)")
                .HasColumnName("GPA");
            entity.Property(e => e.StudentIdNumber).HasMaxLength(50);

            entity.HasOne(d => d.User).WithOne(p => p.StudentProfile)
                .HasForeignKey<StudentProfile>(d => d.UserId)
                .HasConstraintName("FK_StudentProfiles_UserId");
        });

        modelBuilder.Entity<Transcript>(entity =>
        {
            entity.HasKey(e => e.TranscriptId).HasName("PK__Transcri__FD083E1310CDA0F9");

            entity.Property(e => e.CumulativeGpa)
                .HasColumnType("decimal(4, 3)")
                .HasColumnName("CumulativeGPA");
            entity.Property(e => e.GeneratedAt).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.StudentProfile).WithMany(p => p.Transcripts)
                .HasForeignKey(d => d.StudentProfileId)
                .HasConstraintName("FK_Transcripts_StudentProfileId");
        });

        modelBuilder.Entity<TranscriptEntry>(entity =>
        {
            entity.HasKey(e => e.TranscriptEntryId).HasName("PK__Transcri__23A57ACDA52C1E03");

            entity.Property(e => e.LetterGrade).HasMaxLength(2);
            entity.Property(e => e.NumericGrade).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Course).WithMany(p => p.TranscriptEntries)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TranscriptEntries_CourseId");

            entity.HasOne(d => d.Grade).WithMany(p => p.TranscriptEntries)
                .HasForeignKey(d => d.GradeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TranscriptEntries_GradeId");

            entity.HasOne(d => d.Transcript).WithMany(p => p.TranscriptEntries)
                .HasForeignKey(d => d.TranscriptId)
                .HasConstraintName("FK_TranscriptEntries_TranscriptId");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C365B9884");

            entity.HasIndex(e => e.InstitutionId, "IX_Users_InstitutionId");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105345A006376").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D10534796C62B0").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(50);

            entity.HasOne(d => d.Institution).WithMany(p => p.Users)
                .HasForeignKey(d => d.InstitutionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Users_InstitutionId");
        });

        modelBuilder.Entity<VwActiveTermsWithCourseCount>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_ActiveTermsWithCourseCounts");

            entity.Property(e => e.TermName).HasMaxLength(100);
        });

        modelBuilder.Entity<VwAigeneratedQuiz>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_AIGeneratedQuizzes");

            entity.Property(e => e.QuizTitle).HasMaxLength(255);
        });

        modelBuilder.Entity<VwAiinteractionsByUser>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_AIInteractionsByUser");

            entity.Property(e => e.Airesponse).HasColumnName("AIResponse");
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.ModelName).HasMaxLength(255);
        });

        modelBuilder.Entity<VwAimodelStatus>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_AIModelStatus");

            entity.Property(e => e.ModelName).HasMaxLength(255);
        });

        modelBuilder.Entity<VwAllStudentGrade>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_AllStudentGrades");

            entity.Property(e => e.CourseTitle).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.GradableItemType).HasMaxLength(50);
            entity.Property(e => e.ItemTitle).HasMaxLength(255);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.MaxPoints).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Percentage).HasColumnType("decimal(17, 8)");
            entity.Property(e => e.Points).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<VwCourseDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_CourseDetails");

            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.CourseTitle).HasMaxLength(255);
            entity.Property(e => e.InstitutionName).HasMaxLength(255);
            entity.Property(e => e.InstructorEmail).HasMaxLength(255);
            entity.Property(e => e.InstructorName).HasMaxLength(201);
            entity.Property(e => e.TermName).HasMaxLength(100);
        });

        modelBuilder.Entity<VwCoursePassFailRate>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_CoursePassFailRate");

            entity.Property(e => e.PassRate).HasColumnType("numeric(38, 6)");
        });

        modelBuilder.Entity<VwCoursesWithEnrollment>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_CoursesWithEnrollment");

            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.CourseId).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<VwCoursesWithInstructorAndTerm>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_CoursesWithInstructorAndTerm");

            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.CourseTitle).HasMaxLength(255);
            entity.Property(e => e.InstructorName).HasMaxLength(201);
            entity.Property(e => e.TermName).HasMaxLength(100);
        });

        modelBuilder.Entity<VwGradeBookDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_GradeBookDetails");

            entity.Property(e => e.CourseTitle).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.GradableItemType).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.MaxPoints).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Points).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<VwInstitutionSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_InstitutionSummary");

            entity.Property(e => e.InstitutionId).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(255);
        });

        modelBuilder.Entity<VwInstructorsWithCourse>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_InstructorsWithCourses");

            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.CourseTitle).HasMaxLength(255);
            entity.Property(e => e.Department).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        modelBuilder.Entity<VwLessonsWithAssetCount>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_LessonsWithAssetCounts");

            entity.Property(e => e.LectureId).ValueGeneratedOnAdd();
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<VwNotificationTemplatesWithType>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_NotificationTemplatesWithTypes");

            entity.Property(e => e.NotificationType).HasMaxLength(100);
            entity.Property(e => e.TemplateName).HasMaxLength(100);
        });

        modelBuilder.Entity<VwNotificationsWithUserDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_NotificationsWithUserDetails");

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        modelBuilder.Entity<VwQuizResponsesWithCorrectness>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_QuizResponsesWithCorrectness");

            entity.Property(e => e.PointsEarned).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<VwQuizzesWithStat>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_QuizzesWithStats");

            entity.Property(e => e.CourseTitle).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(255);
        });

        modelBuilder.Entity<VwStudentCourseGrade>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_StudentCourseGrades");

            entity.Property(e => e.CourseTitle).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.GradableItemType).HasMaxLength(50);
            entity.Property(e => e.ItemTitle).HasMaxLength(255);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.MaxPoints).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Percentage).HasColumnType("decimal(17, 8)");
            entity.Property(e => e.Points).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<VwStudentEnrollment>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_StudentEnrollments");

            entity.Property(e => e.CourseCode).HasMaxLength(50);
            entity.Property(e => e.CourseTitle).HasMaxLength(255);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<VwStudentQuizPerformance>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_StudentQuizPerformance");

            entity.Property(e => e.AverageScore).HasColumnType("decimal(38, 6)");
        });

        modelBuilder.Entity<VwStudentsWithGpaandCourse>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_StudentsWithGPAAndCourses");

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Gpa)
                .HasColumnType("decimal(4, 3)")
                .HasColumnName("GPA");
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        modelBuilder.Entity<VwTopPerformingStudent>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_TopPerformingStudents");

            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.Gpa)
                .HasColumnType("decimal(4, 3)")
                .HasColumnName("GPA");
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        modelBuilder.Entity<VwUsersWithActivity>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_UsersWithActivity");

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Role).HasMaxLength(50);
            entity.Property(e => e.UserId).ValueGeneratedOnAdd();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
