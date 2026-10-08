/*
  Academix LMS - database schema (source of truth)
  =================================================
  Run this script against SQL Server with SSMS or sqlcmd. It creates the LMS
  database in the instance's default data/log directory, then creates every
  table, view, trigger, stored procedure and function the application uses.

  - If the LMS database already exists on your server, skip the
    "CREATE DATABASE" and "USE" statements below and run the rest.
  - The application startup (EnsureCreatedAsync) can create tables in an
    empty database, but it can never create views, triggers or procedures -
    running this script is always required. See README.md,
    "3. Create the database".

  Generated from a live database on 2025-10-15; schema + demonstration seed rows
  (90 INSERT statements, incl. 16 Users). The seeded Users.PasswordHash values are
  PBKDF2-format placeholders (prefix AQAA), which BCrypt login rejects - those demo
  accounts are inert until re-seeded with BCrypt hashes.
*/
CREATE DATABASE [LMS];
GO
USE [LMS];
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [LMS].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [LMS] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [LMS] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [LMS] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [LMS] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [LMS] SET ARITHABORT OFF 
GO
ALTER DATABASE [LMS] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [LMS] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [LMS] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [LMS] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [LMS] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [LMS] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [LMS] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [LMS] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [LMS] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [LMS] SET  ENABLE_BROKER 
GO
ALTER DATABASE [LMS] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [LMS] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [LMS] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [LMS] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [LMS] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [LMS] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [LMS] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [LMS] SET RECOVERY FULL 
GO
ALTER DATABASE [LMS] SET  MULTI_USER 
GO
ALTER DATABASE [LMS] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [LMS] SET DB_CHAINING OFF 
GO
ALTER DATABASE [LMS] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [LMS] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [LMS] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'LMS', N'ON'
GO
/****** Object:  Table [dbo].[AcademicTerms]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AcademicTerms](
	[AcademicTermId] [int] IDENTITY(1,1) NOT NULL,
	[InstitutionId] [int] NOT NULL,
	[TermName] [nvarchar](100) NOT NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NOT NULL,
	[IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AcademicTermId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AIGeneratedContent]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AIGeneratedContent](
	[AIContentId] [int] IDENTITY(1,1) NOT NULL,
	[AIModelId] [int] NOT NULL,
	[CourseId] [int] NULL,
	[GeneratedContent] [nvarchar](max) NOT NULL,
	[GeneratedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AIContentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AIInteractions]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AIInteractions](
	[InteractionId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[AIModelId] [int] NOT NULL,
	[UserMessage] [nvarchar](max) NOT NULL,
	[AIResponse] [nvarchar](max) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[InteractionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AIModels]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AIModels](
	[AIModelId] [int] IDENTITY(1,1) NOT NULL,
	[ModelName] [nvarchar](255) NOT NULL,
	[IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AIModelId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CourseEnrollments]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CourseEnrollments](
	[EnrollmentId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[CourseId] [int] NOT NULL,
	[EnrolledAt] [datetime2](7) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[FinalGrade] [decimal](5, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[EnrollmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Courses]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Courses](
	[CourseId] [int] IDENTITY(1,1) NOT NULL,
	[InstructorId] [int] NOT NULL,
	[AcademicTermId] [int] NOT NULL,
	[CourseCode] [nvarchar](50) NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[CreditHours] [int] NOT NULL,
	[MaxEnrollment] [int] NOT NULL,
	[CurrentEnrollment] [int] NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[CourseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[GradeBooks]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GradeBooks](
	[GradeBookId] [int] IDENTITY(1,1) NOT NULL,
	[CourseId] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[GradeBookId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Grades]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Grades](
	[GradeId] [int] IDENTITY(1,1) NOT NULL,
	[GradeBookId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[GradableItemType] [nvarchar](50) NOT NULL,
	[GradableItemId] [int] NOT NULL,
	[Points] [decimal](5, 2) NOT NULL,
	[MaxPoints] [decimal](5, 2) NOT NULL,
	[Percentage]  AS (case when [MaxPoints]>(0) then ([Points]/[MaxPoints])*(100) else (0) end) PERSISTED,
	[Comments] [nvarchar](max) NULL,
	[GradedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[GradeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Institutions]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Institutions](
	[InstitutionId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[InstitutionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InstructorProfiles]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InstructorProfiles](
	[InstructorProfileId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Department] [nvarchar](100) NULL,
	[Bio] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[InstructorProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[LearningAssets]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LearningAssets](
	[AssetId] [int] IDENTITY(1,1) NOT NULL,
	[LectureId] [int] NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[AssetType] [nvarchar](50) NOT NULL,
	[FileUrl] [nvarchar](max) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[AssetId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Lectures]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Lectures](
	[LectureId] [int] IDENTITY(1,1) NOT NULL,
	[CourseId] [int] NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[OrderIndex] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[LectureId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notifications]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notifications](
	[NotificationId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[Message] [nvarchar](max) NOT NULL,
	[IsRead] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[NotificationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NotificationTemplates]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NotificationTemplates](
	[TemplateId] [int] IDENTITY(1,1) NOT NULL,
	[NotificationTypeId] [int] NOT NULL,
	[TemplateName] [nvarchar](100) NOT NULL,
	[MessageFormat] [nvarchar](max) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[NotificationTypes]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NotificationTypes](
	[NotificationTypeId] [int] IDENTITY(1,1) NOT NULL,
	[TypeName] [nvarchar](100) NOT NULL,
	[IsActive] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[NotificationTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuestionOptions]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuestionOptions](
	[OptionId] [int] IDENTITY(1,1) NOT NULL,
	[QuestionId] [int] NOT NULL,
	[OptionText] [nvarchar](max) NOT NULL,
	[IsCorrect] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[OptionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuizAttempts]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuizAttempts](
	[AttemptId] [int] IDENTITY(1,1) NOT NULL,
	[QuizId] [int] NOT NULL,
	[UserId] [int] NOT NULL,
	[StartedAt] [datetime2](7) NOT NULL,
	[SubmittedAt] [datetime2](7) NULL,
	[Score] [decimal](5, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[AttemptId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuizQuestions]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuizQuestions](
	[QuestionId] [int] IDENTITY(1,1) NOT NULL,
	[QuizId] [int] NOT NULL,
	[QuestionText] [nvarchar](max) NOT NULL,
	[QuestionType] [nvarchar](50) NOT NULL,
	[Points] [decimal](5, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[QuestionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QuizResponses]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QuizResponses](
	[ResponseId] [int] IDENTITY(1,1) NOT NULL,
	[AttemptId] [int] NOT NULL,
	[QuestionId] [int] NOT NULL,
	[SelectedOptionId] [int] NULL,
	[ResponseText] [nvarchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[ResponseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Quizzes]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Quizzes](
	[QuizId] [int] IDENTITY(1,1) NOT NULL,
	[CourseId] [int] NOT NULL,
	[Title] [nvarchar](255) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[DueDate] [datetime2](7) NULL,
	[TimeLimitMinutes] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[QuizId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StudentProfiles]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StudentProfiles](
	[StudentProfileId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[StudentIdNumber] [nvarchar](50) NULL,
	[AdmissionDate] [date] NULL,
	[GPA] [decimal](4, 3) NULL,
PRIMARY KEY CLUSTERED 
(
	[StudentProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[TranscriptEntries]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TranscriptEntries](
	[TranscriptEntryId] [int] IDENTITY(1,1) NOT NULL,
	[TranscriptId] [int] NOT NULL,
	[CourseId] [int] NOT NULL,
	[GradeId] [int] NOT NULL,
	[LetterGrade] [nvarchar](2) NOT NULL,
	[NumericGrade] [decimal](5, 2) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[TranscriptEntryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Transcripts]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Transcripts](
	[TranscriptId] [int] IDENTITY(1,1) NOT NULL,
	[StudentProfileId] [int] NOT NULL,
	[GeneratedAt] [datetime2](7) NOT NULL,
	[CumulativeGPA] [decimal](4, 3) NULL,
	[TotalCredits] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[TranscriptId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[InstitutionId] [int] NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[PasswordHash] [nvarchar](max) NOT NULL,
	[FirstName] [nvarchar](100) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[Role] [nvarchar](50) NOT NULL,
	[IsActive] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
	[LastLoginAt] [datetime2](7) NULL,
PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[vw_CourseDetails]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_CourseDetails]
AS
SELECT 
    c.CourseId,
    c.Title AS CourseTitle,
    c.CourseCode,
    c.Description,
    u.FirstName + ' ' + u.LastName AS InstructorName,
    u.Email AS InstructorEmail,
    at.TermName,
    at.StartDate,
    at.EndDate,
    i.Name AS InstitutionName
FROM Courses c
JOIN Users u ON c.InstructorId = u.UserId
JOIN AcademicTerms at ON c.AcademicTermId = at.AcademicTermId
JOIN Institutions i ON at.InstitutionId = i.InstitutionId;
GO
/****** Object:  View [dbo].[vw_InstructorsWithCourses]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_InstructorsWithCourses] AS
SELECT
    u.UserId,
    u.FirstName,
    u.LastName,
    u.Email,
    ip.Department,
    c.CourseId,
    c.Title AS CourseTitle,
    c.CourseCode
FROM Users u
JOIN InstructorProfiles ip ON u.UserId = ip.UserId
LEFT JOIN Courses c ON u.UserId = c.InstructorId;
GO
/****** Object:  View [dbo].[vw_StudentCourseGrades]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_StudentCourseGrades]
AS
SELECT
    g.GradeId,
    u.UserId,
    u.FirstName,
    u.LastName,
    c.CourseId,
    c.Title AS CourseTitle,
    g.GradableItemType,
    CASE 
        WHEN g.GradableItemType = 'Quiz' THEN q.Title
        ELSE 'Assignment' -- Placeholder for future assignment types
    END AS ItemTitle,
    g.Points,
    g.MaxPoints,
    g.Percentage,
    g.GradedAt,
    g.Comments
FROM Grades g
JOIN Users u ON g.UserId = u.UserId
JOIN GradeBooks gb ON g.GradeBookId = gb.GradeBookId
JOIN Courses c ON gb.CourseId = c.CourseId
LEFT JOIN QuizAttempts qa ON g.GradableItemId = qa.AttemptId AND g.GradableItemType = 'Quiz'
LEFT JOIN Quizzes q ON qa.QuizId = q.QuizId;
GO
/****** Object:  View [dbo].[vw_StudentEnrollments]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_StudentEnrollments]
AS
SELECT 
    ce.EnrollmentId,
    u.UserId,
    u.FirstName,
    u.LastName,
    u.Email,
    c.CourseId,
    c.Title AS CourseTitle,
    c.CourseCode,
    ce.EnrolledAt,
    ce.Status
FROM CourseEnrollments ce
JOIN Users u ON ce.UserId = u.UserId
JOIN Courses c ON ce.CourseId = c.CourseId
WHERE u.Role = 'Student';
GO
/****** Object:  View [dbo].[vw_StudentsWithGPAAndCourses]    Script Date: 10/15/2025 1:42:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_StudentsWithGPAAndCourses] AS
SELECT 
    u.UserId,
    u.FirstName,
    u.LastName,
    u.Email,
    sp.GPA,
    COUNT(ce.CourseId) AS EnrolledCoursesCount
FROM Users u
JOIN StudentProfiles sp ON u.UserId = sp.UserId
LEFT JOIN CourseEnrollments ce ON u.UserId = ce.UserId AND ce.Status = 'Enrolled'
WHERE u.Role = 'Student'
GROUP BY u.UserId, u.FirstName, u.LastName, u.Email, sp.GPA;
GO
SET IDENTITY_INSERT [dbo].[AcademicTerms] ON 

INSERT [dbo].[AcademicTerms] ([AcademicTermId], [InstitutionId], [TermName], [StartDate], [EndDate], [IsActive]) VALUES (1, 1, N'Fall 2024', CAST(N'2024-09-01' AS Date), CAST(N'2024-12-20' AS Date), 0)
INSERT [dbo].[AcademicTerms] ([AcademicTermId], [InstitutionId], [TermName], [StartDate], [EndDate], [IsActive]) VALUES (2, 1, N'Spring 2025', CAST(N'2025-01-15' AS Date), CAST(N'2025-05-10' AS Date), 0)
INSERT [dbo].[AcademicTerms] ([AcademicTermId], [InstitutionId], [TermName], [StartDate], [EndDate], [IsActive]) VALUES (3, 1, N'Fall 2025', CAST(N'2025-09-01' AS Date), CAST(N'2025-12-20' AS Date), 1)
INSERT [dbo].[AcademicTerms] ([AcademicTermId], [InstitutionId], [TermName], [StartDate], [EndDate], [IsActive]) VALUES (4, 2, N'Fall 2024', CAST(N'2024-08-20' AS Date), CAST(N'2024-12-15' AS Date), 0)
INSERT [dbo].[AcademicTerms] ([AcademicTermId], [InstitutionId], [TermName], [StartDate], [EndDate], [IsActive]) VALUES (5, 2, N'Spring 2025', CAST(N'2025-01-10' AS Date), CAST(N'2025-05-05' AS Date), 0)
INSERT [dbo].[AcademicTerms] ([AcademicTermId], [InstitutionId], [TermName], [StartDate], [EndDate], [IsActive]) VALUES (6, 2, N'Fall 2025', CAST(N'2025-08-20' AS Date), CAST(N'2025-12-15' AS Date), 1)
SET IDENTITY_INSERT [dbo].[AcademicTerms] OFF
GO
SET IDENTITY_INSERT [dbo].[AIModels] ON 

INSERT [dbo].[AIModels] ([AIModelId], [ModelName], [IsActive]) VALUES (1, N'GPT-4 Turbo', 1)
INSERT [dbo].[AIModels] ([AIModelId], [ModelName], [IsActive]) VALUES (2, N'Gemini Pro', 1)
SET IDENTITY_INSERT [dbo].[AIModels] OFF
GO
SET IDENTITY_INSERT [dbo].[CourseEnrollments] ON 

INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (1, 7, 1, CAST(N'2025-09-16T17:21:19.0300000' AS DateTime2), N'Enrolled', NULL)
INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (2, 7, 2, CAST(N'2025-09-16T17:21:19.0300000' AS DateTime2), N'Enrolled', NULL)
INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (3, 8, 1, CAST(N'2025-09-16T17:21:19.0300000' AS DateTime2), N'Enrolled', NULL)
INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (4, 9, 2, CAST(N'2025-09-16T17:21:19.0300000' AS DateTime2), N'Enrolled', NULL)
INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (5, 10, 1, CAST(N'2025-09-16T17:21:19.0300000' AS DateTime2), N'Enrolled', NULL)
INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (6, 10, 2, CAST(N'2025-09-16T17:21:19.0300000' AS DateTime2), N'Completed', CAST(95.00 AS Decimal(5, 2)))
INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (7, 11, 1, CAST(N'2025-09-16T17:21:19.0300000' AS DateTime2), N'Withdrawn', NULL)
INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (8, 12, 3, CAST(N'2025-09-16T17:21:19.0333333' AS DateTime2), N'Enrolled', NULL)
INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (9, 12, 4, CAST(N'2025-09-16T17:21:19.0333333' AS DateTime2), N'Enrolled', NULL)
INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (10, 13, 4, CAST(N'2025-09-16T17:21:19.0333333' AS DateTime2), N'Enrolled', NULL)
INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (11, 14, 3, CAST(N'2025-09-16T17:21:19.0333333' AS DateTime2), N'Enrolled', NULL)
INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (12, 15, 3, CAST(N'2025-09-16T17:21:19.0333333' AS DateTime2), N'Enrolled', NULL)
INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (13, 15, 4, CAST(N'2025-09-16T17:21:19.0333333' AS DateTime2), N'Completed', CAST(88.00 AS Decimal(5, 2)))
INSERT [dbo].[CourseEnrollments] ([EnrollmentId], [UserId], [CourseId], [EnrolledAt], [Status], [FinalGrade]) VALUES (14, 16, 4, CAST(N'2025-09-16T17:21:19.0333333' AS DateTime2), N'Enrolled', NULL)
SET IDENTITY_INSERT [dbo].[CourseEnrollments] OFF
GO
SET IDENTITY_INSERT [dbo].[Courses] ON 

INSERT [dbo].[Courses] ([CourseId], [InstructorId], [AcademicTermId], [CourseCode], [Title], [Description], [CreditHours], [MaxEnrollment], [CurrentEnrollment], [Status], [CreatedAt], [UpdatedAt]) VALUES (1, 3, 3, N'CS101', N'Introduction to Programming', N'Learn the fundamentals of programming using Python.', 3, 50, 0, N'Active', CAST(N'2025-09-16T17:21:19.0266667' AS DateTime2), NULL)
INSERT [dbo].[Courses] ([CourseId], [InstructorId], [AcademicTermId], [CourseCode], [Title], [Description], [CreditHours], [MaxEnrollment], [CurrentEnrollment], [Status], [CreatedAt], [UpdatedAt]) VALUES (2, 4, 3, N'SEC201', N'Network Security Basics', N'An introduction to network vulnerabilities and defense.', 3, 50, 0, N'Active', CAST(N'2025-09-16T17:21:19.0266667' AS DateTime2), NULL)
INSERT [dbo].[Courses] ([CourseId], [InstructorId], [AcademicTermId], [CourseCode], [Title], [Description], [CreditHours], [MaxEnrollment], [CurrentEnrollment], [Status], [CreatedAt], [UpdatedAt]) VALUES (3, 5, 6, N'ART101', N'History of Art', N'Survey of major art movements.', 3, 50, 0, N'Active', CAST(N'2025-09-16T17:21:19.0266667' AS DateTime2), NULL)
INSERT [dbo].[Courses] ([CourseId], [InstructorId], [AcademicTermId], [CourseCode], [Title], [Description], [CreditHours], [MaxEnrollment], [CurrentEnrollment], [Status], [CreatedAt], [UpdatedAt]) VALUES (4, 6, 6, N'LIT202', N'Modern Poetry', N'Exploring poetry from the 20th century.', 3, 50, 0, N'Active', CAST(N'2025-09-16T17:21:19.0266667' AS DateTime2), NULL)
SET IDENTITY_INSERT [dbo].[Courses] OFF
GO
SET IDENTITY_INSERT [dbo].[GradeBooks] ON 

INSERT [dbo].[GradeBooks] ([GradeBookId], [CourseId], [Name], [CreatedAt]) VALUES (1, 1, N'Intro To Programming', CAST(N'2025-09-16T17:21:19.0533333' AS DateTime2))
SET IDENTITY_INSERT [dbo].[GradeBooks] OFF
GO
SET IDENTITY_INSERT [dbo].[Grades] ON 

INSERT [dbo].[Grades] ([GradeId], [GradeBookId], [UserId], [GradableItemType], [GradableItemId], [Points], [MaxPoints], [Comments], [GradedAt]) VALUES (1, 1, 7, N'Quiz', 1, CAST(10.00 AS Decimal(5, 2)), CAST(20.00 AS Decimal(5, 2)), N'Good job on the multiple choice.', CAST(N'2025-09-16T17:21:19.0600000' AS DateTime2))
INSERT [dbo].[Grades] ([GradeId], [GradeBookId], [UserId], [GradableItemType], [GradableItemId], [Points], [MaxPoints], [Comments], [GradedAt]) VALUES (2, 1, 8, N'Quiz', 2, CAST(0.00 AS Decimal(5, 2)), CAST(20.00 AS Decimal(5, 2)), N'Please review the material on data types.', CAST(N'2025-09-16T17:21:19.0600000' AS DateTime2))
INSERT [dbo].[Grades] ([GradeId], [GradeBookId], [UserId], [GradableItemType], [GradableItemId], [Points], [MaxPoints], [Comments], [GradedAt]) VALUES (3, 1, 10, N'Assignment', 1, CAST(95.00 AS Decimal(5, 2)), CAST(100.00 AS Decimal(5, 2)), N'Excellent work on the final project.', CAST(N'2025-09-16T17:21:19.0600000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[Grades] OFF
GO
SET IDENTITY_INSERT [dbo].[Institutions] ON 

INSERT [dbo].[Institutions] ([InstitutionId], [Name], [IsActive], [CreatedAt]) VALUES (1, N'Future Tech University', 1, CAST(N'2025-09-16T17:21:19.0000000' AS DateTime2))
INSERT [dbo].[Institutions] ([InstitutionId], [Name], [IsActive], [CreatedAt]) VALUES (2, N'Global Arts College', 1, CAST(N'2025-09-16T17:21:19.0000000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[Institutions] OFF
GO
SET IDENTITY_INSERT [dbo].[InstructorProfiles] ON 

INSERT [dbo].[InstructorProfiles] ([InstructorProfileId], [UserId], [Department], [Bio]) VALUES (1, 3, N'Computer Science', N'Expert in AI and Machine Learning.')
INSERT [dbo].[InstructorProfiles] ([InstructorProfileId], [UserId], [Department], [Bio]) VALUES (2, 4, N'Cybersecurity', N'Focuses on network security and ethical hacking.')
INSERT [dbo].[InstructorProfiles] ([InstructorProfileId], [UserId], [Department], [Bio]) VALUES (3, 5, N'Fine Arts', N'Specializes in classical painting techniques.')
INSERT [dbo].[InstructorProfiles] ([InstructorProfileId], [UserId], [Department], [Bio]) VALUES (4, 6, N'Literature', N'Renowned for studies in 19th-century poetry.')
SET IDENTITY_INSERT [dbo].[InstructorProfiles] OFF
GO
SET IDENTITY_INSERT [dbo].[LearningAssets] ON 

INSERT [dbo].[LearningAssets] ([AssetId], [LectureId], [Title], [AssetType], [FileUrl]) VALUES (1, 1, N'Python Installation Guide', N'File', N'/assets/python_guide.pdf')
INSERT [dbo].[LearningAssets] ([AssetId], [LectureId], [Title], [AssetType], [FileUrl]) VALUES (2, 1, N'Data Types Video', N'Video', N'https://youtube.com/watch?v=xyz')
INSERT [dbo].[LearningAssets] ([AssetId], [LectureId], [Title], [AssetType], [FileUrl]) VALUES (3, 3, N'Firewall Configuration Example', N'File', N'/assets/firewall.txt')
SET IDENTITY_INSERT [dbo].[LearningAssets] OFF
GO
SET IDENTITY_INSERT [dbo].[Lectures] ON 

INSERT [dbo].[Lectures] ([LectureId], [CourseId], [Title], [Description], [OrderIndex]) VALUES (1, 1, N'Variables and Data Types', N'Content about variables...', 1)
INSERT [dbo].[Lectures] ([LectureId], [CourseId], [Title], [Description], [OrderIndex]) VALUES (2, 1, N'Control Flow', N'Content about loops and conditionals...', 2)
INSERT [dbo].[Lectures] ([LectureId], [CourseId], [Title], [Description], [OrderIndex]) VALUES (3, 2, N'Intro to Firewalls', N'Content about firewalls...', 1)
SET IDENTITY_INSERT [dbo].[Lectures] OFF
GO
SET IDENTITY_INSERT [dbo].[Notifications] ON 

INSERT [dbo].[Notifications] ([NotificationId], [UserId], [Message], [IsRead], [CreatedAt]) VALUES (1, 7, N'Your grade for Quiz 1 has been posted.', 0, CAST(N'2025-09-16T17:21:19.0600000' AS DateTime2))
SET IDENTITY_INSERT [dbo].[Notifications] OFF
GO
SET IDENTITY_INSERT [dbo].[NotificationTypes] ON 

INSERT [dbo].[NotificationTypes] ([NotificationTypeId], [TypeName], [IsActive]) VALUES (1, N'New Grade', 1)
INSERT [dbo].[NotificationTypes] ([NotificationTypeId], [TypeName], [IsActive]) VALUES (2, N'Course Announcement', 1)
SET IDENTITY_INSERT [dbo].[NotificationTypes] OFF
GO
SET IDENTITY_INSERT [dbo].[QuestionOptions] ON 

INSERT [dbo].[QuestionOptions] ([OptionId], [QuestionId], [OptionText], [IsCorrect]) VALUES (1, 2, N'3.14', 0)
INSERT [dbo].[QuestionOptions] ([OptionId], [QuestionId], [OptionText], [IsCorrect]) VALUES (2, 2, N'"Hello"', 0)
INSERT [dbo].[QuestionOptions] ([OptionId], [QuestionId], [OptionText], [IsCorrect]) VALUES (3, 2, N'42', 1)
INSERT [dbo].[QuestionOptions] ([OptionId], [QuestionId], [OptionText], [IsCorrect]) VALUES (4, 2, N'True', 0)
INSERT [dbo].[QuestionOptions] ([OptionId], [QuestionId], [OptionText], [IsCorrect]) VALUES (5, 3, N'Confidentiality, Integrity, Availability', 1)
INSERT [dbo].[QuestionOptions] ([OptionId], [QuestionId], [OptionText], [IsCorrect]) VALUES (6, 3, N'Central Intelligence Agency', 0)
INSERT [dbo].[QuestionOptions] ([OptionId], [QuestionId], [OptionText], [IsCorrect]) VALUES (7, 3, N'Cybersecurity Incident Analysis', 0)
SET IDENTITY_INSERT [dbo].[QuestionOptions] OFF
GO
SET IDENTITY_INSERT [dbo].[QuizAttempts] ON 

INSERT [dbo].[QuizAttempts] ([AttemptId], [QuizId], [UserId], [StartedAt], [SubmittedAt], [Score]) VALUES (1, 1, 7, CAST(N'2025-09-16T17:21:19.0500000' AS DateTime2), CAST(N'2025-09-16T17:21:19.0500000' AS DateTime2), CAST(10.00 AS Decimal(5, 2)))
INSERT [dbo].[QuizAttempts] ([AttemptId], [QuizId], [UserId], [StartedAt], [SubmittedAt], [Score]) VALUES (2, 1, 8, CAST(N'2025-09-16T17:21:19.0500000' AS DateTime2), CAST(N'2025-09-16T17:21:19.0500000' AS DateTime2), CAST(0.00 AS Decimal(5, 2)))
INSERT [dbo].[QuizAttempts] ([AttemptId], [QuizId], [UserId], [StartedAt], [SubmittedAt], [Score]) VALUES (3, 2, 7, CAST(N'2025-09-16T17:21:19.0500000' AS DateTime2), CAST(N'2025-09-16T17:21:19.0500000' AS DateTime2), CAST(10.00 AS Decimal(5, 2)))
SET IDENTITY_INSERT [dbo].[QuizAttempts] OFF
GO
SET IDENTITY_INSERT [dbo].[QuizQuestions] ON 

INSERT [dbo].[QuizQuestions] ([QuestionId], [QuizId], [QuestionText], [QuestionType], [Points]) VALUES (1, 1, N'What keyword is used to declare a variable in Python?', N'ShortAnswer', CAST(10.00 AS Decimal(5, 2)))
INSERT [dbo].[QuizQuestions] ([QuestionId], [QuizId], [QuestionText], [QuestionType], [Points]) VALUES (2, 1, N'Which of the following is an integer data type?', N'MultipleChoice', CAST(10.00 AS Decimal(5, 2)))
INSERT [dbo].[QuizQuestions] ([QuestionId], [QuizId], [QuestionText], [QuestionType], [Points]) VALUES (3, 2, N'What does CIA stand for in cybersecurity?', N'MultipleChoice', CAST(10.00 AS Decimal(5, 2)))
SET IDENTITY_INSERT [dbo].[QuizQuestions] OFF
GO
SET IDENTITY_INSERT [dbo].[QuizResponses] ON 

INSERT [dbo].[QuizResponses] ([ResponseId], [AttemptId], [QuestionId], [SelectedOptionId], [ResponseText]) VALUES (1, 1, 1, NULL, N'No keyword is needed')
INSERT [dbo].[QuizResponses] ([ResponseId], [AttemptId], [QuestionId], [SelectedOptionId], [ResponseText]) VALUES (2, 1, 2, 3, NULL)
INSERT [dbo].[QuizResponses] ([ResponseId], [AttemptId], [QuestionId], [SelectedOptionId], [ResponseText]) VALUES (3, 2, 2, 1, NULL)
INSERT [dbo].[QuizResponses] ([ResponseId], [AttemptId], [QuestionId], [SelectedOptionId], [ResponseText]) VALUES (4, 3, 3, 5, NULL)
SET IDENTITY_INSERT [dbo].[QuizResponses] OFF
GO
SET IDENTITY_INSERT [dbo].[Quizzes] ON 

INSERT [dbo].[Quizzes] ([QuizId], [CourseId], [Title], [Description], [DueDate], [TimeLimitMinutes]) VALUES (1, 1, N'Quiz 1: Python Basics', N'Test your knowledge of variables and types.', CAST(N'2025-10-01T23:59:00.0000000' AS DateTime2), NULL)
INSERT [dbo].[Quizzes] ([QuizId], [CourseId], [Title], [Description], [DueDate], [TimeLimitMinutes]) VALUES (2, 2, N'Quiz 1: Security Concepts', N'Test your knowledge of basic security concepts.', CAST(N'2025-10-05T23:59:00.0000000' AS DateTime2), NULL)
SET IDENTITY_INSERT [dbo].[Quizzes] OFF
GO
SET IDENTITY_INSERT [dbo].[StudentProfiles] ON 

INSERT [dbo].[StudentProfiles] ([StudentProfileId], [UserId], [StudentIdNumber], [AdmissionDate], [GPA]) VALUES (1, 7, N'FTU2025001', CAST(N'2024-09-01' AS Date), CAST(3.800 AS Decimal(4, 3)))
INSERT [dbo].[StudentProfiles] ([StudentProfileId], [UserId], [StudentIdNumber], [AdmissionDate], [GPA]) VALUES (2, 8, N'FTU2025002', CAST(N'2024-09-01' AS Date), CAST(3.500 AS Decimal(4, 3)))
INSERT [dbo].[StudentProfiles] ([StudentProfileId], [UserId], [StudentIdNumber], [AdmissionDate], [GPA]) VALUES (3, 9, N'FTU2025003', CAST(N'2024-09-01' AS Date), CAST(3.200 AS Decimal(4, 3)))
INSERT [dbo].[StudentProfiles] ([StudentProfileId], [UserId], [StudentIdNumber], [AdmissionDate], [GPA]) VALUES (4, 10, N'FTU2025004', CAST(N'2024-09-01' AS Date), CAST(4.000 AS Decimal(4, 3)))
INSERT [dbo].[StudentProfiles] ([StudentProfileId], [UserId], [StudentIdNumber], [AdmissionDate], [GPA]) VALUES (5, 11, N'FTU2025005', CAST(N'2024-09-01' AS Date), CAST(2.900 AS Decimal(4, 3)))
INSERT [dbo].[StudentProfiles] ([StudentProfileId], [UserId], [StudentIdNumber], [AdmissionDate], [GPA]) VALUES (6, 12, N'GAC2025001', CAST(N'2024-08-15' AS Date), CAST(3.700 AS Decimal(4, 3)))
INSERT [dbo].[StudentProfiles] ([StudentProfileId], [UserId], [StudentIdNumber], [AdmissionDate], [GPA]) VALUES (7, 13, N'GAC2025002', CAST(N'2024-08-15' AS Date), CAST(3.900 AS Decimal(4, 3)))
INSERT [dbo].[StudentProfiles] ([StudentProfileId], [UserId], [StudentIdNumber], [AdmissionDate], [GPA]) VALUES (8, 14, N'GAC2025003', CAST(N'2024-08-15' AS Date), CAST(3.100 AS Decimal(4, 3)))
INSERT [dbo].[StudentProfiles] ([StudentProfileId], [UserId], [StudentIdNumber], [AdmissionDate], [GPA]) VALUES (9, 15, N'GAC2025004', CAST(N'2024-08-15' AS Date), CAST(3.600 AS Decimal(4, 3)))
INSERT [dbo].[StudentProfiles] ([StudentProfileId], [UserId], [StudentIdNumber], [AdmissionDate], [GPA]) VALUES (10, 16, N'GAC2025005', CAST(N'2024-08-15' AS Date), CAST(3.300 AS Decimal(4, 3)))
SET IDENTITY_INSERT [dbo].[StudentProfiles] OFF
GO
SET IDENTITY_INSERT [dbo].[Users] ON 

INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (1, 1, N'admin.ftu@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Admin', N'FTU', N'Admin', 1, CAST(N'2025-09-16T17:21:19.0033333' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (2, 2, N'admin.gac@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Admin', N'GAC', N'Admin', 1, CAST(N'2025-09-16T17:21:19.0033333' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (3, 1, N'john.doe@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'John', N'Doe', N'Instructor', 1, CAST(N'2025-09-16T17:21:19.0066667' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (4, 1, N'jane.smith@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Jane', N'Smith', N'Instructor', 1, CAST(N'2025-09-16T17:21:19.0066667' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (5, 2, N'peter.jones@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Peter', N'Jones', N'Instructor', 1, CAST(N'2025-09-16T17:21:19.0066667' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (6, 2, N'mary.williams@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Mary', N'Williams', N'Instructor', 1, CAST(N'2025-09-16T17:21:19.0066667' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (7, 1, N'alice.johnson@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Alice', N'Johnson', N'Student', 1, CAST(N'2025-09-16T17:21:19.0133333' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (8, 1, N'bob.brown@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Bob', N'Brown', N'Student', 1, CAST(N'2025-09-16T17:21:19.0133333' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (9, 1, N'charlie.davis@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Charlie', N'Davis', N'Student', 1, CAST(N'2025-09-16T17:21:19.0133333' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (10, 1, N'diana.miller@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Diana', N'Miller', N'Student', 1, CAST(N'2025-09-16T17:21:19.0133333' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (11, 1, N'eve.wilson@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Eve', N'Wilson', N'Student', 1, CAST(N'2025-09-16T17:21:19.0133333' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (12, 2, N'frank.moore@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Frank', N'Moore', N'Student', 1, CAST(N'2025-09-16T17:21:19.0133333' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (13, 2, N'grace.taylor@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Grace', N'Taylor', N'Student', 1, CAST(N'2025-09-16T17:21:19.0133333' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (14, 2, N'heidi.anderson@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Heidi', N'Anderson', N'Student', 1, CAST(N'2025-09-16T17:21:19.0133333' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (15, 2, N'ivan.thomas@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Ivan', N'Thomas', N'Student', 1, CAST(N'2025-09-16T17:21:19.0133333' AS DateTime2), NULL, NULL)
INSERT [dbo].[Users] ([UserId], [InstitutionId], [Email], [PasswordHash], [FirstName], [LastName], [Role], [IsActive], [CreatedAt], [UpdatedAt], [LastLoginAt]) VALUES (16, 2, N'judy.jackson@example.com', N'AQAAAAEAACcQAAAAEI.../placeholder.../hash', N'Judy', N'Jackson', N'Student', 1, CAST(N'2025-09-16T17:21:19.0133333' AS DateTime2), NULL, NULL)
SET IDENTITY_INSERT [dbo].[Users] OFF
GO
/****** Object:  Index [UQ_User_Course_Enrollment]    Script Date: 10/15/2025 1:42:05 PM ******/
ALTER TABLE [dbo].[CourseEnrollments] ADD  CONSTRAINT [UQ_User_Course_Enrollment] UNIQUE NONCLUSTERED 
(
	[UserId] ASC,
	[CourseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [UQ__GradeBoo__C92D71A6E792C3E7]    Script Date: 10/15/2025 1:42:05 PM ******/
ALTER TABLE [dbo].[GradeBooks] ADD UNIQUE NONCLUSTERED 
(
	[CourseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [UQ__Instruct__1788CC4D10CEAA8F]    Script Date: 10/15/2025 1:42:05 PM ******/
ALTER TABLE [dbo].[InstructorProfiles] ADD UNIQUE NONCLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Notifica__D4E7DFA856526EA8]    Script Date: 10/15/2025 1:42:05 PM ******/
ALTER TABLE [dbo].[NotificationTypes] ADD UNIQUE NONCLUSTERED 
(
	[TypeName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [UQ__StudentP__1788CC4DCAB85C03]    Script Date: 10/15/2025 1:42:05 PM ******/
ALTER TABLE [dbo].[StudentProfiles] ADD UNIQUE NONCLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__StudentP__EDCE3FE405519064]    Script Date: 10/15/2025 1:42:05 PM ******/
ALTER TABLE [dbo].[StudentProfiles] ADD UNIQUE NONCLUSTERED 
(
	[StudentIdNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Users__A9D105345A006376]    Script Date: 10/15/2025 1:42:05 PM ******/
ALTER TABLE [dbo].[Users] ADD UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AcademicTerms] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[AIGeneratedContent] ADD  DEFAULT (getdate()) FOR [GeneratedAt]
GO
ALTER TABLE [dbo].[AIInteractions] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[AIModels] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[CourseEnrollments] ADD  DEFAULT (getdate()) FOR [EnrolledAt]
GO
ALTER TABLE [dbo].[Courses] ADD  DEFAULT ((3)) FOR [CreditHours]
GO
ALTER TABLE [dbo].[Courses] ADD  DEFAULT ((50)) FOR [MaxEnrollment]
GO
ALTER TABLE [dbo].[Courses] ADD  DEFAULT ((0)) FOR [CurrentEnrollment]
GO
ALTER TABLE [dbo].[Courses] ADD  DEFAULT ('Active') FOR [Status]
GO
ALTER TABLE [dbo].[Courses] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[GradeBooks] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Grades] ADD  DEFAULT (getdate()) FOR [GradedAt]
GO
ALTER TABLE [dbo].[Institutions] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Institutions] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Lectures] ADD  DEFAULT ((0)) FOR [OrderIndex]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT ((0)) FOR [IsRead]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[NotificationTypes] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[QuestionOptions] ADD  DEFAULT ((0)) FOR [IsCorrect]
GO
ALTER TABLE [dbo].[QuizAttempts] ADD  DEFAULT (getdate()) FOR [StartedAt]
GO
ALTER TABLE [dbo].[QuizQuestions] ADD  DEFAULT ((10.0)) FOR [Points]
GO
ALTER TABLE [dbo].[Transcripts] ADD  DEFAULT (getdate()) FOR [GeneratedAt]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[AcademicTerms]  WITH CHECK ADD  CONSTRAINT [FK_AcademicTerms_InstitutionId] FOREIGN KEY([InstitutionId])
REFERENCES [dbo].[Institutions] ([InstitutionId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AcademicTerms] CHECK CONSTRAINT [FK_AcademicTerms_InstitutionId]
GO
ALTER TABLE [dbo].[AIGeneratedContent]  WITH CHECK ADD  CONSTRAINT [FK_AIGeneratedContent_AIModelId] FOREIGN KEY([AIModelId])
REFERENCES [dbo].[AIModels] ([AIModelId])
GO
ALTER TABLE [dbo].[AIGeneratedContent] CHECK CONSTRAINT [FK_AIGeneratedContent_AIModelId]
GO
ALTER TABLE [dbo].[AIInteractions]  WITH CHECK ADD  CONSTRAINT [FK_AIInteractions_AIModelId] FOREIGN KEY([AIModelId])
REFERENCES [dbo].[AIModels] ([AIModelId])
GO
ALTER TABLE [dbo].[AIInteractions] CHECK CONSTRAINT [FK_AIInteractions_AIModelId]
GO
ALTER TABLE [dbo].[AIInteractions]  WITH CHECK ADD  CONSTRAINT [FK_AIInteractions_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[AIInteractions] CHECK CONSTRAINT [FK_AIInteractions_UserId]
GO
ALTER TABLE [dbo].[CourseEnrollments]  WITH CHECK ADD  CONSTRAINT [FK_CourseEnrollments_CourseId] FOREIGN KEY([CourseId])
REFERENCES [dbo].[Courses] ([CourseId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CourseEnrollments] CHECK CONSTRAINT [FK_CourseEnrollments_CourseId]
GO
ALTER TABLE [dbo].[CourseEnrollments]  WITH CHECK ADD  CONSTRAINT [FK_CourseEnrollments_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CourseEnrollments] CHECK CONSTRAINT [FK_CourseEnrollments_UserId]
GO
ALTER TABLE [dbo].[Courses]  WITH CHECK ADD  CONSTRAINT [FK_Courses_AcademicTermId] FOREIGN KEY([AcademicTermId])
REFERENCES [dbo].[AcademicTerms] ([AcademicTermId])
GO
ALTER TABLE [dbo].[Courses] CHECK CONSTRAINT [FK_Courses_AcademicTermId]
GO
ALTER TABLE [dbo].[Courses]  WITH CHECK ADD  CONSTRAINT [FK_Courses_InstructorId] FOREIGN KEY([InstructorId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Courses] CHECK CONSTRAINT [FK_Courses_InstructorId]
GO
ALTER TABLE [dbo].[GradeBooks]  WITH CHECK ADD  CONSTRAINT [FK_GradeBooks_CourseId] FOREIGN KEY([CourseId])
REFERENCES [dbo].[Courses] ([CourseId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[GradeBooks] CHECK CONSTRAINT [FK_GradeBooks_CourseId]
GO
ALTER TABLE [dbo].[Grades]  WITH CHECK ADD  CONSTRAINT [FK_Grades_GradeBookId] FOREIGN KEY([GradeBookId])
REFERENCES [dbo].[GradeBooks] ([GradeBookId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Grades] CHECK CONSTRAINT [FK_Grades_GradeBookId]
GO
ALTER TABLE [dbo].[Grades]  WITH CHECK ADD  CONSTRAINT [FK_Grades_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
GO
ALTER TABLE [dbo].[Grades] CHECK CONSTRAINT [FK_Grades_UserId]
GO
ALTER TABLE [dbo].[InstructorProfiles]  WITH CHECK ADD  CONSTRAINT [FK_InstructorProfiles_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[InstructorProfiles] CHECK CONSTRAINT [FK_InstructorProfiles_UserId]
GO
ALTER TABLE [dbo].[LearningAssets]  WITH CHECK ADD  CONSTRAINT [FK_LearningAssets_LessonId] FOREIGN KEY([LectureId])
REFERENCES [dbo].[Lectures] ([LectureId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LearningAssets] CHECK CONSTRAINT [FK_LearningAssets_LessonId]
GO
ALTER TABLE [dbo].[Lectures]  WITH CHECK ADD  CONSTRAINT [FK_Lessons_CourseId] FOREIGN KEY([CourseId])
REFERENCES [dbo].[Courses] ([CourseId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Lectures] CHECK CONSTRAINT [FK_Lessons_CourseId]
GO
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_Notifications_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_UserId]
GO
ALTER TABLE [dbo].[NotificationTemplates]  WITH CHECK ADD  CONSTRAINT [FK_NotificationTemplates_TypeId] FOREIGN KEY([NotificationTypeId])
REFERENCES [dbo].[NotificationTypes] ([NotificationTypeId])
GO
ALTER TABLE [dbo].[NotificationTemplates] CHECK CONSTRAINT [FK_NotificationTemplates_TypeId]
GO
ALTER TABLE [dbo].[QuestionOptions]  WITH CHECK ADD  CONSTRAINT [FK_QuestionOptions_QuestionId] FOREIGN KEY([QuestionId])
REFERENCES [dbo].[QuizQuestions] ([QuestionId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[QuestionOptions] CHECK CONSTRAINT [FK_QuestionOptions_QuestionId]
GO
ALTER TABLE [dbo].[QuizAttempts]  WITH CHECK ADD  CONSTRAINT [FK_QuizAttempts_QuizId] FOREIGN KEY([QuizId])
REFERENCES [dbo].[Quizzes] ([QuizId])
GO
ALTER TABLE [dbo].[QuizAttempts] CHECK CONSTRAINT [FK_QuizAttempts_QuizId]
GO
ALTER TABLE [dbo].[QuizAttempts]  WITH CHECK ADD  CONSTRAINT [FK_QuizAttempts_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[QuizAttempts] CHECK CONSTRAINT [FK_QuizAttempts_UserId]
GO
ALTER TABLE [dbo].[QuizQuestions]  WITH CHECK ADD  CONSTRAINT [FK_QuizQuestions_QuizId] FOREIGN KEY([QuizId])
REFERENCES [dbo].[Quizzes] ([QuizId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[QuizQuestions] CHECK CONSTRAINT [FK_QuizQuestions_QuizId]
GO
ALTER TABLE [dbo].[QuizResponses]  WITH CHECK ADD  CONSTRAINT [FK_QuizResponses_AttemptId] FOREIGN KEY([AttemptId])
REFERENCES [dbo].[QuizAttempts] ([AttemptId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[QuizResponses] CHECK CONSTRAINT [FK_QuizResponses_AttemptId]
GO
ALTER TABLE [dbo].[QuizResponses]  WITH CHECK ADD  CONSTRAINT [FK_QuizResponses_QuestionId] FOREIGN KEY([QuestionId])
REFERENCES [dbo].[QuizQuestions] ([QuestionId])
GO
ALTER TABLE [dbo].[QuizResponses] CHECK CONSTRAINT [FK_QuizResponses_QuestionId]
GO
ALTER TABLE [dbo].[QuizResponses]  WITH CHECK ADD  CONSTRAINT [FK_QuizResponses_SelectedOptionId] FOREIGN KEY([SelectedOptionId])
REFERENCES [dbo].[QuestionOptions] ([OptionId])
GO
ALTER TABLE [dbo].[QuizResponses] CHECK CONSTRAINT [FK_QuizResponses_SelectedOptionId]
GO
ALTER TABLE [dbo].[Quizzes]  WITH CHECK ADD  CONSTRAINT [FK_Quizzes_CourseId] FOREIGN KEY([CourseId])
REFERENCES [dbo].[Courses] ([CourseId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Quizzes] CHECK CONSTRAINT [FK_Quizzes_CourseId]
GO
ALTER TABLE [dbo].[StudentProfiles]  WITH CHECK ADD  CONSTRAINT [FK_StudentProfiles_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([UserId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[StudentProfiles] CHECK CONSTRAINT [FK_StudentProfiles_UserId]
GO
ALTER TABLE [dbo].[TranscriptEntries]  WITH CHECK ADD  CONSTRAINT [FK_TranscriptEntries_CourseId] FOREIGN KEY([CourseId])
REFERENCES [dbo].[Courses] ([CourseId])
GO
ALTER TABLE [dbo].[TranscriptEntries] CHECK CONSTRAINT [FK_TranscriptEntries_CourseId]
GO
ALTER TABLE [dbo].[TranscriptEntries]  WITH CHECK ADD  CONSTRAINT [FK_TranscriptEntries_GradeId] FOREIGN KEY([GradeId])
REFERENCES [dbo].[Grades] ([GradeId])
GO
ALTER TABLE [dbo].[TranscriptEntries] CHECK CONSTRAINT [FK_TranscriptEntries_GradeId]
GO
ALTER TABLE [dbo].[TranscriptEntries]  WITH CHECK ADD  CONSTRAINT [FK_TranscriptEntries_TranscriptId] FOREIGN KEY([TranscriptId])
REFERENCES [dbo].[Transcripts] ([TranscriptId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[TranscriptEntries] CHECK CONSTRAINT [FK_TranscriptEntries_TranscriptId]
GO
ALTER TABLE [dbo].[Transcripts]  WITH CHECK ADD  CONSTRAINT [FK_Transcripts_StudentProfileId] FOREIGN KEY([StudentProfileId])
REFERENCES [dbo].[StudentProfiles] ([StudentProfileId])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Transcripts] CHECK CONSTRAINT [FK_Transcripts_StudentProfileId]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_InstitutionId] FOREIGN KEY([InstitutionId])
REFERENCES [dbo].[Institutions] ([InstitutionId])
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_InstitutionId]
GO
ALTER TABLE [dbo].[CourseEnrollments]  WITH CHECK ADD CHECK  (([Status]='Completed' OR [Status]='Withdrawn' OR [Status]='Enrolled'))
GO
ALTER TABLE [dbo].[Courses]  WITH CHECK ADD CHECK  (([Status]='Archived' OR [Status]='Cancelled' OR [Status]='Completed' OR [Status]='Active'))
GO
ALTER TABLE [dbo].[LearningAssets]  WITH CHECK ADD CHECK  (([AssetType]='Video' OR [AssetType]='URL' OR [AssetType]='File'))
GO
ALTER TABLE [dbo].[QuizQuestions]  WITH CHECK ADD CHECK  (([QuestionType]='ShortAnswer' OR [QuestionType]='TrueFalse' OR [QuestionType]='MultipleChoice'))
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD CHECK  (([Role]='Student' OR [Role]='Instructor' OR [Role]='Admin'))
GO
ALTER DATABASE [LMS] SET  READ_WRITE 
GO




------------------------------------------------------------------------------

-- View of courses with enrollment counts and max capacity
CREATE VIEW vw_CoursesWithEnrollment AS
SELECT
    c.CourseId,
    c.Title,
    c.CourseCode,
    c.CurrentEnrollment,
    c.MaxEnrollment
FROM Courses c;
GO

-- View of active academic terms with course counts
CREATE VIEW vw_ActiveTermsWithCourseCounts AS
SELECT
    at.AcademicTermId,
    at.TermName,
    at.StartDate,
    at.EndDate,
    COUNT(c.CourseId) AS CourseCount
FROM AcademicTerms at
LEFT JOIN Courses c ON at.AcademicTermId = c.AcademicTermId
WHERE at.IsActive = 1
GROUP BY at.AcademicTermId, at.TermName, at.StartDate, at.EndDate;
GO

-- View of quizzes with total questions and attempts
CREATE VIEW vw_QuizzesWithStats AS
SELECT
    q.QuizId,
    q.Title,
    c.Title AS CourseTitle,
    (SELECT COUNT(*) FROM QuizQuestions qq WHERE qq.QuizId = q.QuizId) AS QuestionCount,
    (SELECT COUNT(*) FROM QuizAttempts qa WHERE qa.QuizId = q.QuizId) AS AttemptCount
FROM Quizzes q
JOIN Courses c ON q.CourseId = c.CourseId;
GO

-- View of student grades across all courses
CREATE VIEW vw_AllStudentGrades AS
SELECT * FROM vw_StudentCourseGrades;
GO

-- View of notifications with user details and read status
CREATE VIEW vw_NotificationsWithUserDetails AS
SELECT
    n.NotificationId,
    n.Message,
    n.IsRead,
    n.CreatedAt,
    u.UserId,
    u.FirstName,
    u.LastName,
    u.Email
FROM Notifications n
JOIN Users u ON n.UserId = u.UserId;
GO

-- View of AI interactions by user and course
CREATE VIEW vw_AIInteractionsByUser AS
SELECT
    ai.InteractionId,
    ai.UserMessage,
    ai.AIResponse,
    ai.CreatedAt,
    u.UserId,
    u.FirstName,
    u.LastName,
    am.ModelName
FROM AIInteractions ai
JOIN Users u ON ai.UserId = u.UserId
JOIN AIModels am ON ai.AIModelId = am.AIModelId;
GO


-- View of lessons with asset counts
CREATE VIEW vw_LessonsWithAssetCounts AS
SELECT
    l.LectureId,
    l.Title,
    l.CourseId,
    (SELECT COUNT(*) FROM LearningAssets la WHERE la.LectureId = l.LectureId) AS AssetCount
FROM Lectures l;
GO

-- View of student performance in quizzes (average score, attempts)
CREATE VIEW vw_StudentQuizPerformance AS
SELECT
    UserId,
    QuizId,
    COUNT(AttemptId) AS NumberOfAttempts,
    AVG(Score) AS AverageScore
FROM QuizAttempts
WHERE Score IS NOT NULL
GROUP BY UserId, QuizId;
GO


-- View of users with last login and activity status
CREATE VIEW vw_UsersWithActivity AS
SELECT
    UserId,
    FirstName,
    LastName,
    Email,
    Role,
    IsActive,
    LastLoginAt
FROM Users;
GO

-- View of AI-generated quizzes and related questions
CREATE VIEW vw_AIGeneratedQuizzes AS
SELECT
    q.QuizId,
    q.Title AS QuizTitle,
    qq.QuestionId,
    qq.QuestionText
FROM Quizzes q
JOIN QuizQuestions qq ON q.QuizId = qq.QuizId
JOIN AIGeneratedContent agc ON q.CourseId = agc.CourseId; -- Simplistic join
GO

-- View of gradebook with grades per student
CREATE VIEW vw_GradeBookDetails AS
SELECT
    gb.GradeBookId,
    c.Title AS CourseTitle,
    u.UserId,
    u.FirstName,
    u.LastName,
    g.GradableItemType,
    g.Points,
    g.MaxPoints
FROM GradeBooks gb
JOIN Courses c ON gb.CourseId = c.CourseId
JOIN Grades g ON gb.GradeBookId = g.GradeBookId
JOIN Users u ON g.UserId = u.UserId;
GO

-- View of notification templates with variables
CREATE VIEW vw_NotificationTemplatesWithTypes AS
SELECT
    nt.TemplateId,
    nt.TemplateName,
    nt.MessageFormat,
    nty.TypeName AS NotificationType
FROM NotificationTemplates nt
JOIN NotificationTypes nty ON nt.NotificationTypeId = nty.NotificationTypeId;
GO

-- View of institution summary (users, courses, terms)
CREATE VIEW vw_InstitutionSummary AS
SELECT
    i.InstitutionId,
    i.Name,
    (SELECT COUNT(*) FROM Users u WHERE u.InstitutionId = i.InstitutionId) AS UserCount,
    (SELECT COUNT(*) FROM Courses c JOIN AcademicTerms at ON c.AcademicTermId = at.AcademicTermId WHERE at.InstitutionId = i.InstitutionId) AS CourseCount,
    (SELECT COUNT(*) FROM AcademicTerms at WHERE at.InstitutionId = i.InstitutionId) AS TermCount
FROM Institutions i;
GO

-- View of courses with pass/fail rate
CREATE VIEW vw_CoursePassFailRate AS
SELECT
    ce.CourseId,
    SUM(CASE WHEN ce.FinalGrade >= 50 THEN 1 ELSE 0 END) AS PassCount,
    SUM(CASE WHEN ce.FinalGrade < 50 THEN 1 ELSE 0 END) AS FailCount,
    COUNT(ce.UserId) AS TotalCompletedStudents,
    CASE WHEN COUNT(ce.UserId) > 0 THEN (SUM(CASE WHEN ce.FinalGrade >= 50 THEN 1.0 ELSE 0.0 END) / COUNT(ce.UserId)) * 100 ELSE 0 END AS PassRate
FROM CourseEnrollments ce
WHERE ce.Status = 'Completed' AND ce.FinalGrade IS NOT NULL
GROUP BY ce.CourseId;
GO

-- View of quiz responses with correctness and points earned
CREATE VIEW vw_QuizResponsesWithCorrectness AS
SELECT
    qr.ResponseId,
    qr.AttemptId,
    qq.QuestionText,
    qo.OptionText AS SelectedOption,
    qo.IsCorrect,
    CASE WHEN qo.IsCorrect = 1 THEN qq.Points ELSE 0 END AS PointsEarned
FROM QuizResponses qr
JOIN QuizQuestions qq ON qr.QuestionId = qq.QuestionId
LEFT JOIN QuestionOptions qo ON qr.SelectedOptionId = qo.OptionId;
GO

-- View of top performing students in institution
CREATE VIEW vw_TopPerformingStudents AS
SELECT TOP 100
    sp.UserId,
    u.FirstName,
    u.LastName,
    sp.GPA
FROM StudentProfiles sp
JOIN Users u ON sp.UserId = u.UserId
WHERE sp.GPA IS NOT NULL
ORDER BY sp.GPA DESC;
GO

-- View of active vs inactive AI models
CREATE VIEW vw_AIModelStatus AS
SELECT ModelName, IsActive FROM AIModels;
GO

-- View of courses with instructor and term info
CREATE VIEW vw_CoursesWithInstructorAndTerm AS
SELECT
    c.CourseId,
    c.Title AS CourseTitle,
    c.CourseCode,
    u.FirstName + ' ' + u.LastName AS InstructorName,
    at.TermName
FROM Courses c
JOIN Users u ON c.InstructorId = u.UserId
JOIN AcademicTerms at ON c.AcademicTermId = at.AcademicTermId;
GO



-- Indexing foreign keys and commonly queried columns is crucial for performance.
CREATE INDEX IX_Users_InstitutionId ON Users(InstitutionId);
CREATE INDEX IX_AcademicTerms_InstitutionId ON AcademicTerms(InstitutionId);
CREATE INDEX IX_Courses_InstructorId ON Courses(InstructorId);
CREATE INDEX IX_Courses_AcademicTermId ON Courses(AcademicTermId);
CREATE INDEX IX_CourseEnrollments_UserId ON CourseEnrollments(UserId);
CREATE INDEX IX_CourseEnrollments_CourseId ON CourseEnrollments(CourseId);
CREATE INDEX IX_Lessons_CourseId ON Lectures(CourseId);
CREATE INDEX IX_LearningAssets_LessonId ON LearningAssets(LectureId);
CREATE INDEX IX_Quizzes_CourseId ON Quizzes(CourseId);
CREATE INDEX IX_QuizQuestions_QuizId ON QuizQuestions(QuizId);
CREATE INDEX IX_QuestionOptions_QuestionId ON QuestionOptions(QuestionId);
CREATE INDEX IX_QuizAttempts_QuizId ON QuizAttempts(QuizId);
CREATE INDEX IX_QuizAttempts_UserId ON QuizAttempts(UserId);
CREATE INDEX IX_QuizResponses_AttemptId ON QuizResponses(AttemptId);
CREATE INDEX IX_Grades_UserId ON Grades(UserId);
CREATE INDEX IX_Grades_GradeBookId ON Grades(GradeBookId);
CREATE INDEX IX_Notifications_UserId ON Notifications(UserId);
GO



-------------------------------------------------------------------


-- Procedure to create a new user and their corresponding profile
CREATE PROCEDURE sp_CreateUser
    @InstitutionId INT,
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(MAX),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Role NVARCHAR(50),
    @StudentIdNumber NVARCHAR(50) = NULL, -- Optional for students
    @Department NVARCHAR(100) = NULL -- Optional for instructors
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UserId INT;

    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        RAISERROR ('Email already exists.', 16, 1);
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO Users (InstitutionId, Email, PasswordHash, FirstName, LastName, Role)
        VALUES (@InstitutionId, @Email, @PasswordHash, @FirstName, @LastName, @Role);
        
        SET @UserId = SCOPE_IDENTITY();

        IF @Role = 'Student'
        BEGIN
            INSERT INTO StudentProfiles (UserId, StudentIdNumber) VALUES (@UserId, @StudentIdNumber);
        END
        ELSE IF @Role = 'Instructor'
        BEGIN
            INSERT INTO InstructorProfiles (UserId, Department) VALUES (@UserId, @Department);
        END

        COMMIT TRANSACTION;
        SELECT @UserId AS NewUserId;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO


-- Procedure to enroll a student in a course
CREATE PROCEDURE sp_EnrollStudent
    @UserId INT,
    @CourseId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if user is a student
    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId AND Role = 'Student')
    BEGIN
        RAISERROR ('User is not a student.', 16, 1);
        RETURN;
    END
    
    -- Check for duplicate enrollment
    IF EXISTS (SELECT 1 FROM CourseEnrollments WHERE UserId = @UserId AND CourseId = @CourseId)
    BEGIN
        RAISERROR ('Student is already enrolled in this course.', 16, 1);
        RETURN;
    END

    INSERT INTO CourseEnrollments (UserId, CourseId, Status)
    VALUES (@UserId, @CourseId, 'Enrolled');

    SELECT SCOPE_IDENTITY() AS NewEnrollmentId;
END
GO


-- Procedure to get all courses for a specific student
CREATE PROCEDURE sp_GetStudentCourses
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_StudentEnrollments WHERE UserId = @UserId;
END
GO

-- Procedure to get all students for a specific course
CREATE PROCEDURE sp_GetCourseRoster
    @CourseId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * FROM vw_StudentEnrollments WHERE CourseId = @CourseId;
END
GO


-- Procedure to start a quiz attempt for a student
CREATE PROCEDURE sp_StartQuizAttempt
    @UserId INT,
    @QuizId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Ensure student is enrolled in the course that contains the quiz
    IF NOT EXISTS (
        SELECT 1 
        FROM CourseEnrollments ce
        JOIN Quizzes q ON ce.CourseId = q.CourseId
        WHERE ce.UserId = @UserId AND q.QuizId = @QuizId
    )
    BEGIN
        RAISERROR ('Student is not enrolled in the course for this quiz.', 16, 1);
        RETURN;
    END

    INSERT INTO QuizAttempts (UserId, QuizId) VALUES (@UserId, @QuizId);
    SELECT SCOPE_IDENTITY() as NewAttemptId;
END
GO

-- Procedure to submit a quiz and calculate the score
CREATE PROCEDURE sp_SubmitQuizAttempt
    @AttemptId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TotalScore DECIMAL(5, 2) = 0;
    DECLARE @TotalPoints DECIMAL(5, 2) = 0;
    DECLARE @QuizId INT;
    DECLARE @UserId INT;

    SELECT @QuizId = QuizId, @UserId = UserId FROM QuizAttempts WHERE AttemptId = @AttemptId;

    -- Calculate score from correct multiple choice answers
    SELECT @TotalScore = ISNULL(SUM(q.Points), 0)
    FROM QuizResponses qr
    JOIN QuestionOptions qo ON qr.SelectedOptionId = qo.OptionId
    JOIN QuizQuestions q ON qr.QuestionId = q.QuestionId
    WHERE qr.AttemptId = @AttemptId AND qo.IsCorrect = 1;

    -- Get total possible points for the quiz
    SELECT @TotalPoints = SUM(Points) FROM QuizQuestions WHERE QuizId = @QuizId;

    -- Update the attempt with the score and submission time
    UPDATE QuizAttempts
    SET SubmittedAt = GETDATE(),
        Score = @TotalScore
    WHERE AttemptId = @AttemptId;

    -- Create a grade entry
    DECLARE @GradeBookId INT;
    SELECT @GradeBookId = gb.GradeBookId FROM GradeBooks gb JOIN Quizzes q ON gb.CourseId = q.CourseId WHERE q.QuizId = @QuizId;
    
    IF @GradeBookId IS NOT NULL
    BEGIN
        INSERT INTO Grades(GradeBookId, UserId, GradableItemType, GradableItemId, Points, MaxPoints)
        VALUES(@GradeBookId, @UserId, 'Quiz', @AttemptId, @TotalScore, @TotalPoints);
    END
    
    SELECT @TotalScore AS FinalScore, @TotalPoints AS MaxPoints;
END
GO


-- SP to assign grade to a quiz attempt
CREATE PROCEDURE sp_AssignGradeToQuizAttempt
    @AttemptId INT,
    @Points DECIMAL(5,2),
    @MaxPoints DECIMAL(5,2),
    @Comments NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @UserId INT, @QuizId INT, @CourseId INT, @GradeBookId INT;

    SELECT @UserId = qa.UserId, @QuizId = qa.QuizId, @CourseId = q.CourseId 
    FROM QuizAttempts qa JOIN Quizzes q ON qa.QuizId = q.QuizId
    WHERE qa.AttemptId = @AttemptId;

    SELECT @GradeBookId = GradeBookId FROM GradeBooks WHERE CourseId = @CourseId;

    IF @GradeBookId IS NOT NULL
    BEGIN
        -- Insert or update grade
        MERGE Grades AS target
        USING (SELECT @AttemptId) AS source (GradableItemId)
        ON (target.GradableItemId = source.GradableItemId AND target.GradableItemType = 'Quiz')
        WHEN MATCHED THEN
            UPDATE SET Points = @Points, MaxPoints = @MaxPoints, Comments = @Comments, GradedAt = GETDATE()
        WHEN NOT MATCHED THEN
            INSERT (GradeBookId, UserId, GradableItemType, GradableItemId, Points, MaxPoints, Comments)
            VALUES (@GradeBookId, @UserId, 'Quiz', @AttemptId, @Points, @MaxPoints, @Comments);
    END
END
GO

-- SP to create new academic term and deactivate old ones
CREATE PROCEDURE sp_CreateAcademicTerm
    @InstitutionId INT,
    @TermName NVARCHAR(100),
    @StartDate DATE,
    @EndDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE AcademicTerms SET IsActive = 0 WHERE InstitutionId = @InstitutionId AND IsActive = 1;
    INSERT INTO AcademicTerms (InstitutionId, TermName, StartDate, EndDate, IsActive)
    VALUES (@InstitutionId, @TermName, @StartDate, @EndDate, 1);
END
GO


-- SP to send notification to a user
CREATE PROCEDURE sp_SendNotification
    @UserId INT,
    @Message NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO Notifications (UserId, Message) VALUES (@UserId, @Message);
END
GO

-- SP to mark notification as read
CREATE PROCEDURE sp_MarkNotificationAsRead
    @NotificationId INT,
    @UserId INT -- For security check
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Notifications SET IsRead = 1 
    WHERE NotificationId = @NotificationId AND UserId = @UserId;
END
GO

-- SP to add instructor to a course
CREATE PROCEDURE sp_AssignInstructorToCourse
    @InstructorId INT,
    @CourseId INT
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM Users WHERE UserId = @InstructorId AND Role = 'Instructor')
    BEGIN
        RAISERROR ('User is not an instructor.', 16, 1);
        RETURN;
    END
    UPDATE Courses SET InstructorId = @InstructorId WHERE CourseId = @CourseId;
END
GO


-- SP to record user login timestamp
CREATE PROCEDURE sp_RecordUserLogin
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET LastLoginAt = GETDATE() WHERE UserId = @UserId;
END
GO


-- SP to update course status
CREATE PROCEDURE sp_UpdateCourseStatus
    @CourseId INT,
    @Status NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Courses SET Status = @Status WHERE CourseId = @CourseId;
END
GO





-- SP to update a student's GPA after a new grade is entered
CREATE PROCEDURE sp_UpdateStudentGPA
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    -- NOTE: This is a simplified calculation. A real GPA calculation needs to factor in quality points (Grade * Credits).
    DECLARE @NewGPA DECIMAL(4,3);
    
    SELECT @NewGPA = AVG(g.Percentage / 25.0) -- Simple conversion of percentage to 4.0 scale
    FROM Grades g
    WHERE g.UserId = @UserId;

    UPDATE StudentProfiles SET GPA = @NewGPA WHERE UserId = @UserId;
END
GO

-- SP to archive courses that ended more than 6 months ago
CREATE PROCEDURE sp_ArchiveInactiveCourses
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE c
    SET c.Status = 'Archived'
    FROM Courses c
    JOIN AcademicTerms at ON c.AcademicTermId = at.AcademicTermId
    WHERE at.EndDate < DATEADD(month, -6, GETDATE()) AND c.Status = 'Completed';
END
GO

-- SP to deactivate users who have not logged in for a specified number of months
CREATE PROCEDURE sp_DeactivateInactiveUsers
    @MonthsOfInactivity INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users
    SET IsActive = 0
    WHERE LastLoginAt < DATEADD(month, -@MonthsOfInactivity, GETDATE())
      AND IsActive = 1;
END
GO


-- SP to create a new AI model entry
CREATE PROCEDURE sp_CreateAIModel
    @ModelName NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO AIModels (ModelName, IsActive) VALUES (@ModelName, 1);
    SELECT SCOPE_IDENTITY() AS NewAIModelId;
END
GO

-- SP to reset a user's password
CREATE PROCEDURE sp_ResetUserPassword
    @UserId INT,
    @NewPasswordHash NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Users SET PasswordHash = @NewPasswordHash, UpdatedAt = GETDATE() WHERE UserId = @UserId;
END
GO







---------------------------------------------------------------------------


-- Trigger to automatically update the enrollment count on the Courses table
CREATE TRIGGER trg_UpdateCourseEnrollmentCount
ON CourseEnrollments
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @CourseId INT;
    
    -- Get CourseId from both inserted and deleted pseudo-tables
    SELECT @CourseId = ISNULL(i.CourseId, d.CourseId)
    FROM inserted i
    FULL OUTER JOIN deleted d ON i.EnrollmentId = d.EnrollmentId;

    IF @CourseId IS NOT NULL
    BEGIN
        UPDATE Courses
        SET CurrentEnrollment = (SELECT COUNT(*) FROM CourseEnrollments WHERE CourseId = @CourseId AND Status = 'Enrolled')
        WHERE CourseId = @CourseId;
    END
END
GO

-- Trigger to automatically create a GradeBook when a new Course is created
CREATE TRIGGER trg_CreateGradeBookForCourse
ON Courses
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO GradeBooks (CourseId, Name)
    SELECT i.CourseId, i.Title + ' Grade Book'
    FROM inserted i;
END
GO


-- Trigger to update GPA when a new grade is inserted or updated.
CREATE TRIGGER trg_UpdateGPAOnGradeChange
ON Grades
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    -- This is a placeholder for complex GPA logic.
    -- A real implementation would involve a dedicated SP called from here.
    PRINT 'GPA needs recalculation for the affected student.';
END
GO

-- Trigger to mark enrollment as completed when a final grade is entered.
CREATE TRIGGER trg_CompleteEnrollmentOnFinalGrade
ON CourseEnrollments
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(FinalGrade)
    BEGIN
        UPDATE CourseEnrollments
        SET Status = 'Completed'
        FROM inserted i
        WHERE CourseEnrollments.EnrollmentId = i.EnrollmentId AND i.FinalGrade IS NOT NULL;
    END
END
GO


-- Trigger to deactivate users when their institution is deactivated
CREATE TRIGGER trg_DeactivateUsersOnInstitutionDeactivation
ON Institutions
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF UPDATE(IsActive)
    BEGIN
        UPDATE u
        SET u.IsActive = 0
        FROM Users u
        JOIN inserted i ON u.InstitutionId = i.InstitutionId
        WHERE i.IsActive = 0;
    END
END
GO




--------------------------------------------------------------------

-- Function to get the full name of a user
CREATE FUNCTION fn_GetUserFullName (@UserId INT)
RETURNS NVARCHAR(201)
AS
BEGIN
    DECLARE @FullName NVARCHAR(201);
    SELECT @FullName = FirstName + ' ' + LastName FROM Users WHERE UserId = @UserId;
    RETURN @FullName;
END
GO

-- Function to get the course enrollment count
CREATE FUNCTION fn_GetCourseEnrollmentCount (@CourseId INT)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;
    SELECT @Count = CurrentEnrollment FROM Courses WHERE CourseId = @CourseId;
    RETURN @Count;
END
GO

-- Function to calculate a percentage score
CREATE FUNCTION fn_CalculatePercentageScore (@Points DECIMAL(5,2), @MaxPoints DECIMAL(5,2))
RETURNS DECIMAL(5,2)
AS
BEGIN
    IF @MaxPoints = 0 RETURN 0;
    RETURN (@Points / @MaxPoints) * 100;
END
GO

-- Function to get the number of quizzes in a course
CREATE FUNCTION fn_GetQuizCountInCourse (@CourseId INT)
RETURNS INT
AS
BEGIN
    RETURN (SELECT COUNT(*) FROM Quizzes WHERE CourseId = @CourseId);
END
GO

-- Function to check if a quiz is overdue
CREATE FUNCTION fn_IsQuizOverdue (@QuizId INT)
RETURNS BIT
AS
BEGIN
    DECLARE @IsOverdue BIT = 0;
    IF EXISTS (SELECT 1 FROM Quizzes WHERE QuizId = @QuizId AND DueDate < GETDATE())
    BEGIN
        SET @IsOverdue = 1;
    END
    RETURN @IsOverdue;
END
GO

-- Function to return a letter grade from a percentage
CREATE FUNCTION fn_GetGradeLetterFromPercentage (@Percentage DECIMAL(5,2))
RETURNS NVARCHAR(2)
AS
BEGIN
    RETURN CASE
        WHEN @Percentage >= 90 THEN 'A'
        WHEN @Percentage >= 80 THEN 'B'
        WHEN @Percentage >= 70 THEN 'C'
        WHEN @Percentage >= 60 THEN 'D'
        ELSE 'F'
    END;
END
GO

-- Function to get the number of unread notifications for a user
CREATE FUNCTION fn_GetUnreadNotificationCount (@UserId INT)
RETURNS INT
AS
BEGIN
    RETURN (SELECT COUNT(*) FROM Notifications WHERE UserId = @UserId AND IsRead = 0);
END
GO

-- Function to get term duration in weeks
CREATE FUNCTION fn_GetTermDurationInWeeks (@AcademicTermId INT)
RETURNS INT
AS
BEGIN
    DECLARE @Weeks INT;
    SELECT @Weeks = DATEDIFF(week, StartDate, EndDate) FROM AcademicTerms WHERE AcademicTermId = @AcademicTermId;
    RETURN @Weeks;
END
GO

-- Function to check if an AI model is active
CREATE FUNCTION fn_IsAIModelActive (@AIModelId INT)
RETURNS BIT
AS
BEGIN
    RETURN (SELECT IsActive FROM AIModels WHERE AIModelId = @AIModelId);
END
GO


-- Function to get the count of active courses in an institution
CREATE FUNCTION fn_CountActiveCoursesInInstitution (@InstitutionId INT)
RETURNS INT
AS
BEGIN
    RETURN (SELECT COUNT(*) FROM Courses c JOIN AcademicTerms at ON c.AcademicTermId = at.AcademicTermId WHERE at.InstitutionId = @InstitutionId AND c.Status = 'Active');
END
GO


-- Function to calculate the average quiz score for a user in a course
CREATE FUNCTION fn_GetAverageQuizScoreForUserInCourse (@UserId INT, @CourseId INT)
RETURNS DECIMAL(5,2)
AS
BEGIN
    RETURN (SELECT AVG(Score) FROM QuizAttempts qa JOIN Quizzes q ON qa.QuizId = q.QuizId WHERE qa.UserId = @UserId AND q.CourseId = @CourseId AND qa.Score IS NOT NULL);
END
GO

-- Function to get the total number of instructors in an institution
CREATE FUNCTION fn_GetTotalInstructorsInInstitution (@InstitutionId INT)
RETURNS INT
AS
BEGIN
    RETURN (SELECT COUNT(*) FROM Users WHERE InstitutionId = @InstitutionId AND Role = 'Instructor');
END
GO

-- Table-Valued Function to return active notification templates for a given type
CREATE FUNCTION fn_GetActiveNotificationTemplatesByType (@NotificationTypeId INT)
RETURNS TABLE
AS
RETURN (
    SELECT nt.* FROM NotificationTemplates nt
    JOIN NotificationTypes nty ON nt.NotificationTypeId = nty.NotificationTypeId
    WHERE nty.NotificationTypeId = @NotificationTypeId AND nty.IsActive = 1
);
GO

