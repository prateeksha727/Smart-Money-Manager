using SmartMoneyManager.Models;

namespace SmartMoneyManager.Data
{
    public static class SessionManager
    {
        public static User? CurrentUser { get; private set; }
        public static int   UserId      => CurrentUser?.Id ?? 0;
        public static bool  IsLoggedIn  => CurrentUser != null;
        public static void  Login(User u) => CurrentUser = u;
        public static void  Logout()      => CurrentUser = null;
    }
}
