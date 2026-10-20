using System.Net;
using Learning_Management_System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Lms.IntegrationTests;

public class PreviewAccessTests
{
    private static readonly string[] ProbeUrls =
    {
        "/preview/view?name=Dashboard",
        "/preview/view?name=../Shared/Error",
        "/preview/view",
    };

    static PreviewAccessTests()
    {
        Environment.SetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection",
            TestDatabase.ConnectionString);
    }

    public static IEnumerable<object[]> EnvironmentsAndUrls()
    {
        foreach (var environment in new[] { "Development", "Production" })
        {
            foreach (var url in ProbeUrls)
            {
                yield return new object[] { environment, url };
            }
        }
    }

    [Theory]
    [MemberData(nameof(EnvironmentsAndUrls))]
    public async Task Preview_urls_return_not_found(string environment, string url)
    {
        using var factory = CreateFactory(environment);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(url);

        Assert.True(
            response.StatusCode == HttpStatusCode.NotFound,
            $"Expected 404 for {url} in {environment}, got {(int)response.StatusCode} " +
            $"{response.StatusCode}.");
    }

    private static WebApplicationFactory<Program> CreateFactory(string environment)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.UseSetting(
                "ConnectionStrings:DefaultConnection",
                TestDatabase.ConnectionString);
        });
    }
}
