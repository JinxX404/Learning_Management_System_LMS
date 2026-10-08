using Xunit;

namespace Lms.Tests;

public class PasswordHashingTests
{
    private const string Password = "S3cure!Passw0rd";

    [Fact]
    public void Hashed_password_verifies_against_original()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(Password);

        Assert.True(BCrypt.Net.BCrypt.Verify(Password, hash));
    }

    [Fact]
    public void Wrong_password_fails_verification()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(Password);

        Assert.False(BCrypt.Net.BCrypt.Verify("WrongPassword1!", hash));
    }

    [Fact]
    public void Stored_hash_never_contains_the_plaintext_password()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword(Password);

        Assert.StartsWith("$2", hash);
        Assert.DoesNotContain(Password, hash);
    }

    [Fact]
    public void Each_hash_uses_a_unique_salt()
    {
        var first = BCrypt.Net.BCrypt.HashPassword(Password);
        var second = BCrypt.Net.BCrypt.HashPassword(Password);

        Assert.NotEqual(first, second);
        Assert.True(BCrypt.Net.BCrypt.Verify(Password, first));
        Assert.True(BCrypt.Net.BCrypt.Verify(Password, second));
    }
}
// trailing space test   
