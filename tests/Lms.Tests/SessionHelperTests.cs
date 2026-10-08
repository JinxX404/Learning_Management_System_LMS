using Learning_Management_System.Helpers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Lms.Tests;

public class SessionHelperTests
{
    [Fact]
    public void SetUserId_then_GetUserId_returns_the_same_user()
    {
        var session = new InMemorySession();

        SessionHelper.SetUserId(session, 42);

        Assert.Equal(42, SessionHelper.GetUserId(session));
    }

    [Fact]
    public void GetUserId_returns_null_when_no_user_is_stored()
    {
        var session = new InMemorySession();

        Assert.Null(SessionHelper.GetUserId(session));
    }

    [Fact]
    public void ClearSession_removes_the_stored_user()
    {
        var session = new InMemorySession();
        SessionHelper.SetUserId(session, 7);

        SessionHelper.ClearSession(session);

        Assert.Null(SessionHelper.GetUserId(session));
    }

    private sealed class InMemorySession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();

        public string Id { get; } = Guid.NewGuid().ToString();

        public bool IsAvailable { get; set; } = true;

        public IEnumerable<string> Keys => _store.Keys;

        public void Clear() => _store.Clear();

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

        public void Remove(string key) => _store.Remove(key);

        public void Set(string key, byte[] value) => _store[key] = value;

        public bool TryGetValue(string key, out byte[] value)
        {
            if (_store.TryGetValue(key, out var stored))
            {
                value = stored;
                return true;
            }

            value = Array.Empty<byte>();
            return false;
        }
    }
}
