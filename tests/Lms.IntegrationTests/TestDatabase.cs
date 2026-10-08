using Learning_Management_System.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Lms.IntegrationTests;

public static class TestDatabase
{
    /// <summary>
    /// Connection string for the real SQL Server test database.
    /// Set LMS_TEST_CONNECTION to override; defaults to the local dev database
    /// created by docs/schema/LMS Schema.sql.
    /// </summary>
    public static string ConnectionString { get; } =
        Environment.GetEnvironmentVariable("LMS_TEST_CONNECTION")
        ?? "Server=.;Database=LMS;Trusted_Connection=True;TrustServerCertificate=True";

    public static LmsContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LmsContext>()
            .UseSqlServer(ConnectionString)
            .Options;

        return new LmsContext(options);
    }
}
