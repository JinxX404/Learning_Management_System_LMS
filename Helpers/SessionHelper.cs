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

