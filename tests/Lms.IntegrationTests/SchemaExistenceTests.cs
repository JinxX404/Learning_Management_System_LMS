using System.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Lms.IntegrationTests;

public class SchemaExistenceTests
{
    private static readonly string[] RequiredObjects =
    {
        "dbo.Users",
        "dbo.Courses",
        "dbo.CourseEnrollments",
        "dbo.QuizAttempts",
        "dbo.sp_SubmitQuizAttempt",
        "dbo.vw_InstitutionSummary",
    };

    [Fact]
    public async Task Deployed_schema_contains_core_tables_procedures_and_views()
    {
        await using var context = TestDatabase.CreateContext();

        var canConnect = await context.Database.CanConnectAsync();
        Assert.True(
            canConnect,
            $"Cannot connect to the test database. Run docs/schema/LMS Schema.sql against your SQL Server, " +
            $"or set LMS_TEST_CONNECTION to a database created from that script. " +
            $"Connection: {Redact(TestDatabase.ConnectionString)}");

        var connection = context.Database.GetDbConnection();
        await connection.OpenAsync();

        var missing = new List<string>();
        foreach (var objectName in RequiredObjects)
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT OBJECT_ID(@name);";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "@name";
            parameter.Value = objectName;
            command.Parameters.Add(parameter);

            var result = await command.ExecuteScalarAsync();
            if (result is null or DBNull)
            {
                missing.Add(objectName);
            }
        }

        await connection.CloseAsync();

        Assert.True(
            missing.Count == 0,
            $"Schema is incomplete. Missing objects: {string.Join(", ", missing)}. " +
            $"Re-run docs/schema/LMS Schema.sql (it is the schema source of truth; the app only calls EnsureCreatedAsync).");
    }

    [Fact]
    public async Task Application_can_query_users_through_ef_core()
    {
        await using var context = TestDatabase.CreateContext();

        var canConnect = await context.Database.CanConnectAsync();
        Assert.True(canConnect, $"Cannot connect to the test database: {Redact(TestDatabase.ConnectionString)}");

        _ = await context.Users.AsNoTracking().AnyAsync();
    }

    private static string Redact(string connectionString)
    {
        return string.Join(
            ";",
            connectionString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Where(part => !part.Contains("Password", StringComparison.OrdinalIgnoreCase)));
    }
}
